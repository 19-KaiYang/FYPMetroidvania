using JetBrains.Annotations;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatZone : MonoBehaviour
{
    [Header("Components")]
    public BoxCollider2D areaTrigger;
    public bool detecting = true;

    [Header("Enemy Waves")]
    public List<EnemyWave> EnemyWaveList = new List<EnemyWave>();
    public float waveWaitTime = 1f;

    [Header("Level Triggers")]
    public GameObject door;

    // Private trackers
    private int currWave;
    public List<List<GameObject>> enemyObjects = new();

    private void Start()
    {
        InitialiseEnemies();
    }

    void InitialiseEnemies()
    {
        foreach(EnemyWave wave in EnemyWaveList)
        {
            List<GameObject> objects = new List<GameObject>();
            enemyObjects.Add(objects);
            foreach(EnemySpawn spawn in wave.EnemyList)
            {
                GameObject go = Instantiate(spawn.enemy);
                go.transform.SetParent(this.transform);
                go.transform.position = spawn.spawnPoint.position;
                objects.Add(go);

                Enemy enemy = go.GetComponent<Enemy>();
                if (enemy != null)
                {
                    enemy.player = PlayerController.instance;
                    enemy.enabled = true;
                }

                go.SetActive(false);
            }
        }
    }

    void BeginCombat()
    {
        currWave = -1;
        SpawnNextWave();
    }
    void SpawnNextWave()
    {
        currWave++;
        if (currWave >= EnemyWaveList.Count)
        {
            CombatOver();
            return;
        }

        for(int i = 0; i < enemyObjects[currWave].Count; i++)
        {
            GameObject enemyObj = enemyObjects[currWave][i];
            Health tracker = enemyObj.GetComponent<Health>();
            if (tracker != null) tracker.enemyDeath += OnEnemyDeath;

            EnemySpawn spawn = EnemyWaveList[currWave].EnemyList[i];
            StartCoroutine(SpawnEnemyCoroutine(spawn, enemyObj));
        }
    }
    void CombatOver()
    {
        Debug.Log("Combat zone completed! Opening door...");

        areaTrigger.enabled = false;
        door.SetActive(false);
    }

    IEnumerator SpawnEnemyCoroutine(EnemySpawn spawn, GameObject enemyObj)
    {
        if(spawn.spawnVFX != null && spawn.spawnTime > 0)
        {
            GameObject vfx = Instantiate(spawn.spawnVFX, spawn.spawnPoint.position, Quaternion.identity);
            yield return new WaitForSeconds(spawn.spawnTime);
            Destroy(vfx);
        }

        enemyObj.SetActive(true);
    }

    public void OnEnemyDeath(GameObject enemy)
    {
        if(enemy == null) return;
        int index = enemyObjects[currWave].IndexOf(enemy);
        enemyObjects[currWave].RemoveAt(index);
        if (enemyObjects[currWave].Count <= 0) StartCoroutine(NewWaveCoroutine());
    }

    IEnumerator NewWaveCoroutine()
    {
        yield return new WaitForSeconds(waveWaitTime);
        SpawnNextWave();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!detecting) return;
        if(collision.CompareTag("Player"))
        {
            detecting = false;
            BeginCombat();
        }
    }
}

[Serializable]
public class EnemyWave
{
    public List<EnemySpawn> EnemyList = new List<EnemySpawn>();
}

[Serializable]
public class EnemySpawn
{
    public GameObject enemy;
    public Transform spawnPoint;
    public float spawnTime = 1f;
    public GameObject spawnVFX;
}


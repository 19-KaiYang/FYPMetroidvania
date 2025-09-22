using UnityEngine;
using System.Collections.Generic;

public class ProjectileManager : MonoBehaviour
{
    public static ProjectileManager instance;

    [Header("Projectile Prefabs")]
    public GameObject swordSlashPrefab;
    public GameObject gauntletPrefab;
    public GameObject gauntletChargePrefab;

    private Dictionary<GameObject, Queue<GameObject>> pools = new Dictionary<GameObject, Queue<GameObject>>();

    private void Awake()
    {
        if (instance == null) instance = this;
    }

    private GameObject GetFromPool(GameObject prefab, Vector3 pos, Quaternion rot)
    {
        if (!pools.ContainsKey(prefab))
            pools[prefab] = new Queue<GameObject>();

        GameObject obj;

        if (pools[prefab].Count > 0)
        {
            obj = pools[prefab].Dequeue();
            obj.transform.SetPositionAndRotation(pos, rot);
            obj.SetActive(true);
        }
        else
        {
            obj = Instantiate(prefab, pos, rot);
        }

        return obj;
    }

    public void ReturnToPool(GameObject obj)
    {
        foreach (var kvp in pools)
        {
            if (obj.name.Contains(kvp.Key.name)) // match prefab name
            {
                pools[kvp.Key].Enqueue(obj);
                return;
            }
        }
    }

    // === Typed Spawns ===
    public SwordSlashProjectile SpawnSwordSlash(Vector3 pos, Quaternion rot)
    {
        var go = GetFromPool(swordSlashPrefab, pos, rot);
        var proj = go.GetComponent<SwordSlashProjectile>();
        return proj;
    }

    public GauntletProjectile SpawnGauntlet(Vector3 pos, Quaternion rot)
    {
        var go = GetFromPool(gauntletPrefab, pos, rot);
        return go.GetComponent<GauntletProjectile>();
    }

    public GauntletChargeProjectile SpawnGauntletCharge(Vector3 pos, Quaternion rot)
    {
        var go = GetFromPool(gauntletChargePrefab, pos, rot);
        return go.GetComponent<GauntletChargeProjectile>();
    }

    // Clear all active projectiles
    public void ClearAll()
    {
        foreach (var kvp in pools)
        {
            foreach (var obj in kvp.Value)
                obj.SetActive(false);
        }
    }
}

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

    // Track which prefab each instance came from
    private Dictionary<GameObject, GameObject> instanceToPrefab = new Dictionary<GameObject, GameObject>();

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
            instanceToPrefab[obj] = prefab; // Track which prefab this came from
        }

        return obj;
    }

    public void ReturnToPool(GameObject obj)
    {
        obj.SetActive(false);

        // Use our tracking dictionary to find the correct prefab
        if (instanceToPrefab.TryGetValue(obj, out GameObject prefab))
        {
            if (pools.ContainsKey(prefab))
            {
                pools[prefab].Enqueue(obj);
                return;
            }
        }

        // Fallback: try to match by component type (your original method)
        foreach (var kvp in pools)
        {
            if (obj.TryGetComponent(out ProjectileBase proj) &&
                kvp.Key.GetComponent<ProjectileBase>().GetType() == proj.GetType())
            {
                instanceToPrefab[obj] = kvp.Key; // Update our tracking
                pools[kvp.Key].Enqueue(obj);
                return;
            }
        }

        // If we get here, something went wrong - destroy the object to prevent leaks
        Debug.LogWarning($"Failed to return {obj.name} to pool, destroying instead");
        Destroy(obj);
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

    private void OnDestroy()
    {
        instanceToPrefab.Clear();
    }
}
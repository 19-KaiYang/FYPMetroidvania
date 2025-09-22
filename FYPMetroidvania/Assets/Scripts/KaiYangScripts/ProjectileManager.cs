using System.Collections.Generic;
using UnityEngine;

public class ProjectileManager : MonoBehaviour
{
    public static ProjectileManager instance;

    [Header("Projectile Prefabs")]
    public GameObject basicProjectilePrefab;
    public GameObject swordSlashPrefab;
    public GameObject gauntletPrefab;
    public GameObject gauntletChargePrefab;

    private List<GameObject> activeProjectiles = new List<GameObject>();

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    // ===== Generic spawn =====
    public T SpawnProjectile<T>(T prefab, Vector3 position, Quaternion rotation) where T : Projectile
    {
        T proj = Instantiate(prefab, position, rotation);
        Register(proj.gameObject);
        return proj;
    }

    // ===== Typed spawns (shortcut functions) =====
    public SwordSlashProjectile SpawnSwordSlash(Vector3 position, Quaternion rotation)
    {
        var go = Instantiate(swordSlashPrefab, position, rotation);
        var proj = go.GetComponent<SwordSlashProjectile>();
        Register(proj.gameObject);
        return proj;
    }

    public GauntletProjectile SpawnGauntlet(Vector3 position, Quaternion rotation)
    {
        var go = Instantiate(gauntletPrefab, position, rotation);
        var proj = go.GetComponent<GauntletProjectile>();
        Register(proj.gameObject);
        return proj;
    }

    public GauntletChargeProjectile SpawnGauntletCharge(Vector3 position, Quaternion rotation)
    {
        var go = Instantiate(gauntletChargePrefab, position, rotation);
        var proj = go.GetComponent<GauntletChargeProjectile>();
        Register(proj.gameObject);
        return proj;
    }


    // ===== Management =====
    public void Register(GameObject proj)
    {
        if (!activeProjectiles.Contains(proj))
            activeProjectiles.Add(proj);
    }

    public void Unregister(GameObject proj)
    {
        if (activeProjectiles.Contains(proj))
            activeProjectiles.Remove(proj);
    }

    public void ClearAll()
    {
        foreach (var proj in activeProjectiles)
        {
            if (proj != null) Destroy(proj);
        }
        activeProjectiles.Clear();
    }
}

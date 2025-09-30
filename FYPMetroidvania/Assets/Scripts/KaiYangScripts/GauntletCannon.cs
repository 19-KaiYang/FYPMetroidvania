using UnityEngine;
using System.Collections;

public class GauntletCannon : MonoBehaviour
{
    public GameObject beamPrefab;
    public float chargeTime = 1.5f;

    private SpiritGauge spirit;
    private LayerMask enemyMask;
    private bool facingRight;

    public void Init(bool facingRight, SpiritGauge spirit, LayerMask enemyMask)
    {
        this.facingRight = facingRight;
        this.spirit = spirit;
        this.enemyMask = enemyMask;

        // Start firing sequence
        StartCoroutine(FireRoutine());
    }

    private IEnumerator FireRoutine()
    {
        yield return new WaitForSeconds(chargeTime);

        // Spawn beam in front of cannon
        Vector3 spawnPos = transform.position + new Vector3(facingRight ? 1f : -1f, 0f, 0f);
        GameObject beam = Instantiate(beamPrefab, spawnPos, Quaternion.identity);

        // Initialize beam
        //var beamScript = beam.GetComponent<GauntletBeam>();
        //if (beamScript != null && spirit != null)
        //    beamScript.Init(facingRight, spirit, enemyMask);

        Destroy(gameObject, 0.5f);
    }
}

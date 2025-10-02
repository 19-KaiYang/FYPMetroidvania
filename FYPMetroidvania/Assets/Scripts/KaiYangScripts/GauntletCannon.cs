using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GauntletCannon : MonoBehaviour
{
    [Header("Beam Settings")]
    public float chargeTime = 2f;
    public float damagePerTick = 10f;
    public float tickRate = 0.1f;
    public Vector2 beamSize = new Vector2(20f, 2f);
    public float beamVisualLength = 20f;
    public float cannonOffsetX = 0.5f;

    [Header("Visual")]
    public SpriteRenderer cannonSprite;
    public GameObject beamVisual;

    private SpiritGauge spirit;
    private LayerMask enemyMask;
    private bool facingRight;
    private bool isCharging = true;
    private bool isFiring = false;
    private float chargeTimer = 0f;
    private float beamDuration;

    private HashSet<int> hitThisTick = new HashSet<int>();

    public System.Action OnFinished; 

    private void Awake()
    {
        if (beamVisual != null)
            beamVisual.SetActive(false);
    }

    public void Init(bool playerFacingRight, SpiritGauge spiritGauge, LayerMask enemyLayer,
                  float charge, float damage, float tick, Vector2 size, float duration)
    {
        facingRight = playerFacingRight;
        spirit = spiritGauge;
        enemyMask = enemyLayer;

        chargeTime = charge;
        damagePerTick = damage;
        tickRate = tick;
        beamSize = size;
        beamVisualLength = size.x;
        beamDuration = duration;


        isCharging = true;
        isFiring = false;
        chargeTimer = 0f;

        if (beamVisual != null)
            beamVisual.SetActive(false);

        StartCoroutine(ChargeSequence());
    }

    private IEnumerator ChargeSequence()
    {
        chargeTimer = 0f;
        while (chargeTimer < chargeTime)
        {
            chargeTimer += Time.deltaTime;
            yield return null;
        }
        FireBeam();
    }

    public void ManualOverrideFire()
    {
        if (isCharging && !isFiring)
        {
            StopAllCoroutines();
            FireBeam();
        }
    }

    public void FireBeam()
    {
        if (isFiring) return;

        isFiring = true;
        isCharging = false;

        if (beamVisual != null)
        {
            beamVisual.SetActive(true);

            float direction = facingRight ? 1f : -1f;
            beamVisual.transform.localPosition = new Vector3(
                (cannonOffsetX + beamVisualLength * 0.5f) * direction,
                0f,
                0f
            );

            beamVisual.transform.localScale = new Vector3(
                beamVisualLength,
                beamSize.y,
                1f
            );
        }

        if (spirit != null)
        {
            typeof(SpiritGauge).GetField("currentSpirit",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(spirit, 0f);
        }

        StartCoroutine(BeamDamageLoop());
    }


    private IEnumerator BeamDamageLoop()
    {
        float elapsed = 0f;

        while (elapsed < beamDuration)
        {
            hitThisTick.Clear();
            DamageEnemiesInBeam();

            elapsed += tickRate;
            yield return new WaitForSeconds(tickRate);
        }

        Cleanup();
    }

    private void DamageEnemiesInBeam()
    {
        Vector2 direction = facingRight ? Vector2.right : Vector2.left;
        Vector2 beamCenter = (Vector2)transform.position + direction * (cannonOffsetX + beamSize.x * 0.5f);

        Collider2D[] hits = Physics2D.OverlapBoxAll(beamCenter, beamSize, 0f, enemyMask);

        foreach (var col in hits)
        {
            var h = col.GetComponentInParent<Health>();
            if (h == null) continue;

            int id = h.GetInstanceID();
            if (hitThisTick.Contains(id)) continue;

            hitThisTick.Add(id);
            h.TakeDamage(damagePerTick, direction, false, CrowdControlState.None, 0f);
        }
    }

    private void Cleanup()
    {
        if (spirit != null)
            spirit.StopDrain();

        if (beamVisual != null)
            beamVisual.SetActive(false);

        OnFinished?.Invoke();
        Destroy(gameObject);
    }

    private void OnDrawGizmos()
    {
        if (!isFiring) return;
        Gizmos.color = Color.cyan;
        Vector2 direction = facingRight ? Vector2.right : Vector2.left;
        Vector2 beamCenter = (Vector2)transform.position + direction * (cannonOffsetX + beamSize.x * 0.5f);
        Gizmos.DrawWireCube(beamCenter, beamSize);
    }
}
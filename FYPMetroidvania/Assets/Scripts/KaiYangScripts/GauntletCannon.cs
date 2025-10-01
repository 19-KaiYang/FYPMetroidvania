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
    public float cannonOffsetX = 0.5f; // Distance from cannon center to where beam starts

    [Header("Visual")]
    public SpriteRenderer cannonSprite;
    public GameObject beamVisual;

    private SpiritGauge spirit;
    private LayerMask enemyMask;
    private bool facingRight;
    private bool isCharging = true;
    private bool isFiring = false;
    private float chargeTimer = 0f;

    // Track enemies hit this tick to prevent multi-hits
    private HashSet<int> hitThisTick = new HashSet<int>();

    private void Awake()
    {
        // Hide beam when spawned
        if (beamVisual != null)
        {
            beamVisual.SetActive(false);
        }
    }

    public void Init(bool playerFacingRight, SpiritGauge spiritGauge, LayerMask enemyLayer,
                     float charge, float damage, float tick, Vector2 size)
    {
        facingRight = playerFacingRight;
        spirit = spiritGauge;
        enemyMask = enemyLayer;

        // Apply settings from Skills.cs
        chargeTime = charge;
        damagePerTick = damage;
        tickRate = tick;
        beamSize = size;
        beamVisualLength = size.x; // Match visual length to collider X size

        // Flip cannon sprite if needed
        if (cannonSprite != null)
        {
            cannonSprite.flipX = !facingRight;
        }

        // Hide beam initially
        if (beamVisual != null)
        {
            beamVisual.SetActive(false);
        }

        StartCoroutine(ChargeSequence());
    }

    private IEnumerator ChargeSequence()
    {
        // Charge phase
        while (chargeTimer < chargeTime)
        {
            chargeTimer += Time.deltaTime;
            yield return null;
        }

        // Auto-fire after charging
        FireBeam();
    }

    public void FireBeam()
    {
        if (isFiring) return;

        isFiring = true;
        isCharging = false;

        // Show beam visual
        if (beamVisual != null)
        {
            beamVisual.SetActive(true);

            float direction = facingRight ? 1f : -1f;

            // Position visual at half-length (matches collider + gizmo)
            beamVisual.transform.localPosition = new Vector3(
                (cannonOffsetX + beamVisualLength * 0.5f) * direction,
                0f,
                0f
            );

            // Always positive scale
            beamVisual.transform.localScale = new Vector3(
                beamVisualLength,
                beamSize.y,
                1f
            );
        }

        // Start draining spirit
        if (spirit != null)
            spirit.StartDrain();

        StartCoroutine(BeamDamageLoop());
    }

    private IEnumerator BeamDamageLoop()
    {
        while (spirit != null && !spirit.IsEmpty)
        {
            hitThisTick.Clear();
            DamageEnemiesInBeam();

            yield return new WaitForSeconds(tickRate);
        }

        // End beam
        Cleanup();
    }

    private void DamageEnemiesInBeam()
    {
        Vector2 direction = facingRight ? Vector2.right : Vector2.left;

        // Hitbox center = cannon + offset + half length
        Vector2 beamCenter = (Vector2)transform.position + direction * (cannonOffsetX + beamSize.x * 0.5f);
        Vector2 hitboxSize = beamSize;

        Collider2D[] hits = Physics2D.OverlapBoxAll(beamCenter, hitboxSize, 0f, enemyMask);

        foreach (var col in hits)
        {
            var h = col.GetComponentInParent<Health>();
            if (h == null) continue;

            int id = h.GetInstanceID();
            if (hitThisTick.Contains(id)) continue;

            hitThisTick.Add(id);

            h.TakeDamage(damagePerTick, direction, false, CrowdControlState.None, 0f);
            Debug.Log($"[GauntletCannon] Dealt {damagePerTick} damage to {h.name}");
        }
    }

    private void Cleanup()
    {
        if (spirit != null)
        {
            spirit.StopDrain();
        }

        if (beamVisual != null)
        {
            beamVisual.SetActive(false);
        }

        Destroy(gameObject);
    }

    // Allow manual fire if player presses R again
    private void Update()
    {
        if (isCharging && Input.GetKeyDown(KeyCode.R))
        {
            StopAllCoroutines();
            FireBeam();
        }
    }

    private void OnDrawGizmos()
    {
        if (!isFiring) return;

        Gizmos.color = Color.cyan;
        Vector2 direction = facingRight ? Vector2.right : Vector2.left;

        // Gizmo center matches hitbox + visual
        Vector2 beamCenter = (Vector2)transform.position + direction * (cannonOffsetX + beamSize.x * 0.5f);
        Vector2 hitboxSize = beamSize;

        Gizmos.DrawWireCube(beamCenter, hitboxSize);
    }
}

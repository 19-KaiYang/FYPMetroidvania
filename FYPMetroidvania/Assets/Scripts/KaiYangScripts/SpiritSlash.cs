using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpiritSlash : MonoBehaviour
{
    public float speed = 15f;
    public float bounceRange = 6f;
    public float overshootDistance = 1f;
    public float hitDelay = 0.3f;
    public float hitCooldown = 0.5f;
    public float spiritSlashBloodCost = 10f;
    public float bloodMarkHeal = 20f;

    [Header("Hitbox")]
    public GameObject hitboxObject;

    [Header("Knockback Settings")]
    public float stunKnockbackMultiplier = 1f;
    public float knockdownKnockbackMultiplier = 1f;

    [Header("Crowd Control")]
    public CrowdControlState groundedCC = CrowdControlState.Stunned;
    public CrowdControlState airborneCC = CrowdControlState.Knockdown;
    public float ccDuration = 1.5f;

    [Header("Cutin Animation")]
    public string cutinCanvasName = "UpdatedPlayerUICanvas";
    public string cutinTriggerName = "PlayCutIn";
    public float cutinDuration = 1.4f;

    [Header("Spawn Animation")]
    public float freezeDuration = 0.4f;
    public float burstDelay = 0.3f;
    public float spiritFadeInDuration = 0.3f;
    public float overlayFadeOutDuration = 0.2f;
    public float darkenAlpha = 0.7f;

    private GameObject darkenOverlay;

    private Transform player;
    private Transform currentTarget;
    private LayerMask enemyMask;
    private SpiritGauge spirit;
    private Hitbox hitbox;

    private bool waiting = false;
    private bool isDelaying = false;
    private float lastHitTime = 0f;
    private Vector2 lastMovementDirection;

    private HashSet<int> hitEnemyIds = new HashSet<int>();

    private Health pendingTargetHealth = null;

    private bool isFullyInitialized = false;

    public void Init(Transform playerTransform, Transform target, LayerMask enemyMask, float healAmount)
    {
        player = playerTransform;
        currentTarget = target;
        this.enemyMask = enemyMask;
        bloodMarkHeal = healAmount;

        if (player != null)
        {
            spirit = player.GetComponent<SpiritGauge>();
        }

        if (hitboxObject != null)
        {
            hitbox = hitboxObject.GetComponent<Hitbox>();
            hitboxObject.SetActive(false); // hide hitbox until animation finishes
        }

        // Hide the spirit sprite immediately so it doesn't pop up early
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.enabled = false;

        // event subscribe
        Hitbox.OnHit += OnSpiritSlashHit;

        // Start ultimate
        Skills.InvokeUltimateStart(hitbox);

        // Start with cut-in, then cinematic spawn
        StartCoroutine(PlayCutinThenSpawn());
    }


    private void Update()
    {
        // Don't move until fully initialized (after cutin + dramatic spawn)
        if (!isFullyInitialized) return;

        if (spirit == null || spirit.IsEmpty)
        {
            Destroy(gameObject);
            return;
        }

        if (isDelaying) return;

        if (currentTarget == null)
        {
            if (!waiting) StartCoroutine(WaitForEnemy());
            return;
        }

        Vector2 dir = (currentTarget.position - transform.position).normalized;
        lastMovementDirection = dir;
        transform.position += (Vector3)dir * speed * Time.deltaTime;

        if (Vector2.Distance(transform.position, currentTarget.position) < 0.5f &&
            Time.time - lastHitTime >= hitCooldown)
        {
            ReachTarget(currentTarget);
            lastHitTime = Time.time;
        }
    }

    private void OnSpiritSlashHit(Hitbox hb, Health h)
    {
        if (hb != hitbox) return;
        if (h == null || h.isPlayer) return;

        if (currentTarget != null)
        {
            Health currentTargetHealth = currentTarget.GetComponent<Health>();
            if (currentTargetHealth == null || h != currentTargetHealth)
                return;
        }
        else if (pendingTargetHealth != null)
        {
            if (h != pendingTargetHealth)
                return;
        }
        else
        {
            return;
        }

        int id = h.GetInstanceID();

        if (!hitEnemyIds.Contains(id))
        {
            hitEnemyIds.Add(id);

            Skills.InvokeUltimateHit(hitbox, h);

            Vector2 knockDir = (h.transform.position - transform.position).normalized;

            h.TakeDamage(spiritSlashBloodCost, knockDir, false, CrowdControlState.None, 0f, true, false, 0f);

            Skills skills = player.GetComponent<Skills>();
            if (skills != null)
            {
                skills.ApplySkillCC(h, knockDir, groundedCC, airborneCC, ccDuration,
                                   stunKnockbackMultiplier, knockdownKnockbackMultiplier);
            }

            h.ApplyBloodMark(bloodMarkHeal);
        }
    }

    private void ReachTarget(Transform target)
    {
        if (hitboxObject != null)
        {
            pendingTargetHealth = target.GetComponent<Health>();
            StartCoroutine(EnableHitboxAtTarget(target.position));
        }

        currentTarget = null;
        StartCoroutine(DelayBeforeNextTarget());
    }

    private IEnumerator EnableHitboxAtTarget(Vector3 targetPos)
    {
        if (hitbox == null) yield break;

        Collider2D col = hitboxObject.GetComponent<Collider2D>();
        if (col == null) yield break;

        Vector3 offsetPos = targetPos - (Vector3)(lastMovementDirection * 0.3f);
        transform.position = offsetPos;

        hitbox.ClearHitEnemies();

        yield return null;

        col.enabled = true;
        yield return new WaitForFixedUpdate();
        col.enabled = false;

        Vector3 overshootPosition = targetPos + (Vector3)(lastMovementDirection * overshootDistance);
        transform.position = overshootPosition;

        pendingTargetHealth = null;
    }

    private IEnumerator EnableHitboxTemporarily(float duration)
    {
        if (hitboxObject == null) yield break;

        hitboxObject.SetActive(true);

        if (hitbox != null)
            hitbox.ClearHitEnemies();

        yield return new WaitForSeconds(duration);

        hitboxObject.SetActive(false);
    }

    private IEnumerator DelayBeforeNextTarget()
    {
        isDelaying = true;
        yield return new WaitForSeconds(hitDelay);
        isDelaying = false;
        FindNextTarget();
    }

    private void FindNextTarget()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(player.position, bounceRange, enemyMask);
        var available = new List<Transform>();
        var unhit = new List<Transform>();
        var seenRoots = new HashSet<Transform>();

        foreach (var col in hits)
        {
            var h = col.GetComponentInParent<Health>();
            if (h == null) continue;

            Transform root = h.transform;
            if (!seenRoots.Add(root)) continue;

            available.Add(root);
            if (!hitEnemyIds.Contains(h.GetInstanceID()))
                unhit.Add(root);
        }

        Transform best = null;

        if (unhit.Count > 0)
        {
            float closest = float.MaxValue;
            foreach (var t in unhit)
            {
                float d = Vector2.Distance(transform.position, t.position);
                if (d < closest) { closest = d; best = t; }
            }
        }
        else if (available.Count > 1)
        {
            hitEnemyIds.Clear();

            float closest = float.MaxValue;
            foreach (var t in available)
            {
                float d = Vector2.Distance(transform.position, t.position);
                if (d < closest) { closest = d; best = t; }
            }
        }
        else if (available.Count == 1)
        {
            hitEnemyIds.Clear();
            best = available[0];
        }

        if (best != null) { currentTarget = best; waiting = false; }
        else { currentTarget = null; }
    }

    private IEnumerator WaitForEnemy()
    {
        waiting = true;
        while (currentTarget == null && spirit != null && !spirit.IsEmpty)
        {
            FindNextTarget();
            yield return new WaitForSeconds(0.2f);
        }
        waiting = false;
    }

    private void OnDestroy()
    {
        Hitbox.OnHit -= OnSpiritSlashHit;
        Skills.InvokeUltimateEnd();
    }

    #region CUTIN_AND_SPAWN
    private IEnumerator PlayCutinThenSpawn()
    {
        // Find the canvas
        GameObject cutinCanvas = GameObject.Find(cutinCanvasName);
        Animator cutinAnimator = null;

        if (cutinCanvas != null)
            cutinAnimator = cutinCanvas.GetComponent<Animator>();

        bool hasCutin = (cutinAnimator != null);

        // Store original timescale
        float originalTimeScale = Time.timeScale;

        // Freeze gameplay BEFORE cut-in starts
        Time.timeScale = 0f;

        if (hasCutin)
        {
            cutinAnimator.ResetTrigger(cutinTriggerName);
            cutinAnimator.SetTrigger(cutinTriggerName);
            Debug.Log("[SpiritSlash] Playing Cut-In Animation");

            // Wait for the cut-in animation duration using unscaled time
            float elapsed = 0f;
            while (elapsed < cutinDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
        }

        yield return new WaitForSecondsRealtime(0.05f);

        yield return StartCoroutine(DramaticSpawn());

        // Restore time after everything
        Time.timeScale = originalTimeScale;

        // Mark initialization done
        isFullyInitialized = true;
    }



    #endregion

    #region POLISHINGANIMATION

    private static GameObject cachedOverlay;

    private IEnumerator DramaticSpawn()
    {
        // CREATE overlay if it doesn't exist
        if (cachedOverlay == null)
        {
            cachedOverlay = CreateDarkenOverlay();
        }
        darkenOverlay = cachedOverlay;

        // Freeze time
        float originalTimeScale = Time.timeScale;
        Time.timeScale = 0f;

        // Show overlay
        if (darkenOverlay != null)
            darkenOverlay.SetActive(true);

        // Prepare sprite
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        Vector3 originalScale = transform.localScale;
        Color originalColor = sr != null ? sr.color : Color.white;

        if (sr != null)
        {
            sr.enabled = true; // now enable it for fade-in
            sr.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0);
        }
        transform.localScale = Vector3.zero;

        // Short delay before burst
        float elapsed = 0f;
        while (elapsed < burstDelay)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        // Burst effects while time is frozen
        CreateDramaticBurst();

        // Fade in spirit slash while still frozen
        elapsed = 0f;
        while (elapsed < spiritFadeInDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / spiritFadeInDuration;

            float scale = Mathf.Min(t * 2f, 1.3f);
            if (t > 0.5f) scale = 1.3f - ((t - 0.5f) / 0.5f) * 0.3f;

            transform.localScale = originalScale * scale;

            if (sr != null)
                sr.color = new Color(originalColor.r, originalColor.g, originalColor.b, t);

            yield return null;
        }

        transform.localScale = originalScale;
        if (sr != null) sr.color = originalColor;

        // Resume time
        Time.timeScale = originalTimeScale;

        // Fade out overlay
        if (darkenOverlay != null)
            StartCoroutine(FadeOutOverlay());

        // Enable hitbox only after cinematic is done
        if (hitboxObject != null)
            hitboxObject.SetActive(true);
    }


    private GameObject CreateDarkenOverlay()
    {
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();

        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("SpawnCanvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        }

        canvas.sortingOrder = 9999;

        GameObject overlay = new GameObject("DarkenOverlay");
        overlay.transform.SetParent(canvas.transform, false);

        UnityEngine.UI.Image image = overlay.AddComponent<UnityEngine.UI.Image>();
        image.color = new Color(0, 0, 0, darkenAlpha);

        RectTransform rect = overlay.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        overlay.AddComponent<CanvasGroup>();
        overlay.SetActive(false);

        Debug.Log("Created DarkenOverlay successfully!");

        return overlay;
    }

    private IEnumerator FadeOutOverlay()
    {
        CanvasGroup cg = darkenOverlay.GetComponent<CanvasGroup>();
        if (cg == null)
        {
            darkenOverlay.SetActive(false);
            yield break;
        }

        float elapsed = 0f;

        while (elapsed < overlayFadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / overlayFadeOutDuration;
            cg.alpha = 1 - t;
            yield return null;
        }

        darkenOverlay.SetActive(false);
        cg.alpha = 1f;
    }

    private void CreateDramaticBurst()
    {
        CreateBurstRing(12, 0f, 0.3f, 1.5f, Color.white, 0.2f);
        CreateBurstRing(16, 0f, 0.4f, 2.5f, new Color(0, 1, 1), 0.15f);
        CreateShockwaveRing();
    }

    private void CreateBurstRing(int count, float delay, float duration, float distance, Color color, float size)
    {
        for (int i = 0; i < count; i++)
        {
            float angle = (360f / count) * i;
            Vector2 direction = new Vector2(
                Mathf.Cos(angle * Mathf.Deg2Rad),
                Mathf.Sin(angle * Mathf.Deg2Rad)
            );

            StartCoroutine(AnimateBurstParticle(direction, delay, duration, distance, color, size));
        }
    }

    private IEnumerator AnimateBurstParticle(Vector2 direction, float delay, float duration, float distance, Color color, float size)
    {
        if (delay > 0)
        {
            float delayElapsed = 0f;
            while (delayElapsed < delay)
            {
                delayElapsed += Time.unscaledDeltaTime;
                yield return null;
            }
        }

        GameObject particle = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Destroy(particle.GetComponent<Collider>());

        particle.transform.position = transform.position;
        particle.transform.localScale = Vector3.one * size;

        Renderer rend = particle.GetComponent<Renderer>();
        if (rend != null) rend.material.color = color;

        Vector3 startPos = transform.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;

            float moveT = 1 - Mathf.Pow(1 - t, 2);
            particle.transform.position = startPos + (Vector3)(direction * distance * moveT);

            if (rend != null)
            {
                Color c = rend.material.color;
                c.a = 1 - t;
                rend.material.color = c;
            }

            particle.transform.localScale = Vector3.one * (size * (1 - t * 0.3f));

            yield return null;
        }

        Destroy(particle);
    }

    private void CreateShockwaveRing()
    {
        GameObject ring = new GameObject("Shockwave");
        ring.transform.position = transform.position;

        LineRenderer lr = ring.AddComponent<LineRenderer>();
        lr.useWorldSpace = false;
        lr.loop = true;

        int segments = 40;
        lr.positionCount = segments;

        for (int i = 0; i < segments; i++)
        {
            float angle = (360f / segments) * i;
            float x = Mathf.Cos(angle * Mathf.Deg2Rad);
            float y = Mathf.Sin(angle * Mathf.Deg2Rad);
            lr.SetPosition(i, new Vector3(x, y, 0) * 0.1f);
        }

        lr.startWidth = 0.1f;
        lr.endWidth = 0.1f;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = Color.white;
        lr.endColor = Color.white;
        lr.sortingOrder = 10;

        StartCoroutine(AnimateShockwave(ring, lr, segments));
    }

    private IEnumerator AnimateShockwave(GameObject ring, LineRenderer lr, int segments)
    {
        float duration = 0.5f;
        float maxRadius = 3f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;

            float radius = Mathf.Lerp(0.1f, maxRadius, t);

            for (int i = 0; i < segments; i++)
            {
                float angle = (360f / segments) * i;
                float x = Mathf.Cos(angle * Mathf.Deg2Rad);
                float y = Mathf.Sin(angle * Mathf.Deg2Rad);
                lr.SetPosition(i, new Vector3(x, y, 0) * radius);
            }

            Color c = Color.white;
            c.a = 1 - t;
            lr.startColor = c;
            lr.endColor = c;

            yield return null;
        }

        Destroy(ring);
    }

    #endregion
}
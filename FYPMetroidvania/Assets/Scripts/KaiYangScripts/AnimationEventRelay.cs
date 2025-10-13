using UnityEngine;

public class AnimationEventRelay : MonoBehaviour
{
    private CombatSystem combatSystem;

    // Reference to the ParticleEffects GameObject (assign in Inspector)
    public GameObject particleEffectsObject;

    private void Awake()
    {
        //  Automatically find CombatSystem on the Player 
        combatSystem = GetComponentInParent<CombatSystem>();
    }

    // === Forwarded Methods ===
    public void EnableHitbox(int index) => combatSystem?.EnableHitbox(index);
    public void DisableHitbox(int index) => combatSystem?.DisableHitbox(index);

    public void EnableSwordUpHitbox() => combatSystem?.swordUpHitbox?.SetActive(true);
    public void DisableSwordUpHitbox() => combatSystem?.DisableSwordUpHitbox();

    public void EnableGauntletUpHitbox() => combatSystem?.gauntletUpHitbox?.SetActive(true);
    public void DisableGauntletUpHitbox() => combatSystem?.DisableGauntletUpHitbox();

    public void EnableSwordDownHitbox() => combatSystem?.swordDownHitbox?.SetActive(true);
    public void DisableSwordDownHitbox() => combatSystem?.DisableSwordDownHitbox();

    public void EnableGauntletDownHitbox() => combatSystem?.gauntletDownHitbox?.SetActive(true);
    public void DisableGauntletDownHitbox() => combatSystem?.DisableGauntletDownHitbox();

    // === OLD Particle Effect Methods (using Animator triggers) ===
    public void PlayEffect1()
    {
        if (combatSystem != null)
            combatSystem.PlayEffect1();
    }

    public void PlayEffect2()
    {
        if (combatSystem != null)
            combatSystem.PlayEffect2();
    }

    public void PlayEffect3()
    {
        if (combatSystem != null)
            combatSystem.PlayEffect3();
    }

    // === Direct Control Methods ===
    public void ShowEffect1()
    {
        if (particleEffectsObject != null)
        {
            // Clear any previous sprite first
            var sr = particleEffectsObject.GetComponent<SpriteRenderer>();
            if (sr != null) sr.sprite = null;

            particleEffectsObject.SetActive(true);
            if (combatSystem?.particleEffectAnimator != null)
                combatSystem.particleEffectAnimator.Play("Effect1");
        }
    }

    public void HideEffect1()
    {
        if (particleEffectsObject != null)
        {
            // Clear sprite when hiding
            var sr = particleEffectsObject.GetComponent<SpriteRenderer>();
            if (sr != null) sr.sprite = null;

            particleEffectsObject.SetActive(false);
        }
    }

    public void ShowEffect2()
    {
        if (particleEffectsObject != null)
        {
            // Clear any previous sprite first
            var sr = particleEffectsObject.GetComponent<SpriteRenderer>();
            if (sr != null) sr.sprite = null;

            particleEffectsObject.SetActive(true);
            if (combatSystem?.particleEffectAnimator != null)
                combatSystem.particleEffectAnimator.Play("Effect2");
        }
    }

    public void HideEffect2()
    {
        if (particleEffectsObject != null)
        {
            var sr = particleEffectsObject.GetComponent<SpriteRenderer>();
            if (sr != null) sr.sprite = null;

            particleEffectsObject.SetActive(false);
        }
    }

    public void ShowEffect3()
    {
        if (particleEffectsObject != null)
        {
            // Clear any previous sprite first
            var sr = particleEffectsObject.GetComponent<SpriteRenderer>();
            if (sr != null) sr.sprite = null;

            particleEffectsObject.SetActive(true);
            if (combatSystem?.particleEffectAnimator != null)
                combatSystem.particleEffectAnimator.Play("Effect3");
        }
    }

    public void HideEffect3()
    {
        if (particleEffectsObject != null)
        {
            var sr = particleEffectsObject.GetComponent<SpriteRenderer>();
            if (sr != null) sr.sprite = null;

            particleEffectsObject.SetActive(false);
        }
    }
}
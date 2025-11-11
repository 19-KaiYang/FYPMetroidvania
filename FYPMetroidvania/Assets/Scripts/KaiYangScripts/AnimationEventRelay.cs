using UnityEngine;
using System.Linq;

public class AnimationEventRelay : MonoBehaviour
{
    [SerializeField] Animator animator;
    private PlayerController controller;
    private CombatSystem combatSystem;
    private Skills skills;

    // Reference to the ParticleEffects GameObject (assign in Inspector)
    public GameObject particleEffectsObject;

    private void Awake()
    {
        //  Automatically find CombatSystem on the Player 
        controller = GetComponent<PlayerController>();
        combatSystem = GetComponentInParent<CombatSystem>();
        skills = GetComponentInParent<Skills>();
    }
    public void PlayFootstep()
    {
        AudioManager.PlaySFX(SFXTYPE.PLAYER_FOOTSTEP, 0.2f, pitch: Random.Range(0.9f,1.1f));
    }
    // === Forwarded Methods ===
    public void EnableHitbox(int index) => combatSystem?.EnableHitbox(index);
    public void DisableHitbox(int index) => combatSystem?.DisableHitbox(index);
    public void PlayVFX(string Name) => combatSystem?.PlayVFX(Name);
    public void HideVFX() => combatSystem?.HideVFX();
    public void EndAnimation() => combatSystem?.SetEndAnimation();
    #region Old Methods
    public void EnableSwordUpHitbox() => combatSystem?.swordUpHitbox?.SetActive(true);
    public void DisableSwordUpHitbox() => combatSystem?.DisableSwordUpHitbox();

    public void EnableGauntletUpHitbox() => combatSystem?.gauntletUpHitbox?.SetActive(true);
    public void DisableGauntletUpHitbox() => combatSystem?.DisableGauntletUpHitbox();
    public void EnableSwordDownHitbox() => combatSystem?.swordDownHitbox?.SetActive(true);
    public void DisableSwordDownHitbox() => combatSystem?.DisableSwordDownHitbox();
    public void EnableGauntletDownHitbox() => combatSystem?.gauntletDownHitbox?.SetActive(true);
    public void DisableGauntletDownHitbox() => combatSystem?.DisableGauntletDownHitbox();
#endregion
    public void SetCanTransition(int comboEnd) => combatSystem?.SetCanTransition(comboEnd);
    public void ResetCombo() => combatSystem?.ResetCombo();
    public void SetCanBuffer() => combatSystem?.SetCanBuffer();
    public void SetUppercutStart(int start) => skills?.SetUppercut_Start(start);
    public void SetLungeStart() => skills?.SetLunge_Start();
    public void SetWaveStart(int start) => skills?.SetWave_Start(start);

    #region Old VFX Methods
    // === OLD Particle Effect Methods (using Animator triggers) ===
    public void PlayEffect1()
    {
        if (combatSystem != null)
            combatSystem.PlayEffect1();
        //AudioManager.PlaySFX(SFXTYPE.SWORD_SWING, 0.5f);
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
            AudioManager.PlaySFX(SFXTYPE.SWORD_SWING, 0.5f);
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
            AudioManager.PlaySFX(SFXTYPE.SWORD_SWING, 0.5f);
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
            AudioManager.PlaySFX(SFXTYPE.SWORD_SWING, 1.0f);
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
    #endregion
    //public void PlayVFX(string Name)
    //{
    //    if (particleEffectsObject == null) return;
    //    AttackVFX vfx = combatSystem.attackVFXList.FirstOrDefault(i => i.VFX_name == Name);
    //    if(vfx != null)
    //    {
    //        particleEffectsObject.SetActive(true);
    //        particleEffectsObject.transform.localPosition = vfx.position;
    //        particleEffectsObject.transform.localEulerAngles = new Vector3(0f, 0f, vfx.angle);
    //        particleEffectsObject.transform.localScale = vfx.scale;
    //        if (combatSystem?.particleEffectAnimator != null)
    //            combatSystem.particleEffectAnimator.Play(vfx.animationName);

    //        AudioManager.PlaySFX(SFXTYPE.SWORD_SWING, vfx.sfxVolume);
    //    }
    //}
    //public void HideVFX()
    //{
    //    if (particleEffectsObject != null)
    //    {
    //        var sr = particleEffectsObject.GetComponent<SpriteRenderer>();
    //        if (sr != null) sr.sprite = null;

    //        particleEffectsObject.SetActive(false);
    //    }
    //    controller.animator.SetBool("isAttacking", false);
    //}
}
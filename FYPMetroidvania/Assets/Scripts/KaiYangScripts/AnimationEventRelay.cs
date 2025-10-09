using UnityEngine;

public class AnimationEventRelay : MonoBehaviour
{
    private CombatSystem combatSystem;

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
}

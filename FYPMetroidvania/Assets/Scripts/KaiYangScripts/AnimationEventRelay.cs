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

    public void EnableUpHitbox() => combatSystem?.EnableUpHitbox();
    public void DisableUpHitbox() => combatSystem?.DisableUpHitbox();

    public void EnableDownHitbox() => combatSystem?.EnableDownHitbox();
    public void DisableDownHitbox() => combatSystem?.DisableDownHitbox();
}

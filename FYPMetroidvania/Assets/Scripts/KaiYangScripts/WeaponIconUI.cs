using UnityEngine;
using UnityEngine.UI;

public class WeaponIconUI : MonoBehaviour
{
    [Header("References")]
    public CombatSystem combatSystem; 
    public Image weaponIcon;          

    [Header("Icons")]
    public Sprite swordIcon;
    public Sprite gauntletIcon;
    public Sprite noneIcon;

    private WeaponType lastWeapon;

    private void Start()
    {
        combatSystem = Object.FindFirstObjectByType<CombatSystem>();


        lastWeapon = WeaponType.None;
        UpdateIcon();
    }

    private void Update()
    {
        if (combatSystem == null || weaponIcon == null) return;

        if (combatSystem.currentWeapon != lastWeapon)
        {
            lastWeapon = combatSystem.currentWeapon;
            UpdateIcon();
        }
    }

    private void UpdateIcon()
    {
        switch (lastWeapon)
        {
            case WeaponType.Sword:
                weaponIcon.sprite = swordIcon;
                break;
            case WeaponType.Gauntlet:
                weaponIcon.sprite = gauntletIcon;
                break;
            default:
                weaponIcon.sprite = noneIcon;
                break;
        }
    }
}

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class UpgradeUIController : MonoBehaviour
{
    public UpgradeManager upgradeManager;
    public CombatSystem combat;
    private Skills skills;

    [Header("General Upgrade")]
    public Button generalDamageButton;
    public TextMeshProUGUI generalDamageText;

    [Header("Weapon UI")]
    public TextMeshProUGUI weaponNameText;
    public Button prevWeaponButton;
    public Button nextWeaponButton;

    [Header("Skill 1 UI")]
    public TextMeshProUGUI skill1NameText;
    public Button skill1DamageButton;
    public TextMeshProUGUI skill1DamageText;
    public TextMeshProUGUI skill1EnergyText;

    [Header("Skill 2 UI")]
    public TextMeshProUGUI skill2NameText;
    public Button skill2DamageButton;
    public TextMeshProUGUI skill2DamageText;
    public Button skill2EnergyButton;
    public TextMeshProUGUI skill2EnergyText;

    private WeaponType currentWeapon = WeaponType.None;

    private void Start()
    {
        if (combat == null)
            combat = Object.FindFirstObjectByType<CombatSystem>();
        if (combat != null)
            skills = combat.GetComponent<Skills>();

        if (combat != null)
            currentWeapon = combat.currentWeapon;

        generalDamageButton.onClick.AddListener(() => { upgradeManager.generalDamageLevel++; RefreshUI(); });

        prevWeaponButton.onClick.AddListener(() => SwitchWeapon(-1));
        nextWeaponButton.onClick.AddListener(() => SwitchWeapon(1));

        skill1DamageButton.onClick.AddListener(OnSkill1DamageUpgrade);
        skill2DamageButton.onClick.AddListener(OnSkill2DamageUpgrade);
        skill2EnergyButton.onClick.AddListener(OnSkill2EnergyUpgrade);

        RefreshUI();
    }

    // === Upgrade Handlers ===
    private void OnSkill1DamageUpgrade()
    {
        if (currentWeapon == WeaponType.Sword) upgradeManager.swordDashDamageLevel++;
        else if (currentWeapon == WeaponType.Gauntlet) upgradeManager.gauntletShockwaveDamageLevel++;
        RefreshUI();
    }

    private void OnSkill2DamageUpgrade()
    {
        if (currentWeapon == WeaponType.Sword) upgradeManager.swordUppercutDamageLevel++;
        else if (currentWeapon == WeaponType.Gauntlet) upgradeManager.gauntletLaunchDamageLevel++;
        RefreshUI();
    }

    private void OnSkill2EnergyUpgrade()
    {
        if (currentWeapon == WeaponType.Sword) upgradeManager.swordUppercutEnergyLevel++;
        else if (currentWeapon == WeaponType.Gauntlet) upgradeManager.gauntletLaunchEnergyLevel++;
        RefreshUI();
    }

    // === Weapon Switching ===
    private void SwitchWeapon(int dir)
    {
        if (combat == null) return;

        // Get unlocked weapons from CombatSystem
        var unlockedList = new List<WeaponType>(combat.GetUnlockedWeapons());
        if (unlockedList.Count == 0) return;

        // Find current index in unlocked weapons
        int index = unlockedList.IndexOf(currentWeapon);
        if (index == -1) index = 0; 

        // Cycle
        index = (index + dir + unlockedList.Count) % unlockedList.Count;
        currentWeapon = unlockedList[index];

        combat.SetWeapon(currentWeapon); 
        RefreshUI();
    }



    // === Refresh UI ===
    private void RefreshUI()
    {
        generalDamageText.text = $"General Damage +{upgradeManager.GetGeneralDamageBonus()}";
        weaponNameText.text = currentWeapon.ToString();

        if (skills == null) return;

        if (currentWeapon == WeaponType.Sword)
        {
            // Skill 1: Dash
            skill1NameText.text = "Sword Dash";
            skill1DamageText.text = $"+{upgradeManager.GetSwordDashBonus()} Damage";
            skill1EnergyText.text = $"Cost: {skills.swordDashCost}";

            // Skill 2: Uppercut
            skill2NameText.text = "Sword Uppercut";
            skill2DamageText.text = $"+{upgradeManager.GetSwordUppercutBonus()} Damage";
            skill2EnergyText.text = $"Cost: {skills.swordUppercutCost - upgradeManager.GetSwordUppercutEnergyReduction()}";
        }
        else if (currentWeapon == WeaponType.Gauntlet)
        {
            // Skill 1: Shockwave
            skill1NameText.text = "Gauntlet Shockwave";
            skill1DamageText.text = $"+{upgradeManager.GetGauntletShockwaveBonus()} Damage";
            skill1EnergyText.text = $"Cost: {skills.gauntletShockwaveCost}";

            // Skill 2: Launch
            skill2NameText.text = "Gauntlet Launch";
            skill2DamageText.text = $"+{upgradeManager.GetGauntletLaunchBonus()} Damage";
            skill2EnergyText.text = $"Cost: {skills.gauntletSkillEnergyCost - upgradeManager.GetGauntletLaunchEnergyReduction()}";
        }
        else
        {
            skill1NameText.text = "Skill 1";
            skill1DamageText.text = "-";
            skill1EnergyText.text = "-";

            skill2NameText.text = "Skill 2";
            skill2DamageText.text = "-";
            skill2EnergyText.text = "-";
        }
    }
}

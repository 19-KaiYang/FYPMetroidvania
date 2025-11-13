using UnityEngine;
using TMPro;

public class SkillCostUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Skills playerSkills; 
    [SerializeField] private TMP_Text lungingCostText;
    [SerializeField] private TMP_Text uppercutCostText;
    [SerializeField] private TMP_Text crimsonCostText;

    private void Start()
    {
        if (playerSkills == null)
            playerSkills = FindFirstObjectByType<Skills>();

        UpdateSkillCosts();
    }

    private void UpdateSkillCosts()
    {
        //if (playerSkills == null) return;

        //lungingCostText.text = $"{playerSkills.swordDashCost}";
        //uppercutCostText.text = $"{playerSkills.swordUppercutCost}";
        //crimsonCostText.text = $"{playerSkills.swordSlashEnergyCost}";
    }

}

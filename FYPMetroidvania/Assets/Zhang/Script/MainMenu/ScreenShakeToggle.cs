using UnityEngine;
using UnityEngine.UI;

public class ScreenShakeToggle : MonoBehaviour
{
    public ToggleGroup toggleGroup;
    public bool screenShake { get; private set; }

    void Start()
    {
        foreach (var toggle in toggleGroup.GetComponentsInChildren<Toggle>())
        {
            toggle.onValueChanged.AddListener(isOn =>
            {
                if (isOn)
                {
                    OnToggleChanged(toggle);
                }
            });
        }
    }

    void OnToggleChanged(Toggle changedToggle)
    {
        switch (changedToggle.name)
        {
            case "Toggle_ScreenShake_On":
                screenShake = true;
                break;
            case "Toggle_ScreenShake_Off":
                screenShake = false;
                break;
        }
        SettingData.instance.screenshake = screenShake;
        Debug.Log($"Screen shake£º{screenShake}");
    }
}

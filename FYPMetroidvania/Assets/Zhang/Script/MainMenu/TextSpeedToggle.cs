using UnityEngine;
using UnityEngine.UI;

public class TextSpeedToggle : MonoBehaviour
{
    public ToggleGroup toggleGroup;
    public float currentSpeed { get; private set; }

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
            case "Toggle_Slow":
                currentSpeed = 0.5f;
                break;
            case "Toggle_Default":
                currentSpeed = 1.0f;
                break;
            case "Toggle_Fast":
                currentSpeed = 1.5f;
                break;
        }
        Debug.Log($"Text Speed£º{currentSpeed}");
    }
}

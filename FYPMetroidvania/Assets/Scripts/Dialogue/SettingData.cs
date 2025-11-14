using UnityEngine;

public class SettingData : MonoBehaviour
{
    public static SettingData instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    public float textSpeedMult = 1f;
    public bool screenshake = true;
}

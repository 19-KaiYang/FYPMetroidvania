using UnityEngine;
using UnityEngine.UI;

public class UISpiritActive : MonoBehaviour
{
    [SerializeField] private GameObject inactiveImage;
    [SerializeField] private GameObject activeImage;

    private void Update()
    {
        SpiritSlash spiritSlash = FindFirstObjectByType<SpiritSlash>();

        if (spiritSlash != null)
        {
            activeImage.SetActive(true);
            inactiveImage.SetActive(false);
        }
        else
        {
            activeImage.SetActive(false);
            inactiveImage.SetActive(true);
        }
    }
}

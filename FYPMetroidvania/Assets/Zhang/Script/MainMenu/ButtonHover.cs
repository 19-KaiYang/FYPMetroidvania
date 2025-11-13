using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public class ButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image hoverImage;
    public Transform textSize;
    private Vector2 originalTextSize = new Vector2(1.0f, 1.0f);
    private Vector2 targetTextSize = new Vector2(1.5f, 1.5f);

    void Start()
    {
        HideHover();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (hoverImage != null) hoverImage.gameObject.SetActive(true);
        if (IsInPanel("PausePanel") && textSize) IncreaseTextSize();
        if (IsInPanel("SettingPanel") && textSize) IncreaseTextSize();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        HideHover();
        if (IsInPanel("PausePanel") && textSize) DecreaseTextSize();
        if (IsInPanel("SettingPanel") && textSize) DecreaseTextSize();
    }
    private void OnDisable()
    {
        HideHover();
        if (IsInPanel("PausePanel") && textSize) DecreaseTextSize();
        if (IsInPanel("SettingPanel") && textSize) DecreaseTextSize();
    }

    private void OnDestroy()
    {
        HideHover();
        if (IsInPanel("PausePanel") && textSize) DecreaseTextSize();
        if (IsInPanel("SettingPanel") && textSize) DecreaseTextSize();
    }

    private void HideHover()
    {
        if (hoverImage != null) hoverImage.gameObject.SetActive(false);
    }

    private void IncreaseTextSize()
    {
        if (textSize != null) textSize.localScale = targetTextSize;
    }
    private void DecreaseTextSize()
    {
        if (textSize != null) textSize.localScale = originalTextSize;
    }

    private bool IsInPanel(string parentName)
    {
        Transform t = transform;
        while (t.parent != null)
        {
            t = t.parent;
            if (t.name == parentName)
                return true;
        }
        return false;
    }
}

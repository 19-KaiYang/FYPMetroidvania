using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using System.Text.RegularExpressions;

public class TooltipTrigger : MonoBehaviour, IPointerMoveHandler, IPointerExitHandler, IPointerEnterHandler
{
    private TextMeshProUGUI textComponent;
    private string lastHoveredKeyword = "";
    private bool isPointerOver = false;

    void Awake()
    {
        textComponent = GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        if (isPointerOver && textComponent && TooltipSystem.Instance != null)
        {
            Vector2 mousePosition = Input.mousePosition;
            int linkIndex = TMP_TextUtilities.FindIntersectingLink(textComponent, mousePosition, null);

            if (linkIndex != -1)
            {
                TMP_LinkInfo linkInfo = textComponent.textInfo.linkInfo[linkIndex];
                string keyword = linkInfo.GetLinkID();

                if (keyword != lastHoveredKeyword)
                {
                    lastHoveredKeyword = keyword;
                    if (TooltipSystem.Instance.HasTooltip(keyword))
                    {
                        TooltipSystem.Instance.ShowTooltip(keyword, mousePosition);
                    }
                }
                else
                {
                    TooltipSystem.Instance.UpdateTooltipPosition(mousePosition);
                }
            }
            else if (lastHoveredKeyword != "")
            {
                lastHoveredKeyword = "";
                TooltipSystem.Instance.HideTooltip();
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isPointerOver = true;
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        if (!textComponent || TooltipSystem.Instance == null) return;

        int linkIndex = TMP_TextUtilities.FindIntersectingLink(textComponent, eventData.position, null);

        if (linkIndex != -1)
        {
            TMP_LinkInfo linkInfo = textComponent.textInfo.linkInfo[linkIndex];
            string keyword = linkInfo.GetLinkID();

            if (keyword != lastHoveredKeyword)
            {
                lastHoveredKeyword = keyword;
                if (TooltipSystem.Instance.HasTooltip(keyword))
                {
                    TooltipSystem.Instance.ShowTooltip(keyword, eventData.position);
                }
            }
            else
            {
                TooltipSystem.Instance.UpdateTooltipPosition(eventData.position);
            }
        }
        else if (lastHoveredKeyword != "")
        {
            lastHoveredKeyword = "";
            TooltipSystem.Instance.HideTooltip();
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isPointerOver = false;
        lastHoveredKeyword = "";
        if (TooltipSystem.Instance != null)
            TooltipSystem.Instance.HideTooltip();
    }
}
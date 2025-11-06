using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class TooltipSystem : MonoBehaviour
{
    public static TooltipSystem Instance { get; private set; }

    [Header("Tooltip UI")]
    [SerializeField] private GameObject tooltipPanel;
    [SerializeField] private TextMeshProUGUI tooltipText;
    [SerializeField] private RectTransform tooltipRect;
    [SerializeField] private Canvas canvas;
    [SerializeField] private float offsetX = 10f;
    [SerializeField] private float offsetY = 10f;

    private Dictionary<string, string> tooltipDatabase = new Dictionary<string, string>();
    private CanvasGroup tooltipCanvasGroup;
    private Canvas tooltipCanvas;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        tooltipCanvasGroup = tooltipPanel.GetComponent<CanvasGroup>();
        if (tooltipCanvasGroup == null)
        {
            tooltipCanvasGroup = tooltipPanel.AddComponent<CanvasGroup>();
        }
        tooltipCanvasGroup.blocksRaycasts = false;

        tooltipCanvas = tooltipPanel.GetComponent<Canvas>();
        if (tooltipCanvas == null)
        {
            tooltipCanvas = tooltipPanel.AddComponent<Canvas>();
        }
        tooltipCanvas.overrideSorting = true;
        tooltipCanvas.sortingOrder = 9999; 

        if (tooltipPanel.GetComponent<GraphicRaycaster>() == null)
        {
            tooltipPanel.AddComponent<GraphicRaycaster>();
        }

        InitializeTooltips();
        HideTooltip();
    }

    void InitializeTooltips()
    {
        tooltipDatabase.Add("bleed", "Deals flat damage every time the enemy is hit by an attack. \nDeals higher damage when reapplied");
        tooltipDatabase.Add("pixie dust", "Deals damage over time. \nCan be stacked up to 10 times to increase DPS");
    }

    public void ShowTooltip(string keyword, Vector2 position)
    {
        keyword = keyword.ToLower();

        if (tooltipDatabase.ContainsKey(keyword))
        {
            tooltipPanel.SetActive(true);
            tooltipText.text = tooltipDatabase[keyword];
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                position,
                canvas.worldCamera,
                out localPoint
            );

            tooltipRect.localPosition = localPoint + new Vector2(offsetX, offsetY);

            ClampToScreen();
        }
    }

    public void HideTooltip()
    {
        tooltipPanel.SetActive(false);
    }

    public void UpdateTooltipPosition(Vector2 position)
    {
        if (!tooltipPanel.activeSelf) return;

        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            position,
            canvas.worldCamera,
            out localPoint
        );

        tooltipRect.localPosition = localPoint + new Vector2(offsetX, offsetY);
        ClampToScreen();
    }

    private void ClampToScreen()
    {
        Vector3[] corners = new Vector3[4];
        tooltipRect.GetWorldCorners(corners);
        RectTransform canvasRect = canvas.transform as RectTransform;

        Vector3 pos = tooltipRect.localPosition;

        // Check right edge
        if (corners[2].x > Screen.width)
            pos.x -= (corners[2].x - Screen.width);

        // Check left edge
        if (corners[0].x < 0)
            pos.x -= corners[0].x;

        // Check top edge
        if (corners[1].y > Screen.height)
            pos.y -= (corners[1].y - Screen.height);

        // Check bottom edge
        if (corners[0].y < 0)
            pos.y -= corners[0].y;

        tooltipRect.localPosition = pos;
    }

    public bool HasTooltip(string keyword)
    {
        return tooltipDatabase.ContainsKey(keyword.ToLower());
    }
}
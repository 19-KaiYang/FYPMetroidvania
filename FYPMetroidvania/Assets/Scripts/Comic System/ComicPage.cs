using DG.Tweening;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class ComicPage : MonoBehaviour
{
    public RectTransform rectTransform;
    public List<ComicPanel> Panels;
    public int currentPanel = 0;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void InitialisePage()
    {
        currentPanel = 0;
        for (int i = 0; i < Panels.Count; i++)
        {
            var panel = Panels[i];
            if(i > 0)
            {
                panel.rectTransform.anchoredPosition = panel.startPosition;
            }
            panel.InitialiseDialogues();
        }
    }
    
    public bool TryNextPanel()
    {
        if (Panels[currentPanel].TryNextDialogue() == false)
        {
            currentPanel++;
            if (currentPanel < Panels.Count)
            {
                // Next panel
                NextPanel();
                return true;
            }
            else return false; // Final panel, page finished
        }
        else
        {
            return true; // Next dialogue in panel
        }

    }
    void NextPanel()
    {
        Panels[currentPanel].rectTransform.DOAnchorPos(Panels[currentPanel].endPosition, 0.5f);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}

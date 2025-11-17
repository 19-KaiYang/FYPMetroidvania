using DG.Tweening;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class ComicHandler : MonoBehaviour
{
    public List<ComicPage> Pages;
    public int currentPage;
    public RectTransform pageAnchor;

    [Header("View Settings")]
    public int pageWidth = 1620;
    public int pageSpacing = 200;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentPage = 0;
        InitializeComic();
    }
    void InitializeComic()
    {
        for (int i = 0; i < Pages.Count; i++)
        {
            var page = Pages[i];
            page.rectTransform.anchoredPosition = Vector2.zero + new Vector2(i * (pageWidth + pageSpacing), 0f);
            page.InitialisePage();
        }
    }
    // Update is called once per frame
    void Update()
    {
        // Handle all inputs
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
        {
            if (Pages[currentPage].TryNextPanel() == false)
            {
                currentPage++;
                if (currentPage < Pages.Count)
                {
                    NextPage();
                }
                else
                {
                    Debug.Log("Comic finished");
                }
            }
        }
    }

    void NextPage()
    {
        pageAnchor.DOAnchorPosX(pageAnchor.anchoredPosition.x - pageWidth - pageSpacing, 1f).SetEase(Ease.OutSine);
    }
}

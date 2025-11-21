using DG.Tweening;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ComicHandler : MonoBehaviour
{
    public List<ComicPage> Pages;
    public int currentPage;
    public RectTransform pageAnchor;
    public CanvasGroup fade;
    public Image autoplayHighlight;
    public bool bgm;

    [Header("View Settings")]
    public int pageWidth = 1620;
    public int pageSpacing = 200;
    public bool autoplaying;
    public float autoplaytime = 2f;
    private float autoTimer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        fade.alpha = 1f;
        fade.DOFade(0f, 0.5f);
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
        autoplaying = false;
        autoTimer = 0f;
        if(bgm) AudioManager.instance.PlayBGM(BGMType.ENDING_COMIC);
        Pages[currentPage].ActivatePage();
    }
    // Update is called once per frame
    void Update()
    {
        if (autoplaying)
        {
            autoTimer += Time.deltaTime;
            if(autoTimer > autoplaytime)
            {
                autoTimer = 0f;
                if (Pages[currentPage].TryNextPanel() == false)
                {
                    currentPage++;
                    if (currentPage < Pages.Count)
                    {
                        NextPage();
                    }
                    else
                    {

                        StartCoroutine(FadeOut());
                    }
                }
            }
        }
        // Handle all inputs
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
        {
            if (Pages[currentPage].TryNextPanel() == false)
            {
                currentPage++;
                if (currentPage < Pages.Count)
                {
                    NextPage();
                    Pages[currentPage].ActivatePage();
                }
                else
                {
                    StartCoroutine(FadeOut());
                }
            }
        }
        if(Input.GetKeyDown(KeyCode.Tab))
        {
            autoplaying = !autoplaying;
            if (autoplaying)
            {
                autoplayHighlight.gameObject.SetActive(true);
                autoplayHighlight.fillAmount = 0f;
                autoplayHighlight.DOFillAmount(1f, 3f).SetEase(Ease.Linear).SetLoops(-1, LoopType.Restart);
            }
            else
            {
                autoplayHighlight.gameObject.SetActive(false);
                autoplayHighlight.DOKill();
            }
        }
    }
    void NextPage()
    {
        pageAnchor.DOComplete();
        pageAnchor.DOAnchorPosX(pageAnchor.anchoredPosition.x - pageWidth - pageSpacing, 0.3f).SetEase(Ease.OutSine);
    }

    IEnumerator FadeOut()
    {
        fade.alpha = 0f;
        yield return fade.DOFade(1f, 3f);
        SceneManager.LoadScene("CreditsScreen");
    }
}

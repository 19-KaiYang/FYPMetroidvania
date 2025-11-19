using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class JournalNavigator : MonoBehaviour
{
    [Header("Page GameObjects")]
    public GameObject[] pages; 

    [Header("Navigation Buttons")]
    public Button nextButton;
    public Button previousButton;
    public Button mainmenuButton;

    private int currentPageIndex = 0;

    void Start()
    {
        nextButton.onClick.AddListener(NextPage);
        previousButton.onClick.AddListener(PreviousPage);
        mainmenuButton.onClick.AddListener(backToMainMenu);
        ShowPage(currentPageIndex);
    }

    void backToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    void ShowPage(int index)
    {
        // Disable all pages
        for (int i = 0; i < pages.Length; i++)
        {
            pages[i].SetActive(false);
        }
        if (index >= 0 && index < pages.Length)
        {
            pages[index].SetActive(true);
        }
        UpdateButtons();
    }

    void NextPage()
    {
        if (currentPageIndex < pages.Length - 1)
        {
            currentPageIndex++;
            ShowPage(currentPageIndex);
        }
    }

    void PreviousPage()
    {
        if (currentPageIndex > 0)
        {
            currentPageIndex--;
            ShowPage(currentPageIndex);
        }
    }

    void UpdateButtons()
    {
        previousButton.gameObject.SetActive(currentPageIndex > 0);
        nextButton.gameObject.SetActive(currentPageIndex < pages.Length - 1);
    }
}
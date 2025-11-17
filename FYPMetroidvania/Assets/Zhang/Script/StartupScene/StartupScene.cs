using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartupScene : MonoBehaviour
{
    public GameObject rrrat;
    public GameObject nypsdm;
    
    void Start()
    {
        
    }
    
    void Update()
    {
        
    }

    public void EnableRrrat()
    {
        rrrat.SetActive(true);
    }

    public void LoadMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}

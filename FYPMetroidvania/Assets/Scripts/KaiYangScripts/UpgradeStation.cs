using UnityEngine;

public class UpgradeStation : MonoBehaviour
{
    public GameObject upgradeUI; // Assign your UpgradeUI panel in Inspector
    private bool playerInRange = false;
    private bool isOpen = false;

    private void Start()
    {
        if (upgradeUI != null)
            upgradeUI.SetActive(false); // start hidden
    }

    private void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.H))
        {
            isOpen = !isOpen;
            upgradeUI.SetActive(isOpen);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInRange = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            if (isOpen)
            {
                isOpen = false;

                if (upgradeUI != null)  
                    upgradeUI.SetActive(false);
            }
        }
    }

}

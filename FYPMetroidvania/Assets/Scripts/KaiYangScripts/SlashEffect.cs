using System.Collections;
using UnityEngine;

public class SlashEffect : MonoBehaviour
{
    [Header("Slash Sprite")]
    [SerializeField] private GameObject slashSprite;

    [Header("Settings")]
    [SerializeField] private float displayDuration = 0.2f;

    private PlayerController playerController;

    void Start()
    {
        playerController = GetComponentInParent<PlayerController>();

        if (slashSprite != null)
        {
            slashSprite.SetActive(false);
        }
    }

    // Call this from Animation Events
    public void ShowSlash()
    {
        if (slashSprite != null)
        {
            // Flip slash if facing left
            if (playerController != null && !playerController.facingRight)
            {
                Vector3 scale = slashSprite.transform.localScale;
                scale.x = -Mathf.Abs(scale.x);
                slashSprite.transform.localScale = scale;
            }
            else
            {
                Vector3 scale = slashSprite.transform.localScale;
                scale.x = Mathf.Abs(scale.x);
                slashSprite.transform.localScale = scale;
            }

            StartCoroutine(ShowSlashCoroutine());
        }
    }

    private IEnumerator ShowSlashCoroutine()
    {
        slashSprite.SetActive(true);
        yield return new WaitForSeconds(displayDuration);
        slashSprite.SetActive(false);
    }
}
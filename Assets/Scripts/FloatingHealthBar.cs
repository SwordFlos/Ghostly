using UnityEngine;
using UnityEngine.UI;

public class FloatingHealthBar : MonoBehaviour
{
    [Header("Health Bar References")]
    public Image healthBarFill;
    public CanvasGroup canvasGroup;

    [Header("Flash Settings")]
    public float flashDuration = 0.3f;
    public Color flashColor = Color.red;
    public int flashCount = 2;

    private Transform mainCamera;
    private Player player;
    private Image healthBarBackground;
    private Color originalFillColor;
    private bool isFlashing = false;

    void Start()
    {
        mainCamera = Camera.main.transform;
        player = GetComponentInParent<Player>();
        healthBarBackground = healthBarFill.transform.parent.GetComponent<Image>();
        originalFillColor = healthBarFill.color;

        if (player != null && canvasGroup != null)
        {
            canvasGroup.alpha = 0f; // Start hidden
        }
    }

    void LateUpdate() // Use LateUpdate for billboarding to avoid jitter
    {
        if (mainCamera != null)
        {
            // Make the health bar always face the camera
            transform.LookAt(transform.position + mainCamera.forward);
        }

        if (player == null || canvasGroup == null) return;

        // Update health bar value
        float healthPercent = (float)player.GetCurrentHealth() / player.GetMaxHealth();
        healthBarFill.fillAmount = healthPercent;

        // Show bar only when not at full health
        if (healthPercent < 1f)
        {
            canvasGroup.alpha = 1f;
        }
        else
        {
            canvasGroup.alpha = 0f;
        }
    }

    public void FlashHealthBar()
    {
        if (!isFlashing)
        {
            StartCoroutine(FlashCoroutine());
        }
    }

    private System.Collections.IEnumerator FlashCoroutine()
    {
        isFlashing = true;

        for (int i = 0; i < flashCount; i++)
        {
            // Flash to red
            healthBarFill.color = flashColor;
            yield return new WaitForSeconds(flashDuration / (flashCount * 2));

            // Return to normal
            healthBarFill.color = originalFillColor;
            yield return new WaitForSeconds(flashDuration / (flashCount * 2));
        }

        isFlashing = false;
    }
}
using UnityEngine;
using UnityEngine.UI;

public class EssenceManager : MonoBehaviour
{
    [Header("Essence Settings")]
    public float totalEssenceRequired = 10f;
    public float essenceDepletionRate = 1f;
    public float initialDepletionDelay = 5f;

    [Header("UI Reference")]
    public Slider essenceSlider;

    public float currentEssence = 0f;
    private float gracePeriodTimer = 0f;
    private bool depletionStarted = false;

    void Start()
    {
        // Setup the slider
        if (essenceSlider != null)
        {
            essenceSlider.minValue = 0f;
            essenceSlider.maxValue = totalEssenceRequired;
            essenceSlider.value = currentEssence;
        }

        gracePeriodTimer = initialDepletionDelay;
    }

    void Update()
    {
        if (!depletionStarted)
        {
            // Countdown to when depletion starts
            gracePeriodTimer -= Time.deltaTime;
            gracePeriodTimer = Mathf.Max(gracePeriodTimer, 0f);

            if (gracePeriodTimer <= 0f)
            {
                depletionStarted = true;
            }
        }
        else
        {
            // Continuous depletion
            currentEssence -= essenceDepletionRate * Time.deltaTime;
            currentEssence = Mathf.Max(currentEssence, 0f);

            if (currentEssence <= 0f)
            {
                LoseGame();
            }
        }

        UpdateSlider();
        CheckWinCondition();
    }

    public void AddEssence(float amount)
    {
        currentEssence += amount;
        currentEssence = Mathf.Clamp(currentEssence, 0f, totalEssenceRequired);
        UpdateSlider();
    }

    private void UpdateSlider()
    {
        if (essenceSlider != null)
        {
            essenceSlider.value = currentEssence;
        }
    }

    private void CheckWinCondition()
    {
        if (currentEssence >= totalEssenceRequired)
        {
            WinGame();
        }
    }

    private void WinGame()
    {
        Debug.Log("You collected all essence! You win!");
        Time.timeScale = 0f;
        // Add your win screen logic here
    }

    private void LoseGame()
    {
        Debug.Log("You ran out of essence! Game Over!");
        Time.timeScale = 0f;
        // Add your lose screen logic here
    }
}
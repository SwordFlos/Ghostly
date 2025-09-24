using UnityEngine;
using TMPro;

public class FPSToggle : MonoBehaviour
{
    public TMP_Text fpsText; // Drag a TMP Text object here
    public KeyCode toggleKey = KeyCode.F1;

    private float updateInterval = 0.5f;
    private float accum = 0.0f;
    private int frames = 0;
    private float timeleft;
    private bool showFPS = false;

    void Start()
    {
        timeleft = updateInterval;

        if (fpsText != null)
        {
            fpsText.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        // Toggle FPS display
        if (Input.GetKeyDown(toggleKey))
        {
            showFPS = !showFPS;
            if (fpsText != null)
            {
                fpsText.gameObject.SetActive(showFPS);
            }
        }

        // Calculate FPS
        if (showFPS)
        {
            timeleft -= Time.deltaTime;
            accum += Time.timeScale / Time.deltaTime;
            frames++;

            if (timeleft <= 0.0f)
            {
                float fps = accum / frames;
                if (fpsText != null)
                {
                    fpsText.text = $"FPS: {fps:F1}";

                    // Optional: Color coding
                    if (fps < 30) fpsText.color = Color.red;
                    else if (fps < 60) fpsText.color = Color.yellow;
                    else fpsText.color = Color.green;
                }

                timeleft = updateInterval;
                accum = 0.0f;
                frames = 0;
            }
        }
    }
}
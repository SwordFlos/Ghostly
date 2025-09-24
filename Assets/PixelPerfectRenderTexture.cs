using UnityEngine;

[RequireComponent(typeof(Camera))]
public class PixelPerfectRenderTexture : MonoBehaviour
{
    [Header("Render Texture Settings")]
    public int targetWidth = 320;  // Your desired pixel resolution
    public int targetHeight = 240;
    public FilterMode filterMode = FilterMode.Point;

    private Camera renderCamera;
    private RenderTexture renderTexture;

    void Start()
    {
        renderCamera = GetComponent<Camera>();

        // Create render texture with point filtering
        renderTexture = new RenderTexture(targetWidth, targetHeight, 24);
        renderTexture.filterMode = filterMode;
        renderTexture.antiAliasing = 1; // No anti-aliasing!
        renderTexture.Create();

        // Assign to camera
        renderCamera.targetTexture = renderTexture;
    }

    void OnDestroy()
    {
        // Clean up
        if (renderTexture != null)
        {
            renderTexture.Release();
            Destroy(renderTexture);
        }
    }

    // Optional: Adjust resolution in real-time
    public void SetResolution(int width, int height)
    {
        if (renderTexture != null)
        {
            renderTexture.Release();
            renderTexture.width = width;
            renderTexture.height = height;
            renderTexture.Create();
        }
    }
}
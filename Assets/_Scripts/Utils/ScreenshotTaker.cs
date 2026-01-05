using UnityEngine;

public class ScreenshotTaker : MonoBehaviour
{
    [Header("Settings")]
    public int width = 1920;
    public int height = 1080;
    public string folderName = "Screenshots";

    void Update()
    {
        // Press 'K' (for Karakalpakstan or Keep!) to take the shot
        if (Input.GetKeyDown(KeyCode.K))
        {
            TakeScreenshot();
        }
    }

    public void TakeScreenshot()
    {
        // Create folder if it doesn't exist
        if (!System.IO.Directory.Exists(folderName))
        {
            System.IO.Directory.CreateDirectory(folderName);
        }

        string fileName = $"{folderName}/OneShotSale_{System.DateTime.Now:yyyy-MM-dd_HH-mm-ss}.png";
        
        // This captures the game at the specific resolution
        ScreenCapture.CaptureScreenshot(fileName);
        
        Debug.Log($"<color=green>Screenshot Saved to:</color> {fileName} at {width}x{height}");
    }
}
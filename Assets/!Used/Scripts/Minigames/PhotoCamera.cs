using UnityEngine;
using UnityEngine.InputSystem;
public class PhotoCamera : MonoBehaviour
{
    [SerializeField] private RenderTexture renderTexture;
    public enum SaveTextureFileFormat
    {
        EXR, JPG, PNG, TGA
    };


    public void TakePhoto()
    {
        if (renderTexture == null)
        {
            Debug.LogError("No RenderTexture assigned.");
            return;
        }

        string folder = System.IO.Path.Combine(Application.persistentDataPath, "Screenshots");
        string name = System.DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss-fff") + ".png";
        string filePath = System.IO.Path.Combine(folder, name);

        SaveTextureToFile(renderTexture, filePath, renderTexture.width, renderTexture.height, SaveTextureFileFormat.PNG, 95, true,success =>
        {
            if (success)
                Debug.Log("Photo taken!\nSaved as " + name);
            else
                Debug.LogError("Failed to save photo: " + filePath);
        });
    }


    private void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            TakePhoto();
        }
    }

    public static void SaveTextureToFile(Texture source, string filePath, int width, int height, SaveTextureFileFormat fileFormat = SaveTextureFileFormat.PNG, int jpgQuality = 95, bool asynchronous = true, System.Action<bool> done = null)
    {
        if (source == null ||!(source is Texture2D || source is RenderTexture) || string.IsNullOrWhiteSpace(filePath))
        {
            done?.Invoke(false);
            return;
        }

        if (width <= 0) width = source.width;
        if (height <= 0) height = source.height;

        RenderTexture resizeRT = null;
        try
        {
            string directory = System.IO.Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory))
                System.IO.Directory.CreateDirectory(directory);

            resizeRT = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32);

            Graphics.Blit(source, resizeRT);

            var graphicsFormat = resizeRT.graphicsFormat;
            var request = UnityEngine.Rendering.AsyncGPUReadback.Request(resizeRT, 0, readback =>
            {
                bool success = false;
                Unity.Collections.NativeArray<byte> encoded = default;
                try
                {
                    if (readback.hasError)
                        return;
                    var pixels = readback.GetData<byte>();
                    encoded = fileFormat switch
                    {
                        SaveTextureFileFormat.EXR => ImageConversion.EncodeNativeArrayToEXR(pixels, graphicsFormat, (uint)width, (uint)height),
                        SaveTextureFileFormat.JPG => ImageConversion.EncodeNativeArrayToJPG(pixels, graphicsFormat, (uint)width, (uint)height, 0, Mathf.Clamp(jpgQuality, 1, 100)),
                        SaveTextureFileFormat.TGA => ImageConversion.EncodeNativeArrayToTGA(pixels, graphicsFormat, (uint)width, (uint)height),
                        _ => ImageConversion.EncodeNativeArrayToPNG(pixels, graphicsFormat, (uint)width, (uint)height),
                    };
                    System.IO.File.WriteAllBytes(filePath, encoded.ToArray());
                    success = true;
                }
                catch (System.Exception e)
                {
                    Debug.LogException(e);
                }
                finally
                {
                    if (encoded.IsCreated)
                        encoded.Dispose();
                    RenderTexture.ReleaseTemporary(resizeRT);
                    done?.Invoke(success);
                }
            });

            if (!asynchronous)
                request.WaitForCompletion();
        }
        catch (System.Exception e)
        {
            if (resizeRT != null)
                RenderTexture.ReleaseTemporary(resizeRT);
            Debug.LogException(e);
            done?.Invoke(false);
        }
    }
}

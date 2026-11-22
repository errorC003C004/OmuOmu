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
        if (!System.IO.Directory.Exists(System.IO.Path.Combine(Application.persistentDataPath, "Screenshots")))
        {
            System.IO.Directory.CreateDirectory(System.IO.Path.Combine(Application.persistentDataPath, "Screenshots"));
        }
        var timestamp = System.DateTime.Now;
        string name = timestamp.ToString("yyyy-MM-dd HH-mm-ss-fff") + ".png";
        SaveTextureToFile(renderTexture, System.IO.Path.Combine(Application.persistentDataPath, "Screenshots/" + name), renderTexture.width, renderTexture.height, SaveTextureFileFormat.PNG, 95);
        Debug.Log("Photo taken!\nSaved As " + name);
    }


    private void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            TakePhoto();
        }
    }

    static public void SaveTextureToFile(Texture source, string filePath, int width, int height, SaveTextureFileFormat fileFormat = SaveTextureFileFormat.PNG, int jpgQuality = 95, bool asynchronous = true, System.Action<bool> done = null)
    {
        // check that the input we're getting is something we can handle:
        if (!(source is Texture2D || source is RenderTexture))
        {
            done?.Invoke(false);
            return;
        }

        // use the original texture size in case the input is negative:
        if (width < 0 || height < 0)
        {
            width = source.width;
            height = source.height;
        }

        // resize the original image:
        var resizeRT = RenderTexture.GetTemporary(width, height, 0);
        Graphics.Blit(source, resizeRT);

        // create a native array to receive data from the GPU:
        var narray = new Unity.Collections.NativeArray<byte>(width * height * 4, Unity.Collections.Allocator.Persistent, Unity.Collections.NativeArrayOptions.UninitializedMemory);

        // request the texture data back from the GPU:
        var request = UnityEngine.Rendering.AsyncGPUReadback.RequestIntoNativeArray (ref narray, resizeRT, 0, (UnityEngine.Rendering.AsyncGPUReadbackRequest request) =>
        {
            // if the readback was successful, encode and write the results to disk
            if (!request.hasError)
            {
                Unity.Collections.NativeArray<byte> encoded;

                switch (fileFormat)
                {
                    case SaveTextureFileFormat.EXR:
                        encoded = ImageConversion.EncodeNativeArrayToEXR(narray, resizeRT.graphicsFormat, (uint)width, (uint)height);
                        break;
                    case SaveTextureFileFormat.JPG:
                        encoded = ImageConversion.EncodeNativeArrayToJPG(narray, resizeRT.graphicsFormat, (uint)width, (uint)height, 0, jpgQuality);
                        break;
                    case SaveTextureFileFormat.TGA:
                        encoded = ImageConversion.EncodeNativeArrayToTGA(narray, resizeRT.graphicsFormat, (uint)width, (uint)height);
                        break;
                    default:
                        encoded = ImageConversion.EncodeNativeArrayToPNG(narray, resizeRT.graphicsFormat, (uint)width, (uint)height);
                        break;
                }

                System.IO.File.WriteAllBytes(filePath, encoded.ToArray());
                encoded.Dispose();
            }

            narray.Dispose();

            // notify the user that the operation is done, and its outcome.
            done?.Invoke(!request.hasError);
        });

        if (!asynchronous)
            request.WaitForCompletion();
    }
}

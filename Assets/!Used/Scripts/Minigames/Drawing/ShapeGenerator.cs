
using UnityEngine;

public static class ShapeGenerator
{
    public static bool[] GenerateBox(int width, int height, int inset, int thickness)
    {
        bool[] mask = new bool[width * height];

        for (int y = inset; y < height - inset; y++)
        {
            for (int x = inset; x < width - inset; x++)
            {
                bool edge =
                    x < inset + thickness ||
                    x >= width - inset - thickness ||
                    y < inset + thickness ||
                    y >= height - inset - thickness;

                if (edge)
                    mask[y * width + x] = true;
            }
        }

        return mask;
    }

    public static bool[] GenerateMask(Texture2D source, int width, int height, out Texture2D displayTexture)
    {
        bool[] mask = new bool[width * height];
        Color32[] displayPixels = new Color32[width * height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Color32 pixel = source.GetPixelBilinear(
                    (x + 0.5f) / width,
                    (y + 0.5f) / height
                );

                bool isShape = pixel.a > 128 &&
                    (pixel.r + pixel.g + pixel.b) / 3 < 200;

                int index = y * width + x;

                mask[index] = isShape;

                displayPixels[index] = isShape
                    ? new Color32(160, 160, 160, 255)
                    : new Color32(0, 0, 0, 0);
            }
        }

        displayTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        displayTexture.filterMode = FilterMode.Bilinear;
        displayTexture.wrapMode = TextureWrapMode.Clamp;
        displayTexture.SetPixels32(displayPixels);
        displayTexture.Apply();

        return mask;
    }

    public static Texture2D CreateReferenceTexture(int width, int height, bool[] mask)
    {
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        Color32[] pixels = new Color32[width * height];

        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = mask[i]
                ? new Color32(160, 160, 160, 255)
                : new Color32(0, 0, 0, 0);

        texture.SetPixels32(pixels);
        texture.Apply();

        return texture;
    }
}

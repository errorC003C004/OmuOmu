
using UnityEngine;

public static class DrawingGrader
{
    public static float CalculateAccuracy(Texture2D drawing, bool[] reference, int tolerance)
    {
        int width = drawing.width;
        int height = drawing.height;

        Color32[] pixels = drawing.GetPixels32();

        bool[] playerMask = new bool[width * height];

        int playerCount = 0;
        int referenceCount = 0;

        for (int i = 0; i < pixels.Length; i++)
        {
            playerMask[i] = pixels[i].a > 0;

            if (playerMask[i])
                playerCount++;

            if (reference[i])
                referenceCount++;
        }

        if (playerCount == 0 || referenceCount == 0)
            return 0f;

        int matchedPlayer = 0;
        int matchedReference = 0;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int index = y * width + x;

                bool nearReference = false;
                bool nearPlayer = false;

                for (int dy = -tolerance; dy <= tolerance; dy++)
                {
                    for (int dx = -tolerance; dx <= tolerance; dx++)
                    {
                        if (dx * dx + dy * dy > tolerance * tolerance)
                            continue;

                        int nx = x + dx;
                        int ny = y + dy;

                        if (nx < 0 || nx >= width || ny < 0 || ny >= height)
                            continue;

                        int neighbor = ny * width + nx;

                        if (reference[neighbor])
                            nearReference = true;

                        if (playerMask[neighbor])
                            nearPlayer = true;
                    }
                }

                if (playerMask[index] && nearReference)
                    matchedPlayer++;

                if (reference[index] && nearPlayer)
                    matchedReference++;
            }
        }

        float precision = (float)matchedPlayer / playerCount;
        float coverage = (float)matchedReference / referenceCount;

        if (precision + coverage == 0)
            return 0f;

        float f1 = 2f * precision * coverage / (precision + coverage);

        return Mathf.Clamp(f1 * 100f, 0f, 100f);
    }
}

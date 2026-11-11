
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class DrawingManager : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    [Header("Drawing")]
    [SerializeField] private RawImage playerDrawing;
    [SerializeField] private RawImage referenceImage;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private Slider pencilSizeSlider;

    [Header("Settings")]
    [SerializeField] private Texture2D[] customReference;
    [SerializeField] private int textureWidth = 256;
    [SerializeField] private int textureHeight = 256;
    [SerializeField] private int gradingTolerance = 5;

    private Texture2D drawingTexture;
    private Texture2D referenceTexture;
    private bool[] referenceMask;

    private RectTransform drawingRect;
    private Vector2 previousPosition;
    private bool hasPreviousPosition;
    private bool submitted;

    private void Start()
    {
        referenceImage.enabled = true;
        playerDrawing.enabled = true;

        drawingRect = playerDrawing.rectTransform;

        drawingTexture = new Texture2D(
            textureWidth,
            textureHeight,
            TextureFormat.RGBA32,
            false
        );

        drawingTexture.filterMode = FilterMode.Point;

        referenceMask = ShapeGenerator.GenerateMask(
            customReference[Random.Range(0, customReference.Length)],
            textureWidth,
            textureHeight,
            out referenceTexture
        );

        referenceImage.texture = referenceTexture;
        playerDrawing.texture = drawingTexture;

        ClearDrawing();

        pencilSizeSlider.minValue = 1;
        pencilSizeSlider.maxValue = 12;
        pencilSizeSlider.value = 4;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (submitted)
            return;

        if (TryGetDrawingPosition(eventData, out Vector2 position))
        {
            previousPosition = position;
            hasPreviousPosition = true;

            DrawCircle(
                Mathf.RoundToInt(position.x),
                Mathf.RoundToInt(position.y)
            );

            drawingTexture.Apply();
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (submitted || !hasPreviousPosition)
            return;

        if (TryGetDrawingPosition(eventData, out Vector2 position))
        {
            DrawLine(previousPosition, position);

            previousPosition = position;

            drawingTexture.Apply();
        }
        else
        {
            hasPreviousPosition = false;
        }
    }

    private bool TryGetDrawingPosition(PointerEventData eventData, out Vector2 position)
    {
        position = Vector2.zero;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
            drawingRect,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPoint))
            return false;

        Rect rect = drawingRect.rect;

        float normalizedX = (localPoint.x - rect.xMin) / rect.width;
        float normalizedY = (localPoint.y - rect.yMin) / rect.height;

        if (normalizedX < 0 || normalizedX > 1 ||
            normalizedY < 0 || normalizedY > 1)
            return false;

        position = new Vector2(
            normalizedX * (textureWidth - 1),
            normalizedY * (textureHeight - 1)
        );

        return true;
    }

    private void DrawLine(Vector2 start, Vector2 end)
    {
        float distance = Vector2.Distance(start, end);

        int steps = Mathf.Max(1, Mathf.CeilToInt(distance));

        for (int i = 0; i <= steps; i++)
        {
            float t = (float)i / steps;

            Vector2 point = Vector2.Lerp(start, end, t);

            DrawCircle(
                Mathf.RoundToInt(point.x),
                Mathf.RoundToInt(point.y)
            );
        }
    }

    private void DrawCircle(int centerX, int centerY)
    {
        int radius = Mathf.RoundToInt(pencilSizeSlider.value / 2f);

        for (int y = -radius; y <= radius; y++)
        {
            for (int x = -radius; x <= radius; x++)
            {
                if (x * x + y * y > radius * radius)
                    continue;

                int px = centerX + x;
                int py = centerY + y;

                if (px < 0 || px >= textureWidth ||
                    py < 0 || py >= textureHeight)
                    continue;

                drawingTexture.SetPixel(px, py, Color.black);
            }
        }
    }

    public void ClearDrawing()
    {
        Color32[] pixels = new Color32[textureWidth * textureHeight];

        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = new Color32(0, 0, 0, 0);

        drawingTexture.SetPixels32(pixels);
        drawingTexture.Apply();

        hasPreviousPosition = false;
        submitted = false;

        if (scoreText != null)
            scoreText.text = "";
    }

    public void SubmitDrawing()
    {
        if (submitted)
            return;

        submitted = true;
        hasPreviousPosition = false;

        float accuracy = DrawingGrader.CalculateAccuracy(
            drawingTexture,
            referenceMask,
            gradingTolerance
        );

        scoreText.text = "Accuracy: " + accuracy.ToString("F1") + "%";
    }

    private void OnDestroy()
    {
        if (drawingTexture != null)
            Destroy(drawingTexture);

        if (referenceTexture != null)
            Destroy(referenceTexture);
    }
}

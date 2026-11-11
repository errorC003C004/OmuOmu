using UnityEngine;
using UnityEngine.InputSystem;

public class BasketballThrow : MonoBehaviour
{
    public static BasketballThrow Instance;
    public bool canThrow;
    [SerializeField] private GameObject basketballPrefab;
    [SerializeField] private float throwForceMin = 3f;
    [SerializeField] private float throwForceCurrent;
    [SerializeField] private float throwForceMax = 7.5f;
    [SerializeField] private float chargeTime = 3f; //secs

    private LineRenderer lineRenderer;
    Vector3 start = Vector3.zero; // Start point
    Vector3 middle = Vector3.zero; // Control point
    Vector3 end = Vector3.zero; // End point

    [SerializeField] private float previewDuration = 4f;
    private const int TrajectoryPoints = 21;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        throwForceCurrent = throwForceMin;
        transform.localPosition = new Vector3(0.4f, -0.333f, 0.35f);
        lineRenderer = GetComponentInChildren<LineRenderer>();
        lineRenderer.positionCount = TrajectoryPoints;
    }

    void Update()
    {
        MyInput();
        CalculateTrajectory();
        DrawCurve();
    }

    private void MyInput()
    {
        if (canThrow && Mouse.current.leftButton.isPressed)
        {
            float charge = Mathf.Clamp01(
                (throwForceCurrent - throwForceMin) / (throwForceMax - throwForceMin)
            );

            charge = Mathf.Clamp01(charge + Time.deltaTime / chargeTime);
            Inventory.Instance.BasketballCharge.SetActive(true);
            Inventory.Instance.BasketballChargeFill.fillAmount = charge;
            throwForceCurrent = Mathf.Lerp(throwForceMin, throwForceMax, charge);
        }
        if (canThrow && (Mouse.current.leftButton.wasReleasedThisFrame || throwForceCurrent == throwForceMax))
        {
            ThrowBasketball();
        }
    }

    private void ThrowBasketball()
    {
        Inventory.Instance.BasketballCharge.SetActive(false);
        Inventory.Instance.BasketballChargeFill.fillAmount = 0;
        GameObject basketball = Instantiate(basketballPrefab, transform.position, Quaternion.identity);
        Rigidbody rb = basketball.GetComponent<Rigidbody>();
        Vector3 combinedForce = (Camera.main.transform.up * 2) + (Camera.main.transform.forward * 2);
        rb.AddForce(combinedForce * throwForceCurrent, ForceMode.Impulse);

        Inventory.Instance.UseItem(gameObject);
    }

    private void CalculateTrajectory()
    {
        start = transform.position;

        Vector3 impulseDirection =
            (Camera.main.transform.up * 2f) +
            (Camera.main.transform.forward * 2f);

        Rigidbody prefabRb = basketballPrefab.GetComponent<Rigidbody>();
        float mass = prefabRb != null ? prefabRb.mass : 1f;

        // AddForce(..., ForceMode.Impulse) changes velocity by impulse / mass.
        Vector3 initialVelocity = impulseDirection * throwForceCurrent / mass;

        // These control points make the quadratic Bézier match the ballistic path.
        middle = start + initialVelocity * (previewDuration * 0.5f);

        end = start
            + initialVelocity * previewDuration
            + 0.5f * previewDuration * previewDuration * Physics.gravity;
    }


    void DrawCurve()
    {
        for (int i = 0; i < TrajectoryPoints; i++)
        {
            float t = i / (float)(TrajectoryPoints - 1);
            lineRenderer.SetPosition(i, CalculateBezierPoint(t, start, middle, end));
        }
    }

    Vector3 CalculateBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
    {
        return Mathf.Pow(1 - t, 2) * p0 + 2 * (1 - t) * t * p1 + Mathf.Pow(t, 2) * p2;
    }
}

using UnityEngine;
using UnityEngine.InputSystem;

public class CamLook : MonoBehaviour
{
    public bool CanLook = true;
    public bool LookAffectsTransform;
    public Transform Player;
    [SerializeField] private float sens = 15f;
    float xRotation = 0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        if (!CanLook) return;
        float mouseX = Mouse.current.delta.x.ReadValue() * sens * Time.deltaTime;
        float mouseY = Mouse.current.delta.y.ReadValue() * sens * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.parent.Rotate(Vector3.up * mouseX);
    }
}

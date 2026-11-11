using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    private CamLook cam;
    
    [SerializeField] private InputActionAsset inputActions;
    private InputAction jumpAction;
    private InputAction sprintAction;
    private InputAction moveAction;
    private Vector2 moveInput;


    private Rigidbody rb;

    [Header("Values")]
    public bool canMove = true;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float sprintMultiplier = 2f;
    [SerializeField] private float jumpForce = 5f;
    private bool isGrounded = false;

    private void Awake () {
        jumpAction = inputActions.FindAction("Jump");
        if (jumpAction == null) Debug.LogError("<color=black>[Player Movement] </color>Jump action not found in InputActionAsset.");

        sprintAction = inputActions.FindAction("Sprint");
        if (sprintAction == null) Debug.LogError("<color=black>[Player Movement] </color>Sprint action not found in InputActionAsset.");

        moveAction = inputActions.FindAction("Move");
        if (moveAction == null) Debug.LogError("<color=black>[Player Movement] </color>Move action not found in InputActionAsset.");
    }

    private void Start()
    {
        CheckNulls();
    }

    private void Update()
    {
        MyInput();
    }

    private void CheckNulls()
    {
        cam = GetComponentInChildren<CamLook>();
        if (cam == null) Debug.LogError("<color=black>[Player Movement] </color>CamLook component not found in children of the player.");

        rb = GetComponent<Rigidbody>();
        if (rb == null) Debug.LogError("<color=black>[Player Movement] </color>Rigidbody component not found on the player.");
    }

    void OnEnable()
    {
        jumpAction?.Enable();
        sprintAction?.Enable();
        moveAction?.Enable();
    }

    void OnDisable()
    {
        jumpAction?.Disable();
        sprintAction?.Disable();
        moveAction?.Disable();
    }


    private void MyInput()
    {
        if (jumpAction.WasPressedThisFrame())
        {
            //Debug.Log("<color=black>[Player Movement] </color>Jump action triggered.");
            Jump();
        }

        if (canMove)
        {
            // make it use rigidbody instead of transform.position += movement
            Vector2 input = moveAction.ReadValue<Vector2>();
            //print("<color=black>[Player Movement] </color>Move action triggered with input: " + input);
            Vector3 movement = new(input.x, 0f, input.y);
            movement = cam.transform.parent.TransformDirection(movement);
            float speed = moveSpeed;
            if (sprintAction.IsPressed())
            {
                speed *= sprintMultiplier; // Multiply the speed when sprinting
            }
            rb.linearVelocity = new Vector3(movement.x * speed, rb.linearVelocity.y, movement.z * speed);
        }
    }

    private void Jump()
    {
        if (isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground") || collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            isGrounded = true;
            //Debug.Log("<color=black>[Player Movement] </color>Player has landed on the ground.");
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground") || collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            isGrounded = false;
            //Debug.Log("<color=black>[Player Movement] </color>Player has left the ground.");
        }
    }
}

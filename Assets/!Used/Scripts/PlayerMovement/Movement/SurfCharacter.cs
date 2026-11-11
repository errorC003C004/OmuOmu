using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;

namespace Fragsurf.Movement {
    [AddComponentMenu ("Fragsurf/Surf Character")]
    public class SurfCharacter : MonoBehaviour, ISurfControllable {

        [Header("T")]

        [SerializeField] private InputActionAsset inputActions;
        private InputAction crouchAction;
        private InputAction jumpAction;
        private InputAction sprintAction;
        private InputAction moveAction;
        private Vector2 moveInput;


        public enum ColliderType {
            Capsule,
            Box
        }
        [Header("Physics Settings")]
        public bool isMoving = false;
        public bool isJumping = false;
        public bool isCrouching => _moveData.crouching;

        [SerializeField] private bool canBhop; 

        [SerializeField] private float jumpTimerMax;

        [SerializeField] private float jumpTimerCurrent;

        private bool wasGrounded;

        public Vector3 colliderSize = new Vector3 (1f, 2f, 1f);
        public ColliderType collisionType { get { return ColliderType.Box; } }
        public float weight = 75f;
        public float rigidbodyPushForce = 2f;
        public bool solidCollider = false;

        [Header("View Settings")]
        public Transform viewTransform;
        public Transform playerRotationTransform;

        [Header ("Crouching setup")]
        public float crouchingHeightMultiplier = 0.5f;
        public float crouchingSpeed = 10f;
        float defaultHeight;
        bool allowCrouch = true;

        [Header ("Features")]
        public bool crouchingEnabled = true;
        public bool slidingEnabled = false;
        public bool laddersEnabled = true;
        public bool supportAngledLadders = true;

        [Header("ADS Settings")]
        public float adsSpeedMultiplier = 0.5f; // 0.5 = half speed (2x slower)
        public bool isADS;

        [Header ("Step offset (can be buggy, enable at your own risk)")]
        public bool useStepOffset = false;
        
        public float stepOffset = 0.35f;

        [Header ("Movement Config")]
        [SerializeField]
        public MovementConfig movementConfig;
        
        private GameObject _groundObject;
        private Vector3 _baseVelocity;
        private Collider _collider;
        private Vector3 _angles;
        private Vector3 _startPosition;
        private GameObject _colliderObject;
        private GameObject _cameraWaterCheckObject;
        private CameraWaterCheck _cameraWaterCheck;

        private MoveData _moveData = new MoveData ();
        private SurfController _controller;

        private Rigidbody rb;

        private List<Collider> triggers = new List<Collider> ();
        private int numberOfTriggers = 0;

        private bool underwater = false;

        [Header("Vaulting Settings")]
        [SerializeField] private float vaultMaxHeight = 1.5f;
        //[SerializeField] private float vaultSpeed = 5f;
        private bool isVaulting = false;
        ///// Properties /////

        public MoveType moveType { get { return MoveType.Walk; } }
        public MovementConfig moveConfig { get { return movementConfig; } }
        public MoveData moveData { get { return _moveData; } }
        public new Collider collider { get { return _collider; } }

        public GameObject groundObject {

            get { return _groundObject; }
            set { _groundObject = value; }

        }

        public Vector3 baseVelocity { get { return _baseVelocity; } }

        public Vector3 forward { get { return viewTransform.forward; } }
        public Vector3 right { get { return viewTransform.right; } }
        public Vector3 up { get { return viewTransform.up; } }

        private BoxCollider boxc;

        Vector3 prevPosition;
        ///// Methods /////

		private void OnDrawGizmos()
		{
            Gizmos.color = Color.red;
            if (boxc != null)
            {
                Gizmos.DrawWireCube( transform.position, new Vector3 (boxc.size.x, boxc.size.y, boxc.size.z) );
            }
            else
            {
                Gizmos.DrawWireCube( transform.position, colliderSize );
            }
		}
		
        private void Awake () {
            _controller = new SurfController(inputActions)
            {
                playerTransform = playerRotationTransform
            };
            crouchAction = inputActions.FindAction("Crouch");
            jumpAction = inputActions.FindAction("Jump");
            sprintAction = inputActions.FindAction("Sprint");
            moveAction = inputActions.FindAction("Move");
            if (viewTransform != null) {

                _controller.camera = viewTransform;
                _controller.cameraYPos = viewTransform.localPosition.y;

            }

        }

        public bool isGrounded() {
            if(_groundObject != null) {
                _moveData.grounded = true;
                return true;
            }
            else {
                _moveData.grounded = false;
                return false;
            }
        }
        private void Start () {
            
            _colliderObject = new GameObject ("PlayerCollider");
            _colliderObject.layer = gameObject.layer;
            _colliderObject.transform.SetParent (transform);
            _colliderObject.transform.rotation = Quaternion.identity;
            _colliderObject.transform.localPosition = Vector3.zero;
            _colliderObject.transform.SetSiblingIndex (0);

            // Water check
            _cameraWaterCheckObject = new GameObject ("Camera water check");
            _cameraWaterCheckObject.layer = gameObject.layer;
            _cameraWaterCheckObject.transform.position = viewTransform.position;

            SphereCollider _cameraWaterCheckSphere = _cameraWaterCheckObject.AddComponent<SphereCollider> ();
            _cameraWaterCheckSphere.radius = 0.1f;
            _cameraWaterCheckSphere.isTrigger = true;

            Rigidbody _cameraWaterCheckRb = _cameraWaterCheckObject.AddComponent<Rigidbody> ();
            _cameraWaterCheckRb.useGravity = false;
            _cameraWaterCheckRb.isKinematic = true;

            _cameraWaterCheck = _cameraWaterCheckObject.AddComponent<CameraWaterCheck> ();

            prevPosition = transform.position;

            if (viewTransform == null)
                viewTransform = Camera.main.transform;

            if (playerRotationTransform == null && transform.childCount > 0)
                playerRotationTransform = transform.GetChild (0);

            _collider = gameObject.GetComponent<Collider> ();

            if (_collider != null)
                GameObject.Destroy (_collider);

            rb = gameObject.GetComponent<Rigidbody> ();
            if (rb == null)
                rb = gameObject.AddComponent<Rigidbody> ();

            allowCrouch = crouchingEnabled;

            rb.isKinematic = true;
            rb.useGravity = false;
            rb.angularDamping = 0f;
            rb.linearDamping = 0f;
            rb.mass = weight;


            switch (collisionType) {

                case ColliderType.Box:

                _collider = _colliderObject.AddComponent<BoxCollider> ();

                boxc = (BoxCollider)_collider;
                boxc.size = colliderSize;
                boxc.tag = "Player";

                defaultHeight = boxc.size.y;

                break;
                case ColliderType.Capsule:

                _collider = _colliderObject.AddComponent<CapsuleCollider> ();

                var capc = (CapsuleCollider)_collider;
                capc.height = colliderSize.y;
                capc.radius = colliderSize.x / 2f;

                defaultHeight = capc.height;

                break;

            }

            _moveData.slopeLimit = movementConfig.slopeLimit;

            _moveData.rigidbodyPushForce = rigidbodyPushForce;

            _moveData.slidingEnabled = slidingEnabled;
            _moveData.laddersEnabled = laddersEnabled;
            _moveData.angledLaddersEnabled = supportAngledLadders;

            _moveData.playerTransform = transform;
            _moveData.viewTransform = viewTransform;
            _moveData.viewTransformDefaultLocalPos = viewTransform.localPosition;

            _moveData.defaultHeight = defaultHeight;
            _moveData.crouchingHeight = crouchingHeightMultiplier;
            _moveData.crouchingSpeed = crouchingSpeed;
            
            _collider.isTrigger = !solidCollider;
            _moveData.origin = transform.position;
            _startPosition = transform.position;

            _moveData.useStepOffset = useStepOffset;
            _moveData.stepOffset = stepOffset;

        }



        private void Update()
        {
            bool grounded = isGrounded();

            // Player has just landed.
            if (grounded && !wasGrounded)
            {
                if (!canBhop)
                    StartJumpTimer();
            }

            if (!canBhop)
                UpdateJumpTimer();

            wasGrounded = grounded;

            _colliderObject.transform.rotation = Quaternion.identity;
            if (!isVaulting || vaultMaxHeight == 0f) {
                CheckForVault();
            }
            UpdateMoveData ();

            Vector3 positionalMovement = transform.position - prevPosition;
            transform.position = prevPosition;
            moveData.origin += positionalMovement;

            if (numberOfTriggers != triggers.Count) {
                numberOfTriggers = triggers.Count;

                underwater = false;
                triggers.RemoveAll (item => item == null);
                foreach (Collider trigger in triggers) {

                    if (trigger == null)
                        continue;

                    if (trigger.GetComponentInParent<Water> ())
                        underwater = true;

                }

            }

            _moveData.cameraUnderwater = _cameraWaterCheck.IsUnderwater ();
            _cameraWaterCheckObject.transform.position = viewTransform.position;
            moveData.underwater = underwater;
            
            if (allowCrouch)
                _controller.Crouch (this, movementConfig, Time.deltaTime);

            _controller.ProcessMovement (this, movementConfig, Time.deltaTime);
            if (isADS)
            {
                moveData.velocity.x *= adsSpeedMultiplier;
                moveData.velocity.z *= adsSpeedMultiplier;
            }

            transform.position = moveData.origin;
            prevPosition = transform.position;

            _colliderObject.transform.rotation = Quaternion.identity;
            isMoving = _moveData.velocity.x != 0f || _moveData.velocity.z != 0f;
            isJumping = _moveData.velocity.y != 0f;
        }

    private void CheckForVault() {
        RaycastHit firstHit;

        Vector3 rayStart = transform.position + Vector3.up * 0.5f;
        Vector3 rayDirection = new Vector3(forward.x, 0f, forward.z).normalized;

        int layerMask = ~(1 << gameObject.layer);

        Debug.DrawLine(rayStart, rayStart + rayDirection * 1f, Color.blue);

        if (Physics.Raycast(rayStart, rayDirection, out firstHit, 1f, layerMask)) {
            // /Debug.Log("Vaultable surface detected in front: " + firstHit.collider.name);

            if (firstHit.collider != null) {
                float ledgeHeight = firstHit.point.y - transform.position.y;

                if (ledgeHeight > 0.2f && ledgeHeight <= vaultMaxHeight) {
                    Vector3 secondRayStart = firstHit.point + (rayDirection * 0.5f) + (Vector3.up * (0.8f * vaultMaxHeight));  // Adjust height to be higher
                    Vector3 secondRayDirection = Vector3.down;

                    Debug.DrawLine(secondRayStart, secondRayStart + secondRayDirection * vaultMaxHeight, Color.yellow);  // Draw the angled ray in yellow

                    RaycastHit secondHit;
                    if (Physics.Raycast(secondRayStart, secondRayDirection, out secondHit, vaultMaxHeight, layerMask)) {
                        // /Debug.Log("Found valid place to land: " + secondHit.point);

                        if (jumpAction.triggered) {
                            // /Debug.Log("Jump button pressed, starting vault.");
                            StartCoroutine(PerformVault(secondHit.point, ledgeHeight));
                        }
                    } else {
                        // /Debug.Log("No valid landing spot detected; likely a wall or drop-off.");
                    }
                } else {
                    // /Debug.Log("Detected object is too high to vault.");
                }
            }
        }
    }

    private IEnumerator PerformVault(Vector3 targetPosition, float ledgeHeight) {
        Debug.Log("You want to vault");
        isVaulting = true;
        Collider playerCollider = GetComponent<Collider>();

        if (playerCollider != null) playerCollider.enabled = false;

        Vector3 startPosition = transform.position;
        float duration = 0.5f;
        Vector3 adjustedTargetPosition = new Vector3(targetPosition.x, targetPosition.y + 0.1f + ledgeHeight, targetPosition.z);  // Ensure higher vault

        float elapsedTime = 0f;
        while (elapsedTime < duration) {
            transform.position = Vector3.Lerp(startPosition, adjustedTargetPosition, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        transform.position = adjustedTargetPosition;
        if (playerCollider != null) playerCollider.enabled = true;

        isVaulting = false;
    }
        private void ResetPosition () {
            
            moveData.velocity = Vector3.zero;
            moveData.origin = _startPosition;

        }

        private void UpdateMoveData () {
            Vector2 moveInput = moveAction.ReadValue<Vector2>();
            _moveData.verticalAxis = moveInput.y;
            _moveData.horizontalAxis = moveInput.x;

            _moveData.sprinting = sprintAction.IsPressed();
            if (crouchAction.WasPressedThisFrame())
                _moveData.crouching = true;

            if (!crouchAction.IsPressed())
                _moveData.crouching = false;
            
            //if (Input.GetKeyDown(KeyCode.Comma)) Time.timeScale -= .5f;
            //if (Input.GetKeyDown(KeyCode.Period)) Time.timeScale += .5f;
            bool moveLeft = _moveData.horizontalAxis < 0f;
            bool moveRight = _moveData.horizontalAxis > 0f;
            bool moveFwd = _moveData.verticalAxis > 0f;
            bool moveBack = _moveData.verticalAxis < 0f;
            float adsMultiplier = isADS ? adsSpeedMultiplier : 1f;

            if (!moveLeft && !moveRight)
                _moveData.sideMove = 0f;
            else if (moveLeft)
                _moveData.sideMove = -moveConfig.acceleration * adsMultiplier;
            else if (moveRight)
                _moveData.sideMove = moveConfig.acceleration * adsMultiplier;

            if (!moveFwd && !moveBack)
                _moveData.forwardMove = 0f;
            else if (moveFwd)
                _moveData.forwardMove = moveConfig.acceleration * adsMultiplier;
            else if (moveBack)
                _moveData.forwardMove = -moveConfig.acceleration * adsMultiplier;
            
            if (jumpAction.triggered)
                Jump();

            if (!jumpAction.IsPressed())
                _moveData.wishJump = false;
            
            _moveData.viewAngles = _angles;

        }

    private void Jump()
    {
        if (canBhop)
        {
            _moveData.wishJump = true;
            return;
        }

        if (jumpTimerCurrent > 0f)
            return;

        _moveData.wishJump = true;
    }


    private void StartJumpTimer()
    {
        jumpTimerCurrent = jumpTimerMax;
    }

    private void UpdateJumpTimer()
    {
        if (jumpTimerCurrent <= 0f)
            return;

        jumpTimerCurrent -= Time.deltaTime;

        if (jumpTimerCurrent < 0f)
            jumpTimerCurrent = 0f;
    }

    private void OnEnable()
    {
        jumpAction?.Enable();
        crouchAction?.Enable();
        sprintAction?.Enable();
    }

    private void OnDisable()
    {
        jumpAction?.Disable();
        crouchAction?.Disable();
        sprintAction?.Disable();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

        private void DisableInput () {

            _moveData.verticalAxis = 0f;
            _moveData.horizontalAxis = 0f;
            _moveData.sideMove = 0f;
            _moveData.forwardMove = 0f;
            _moveData.wishJump = false;

        }
        public static float ClampAngle (float angle, float from, float to) {

            if (angle < 0f)
                angle = 360 + angle;

            if (angle > 180f)
                return Mathf.Max (angle, 360 + from);

            return Mathf.Min (angle, to);

        }

        private void OnTriggerEnter (Collider other) {
            
            if (!triggers.Contains (other))
                triggers.Add (other);

        }

        private void OnTriggerExit (Collider other) {
            
            if (triggers.Contains (other))
                triggers.Remove (other);

        }

        private void OnCollisionStay (Collision collision) {

            if (collision.rigidbody == null)
                return;

            Vector3 relativeVelocity = collision.relativeVelocity * collision.rigidbody.mass / 50f;
            Vector3 impactVelocity = new Vector3 (relativeVelocity.x * 0.0025f, relativeVelocity.y * 0.00025f, relativeVelocity.z * 0.0025f);

            float maxYVel = Mathf.Max (moveData.velocity.y, 10f);
            Vector3 newVelocity = new Vector3 (moveData.velocity.x + impactVelocity.x, Mathf.Clamp (moveData.velocity.y + Mathf.Clamp (impactVelocity.y, -0.5f, 0.5f), -maxYVel, maxYVel), moveData.velocity.z + impactVelocity.z);

            newVelocity = Vector3.ClampMagnitude (newVelocity, Mathf.Max (moveData.velocity.magnitude, 30f));
            moveData.velocity = newVelocity;

        }

    }
}

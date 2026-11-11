using UnityEngine;
using UnityEngine.InputSystem;
public class MoveCamera : MonoBehaviour
{
	public static MoveCamera Instance;

	[Header("Useable")]
	public bool canLook = true;
	public bool lookAffectsTransform = true;

	[Header("Cameras")]
	public Camera viewCamera;

	[Header("References")]
	public Transform bodyTransform;

	[Header("Sensitivity")]
	public float sensitivityMultiplier = 1f;
	public float horizontalSensitivity = 1f;
	public float verticalSensitivity = 1f;

	[Header("Restrictions")]
	public float minYRotation = -90f;
	public float maxYRotation = 90f;

	[Header("Free Look")]
	[Tooltip("Maximum angle the camera can turn away from the body.")]
	public float maxFreeLookAngle = 90f;

	[Header("Aimpunch")]
	[Tooltip("Bigger number makes the response more damped.")]
	public float punchDamping = 9f;

	[Tooltip("Bigger number increases the speed at which the view corrects.")]
	public float punchSpringConstant = 65f;

	[HideInInspector]
	public Vector2 punchAngle;

	[HideInInspector]
	public Vector2 punchAngleVel;

	private Vector3 realRotation;

	private float bodyYaw;
	private float freeLookYaw;

	private void Awake()
	{
		Instance = this;
	}

	private void Start()
	{
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;

		bodyYaw = bodyTransform.eulerAngles.y;
		realRotation.y = bodyYaw;

		SyncViewCamera();
	}


	private void Update()
	{

		if (!canLook || Time.timeScale <= 0f)
			return;

		DecayPunchAngle();
		float xMovement = Mouse.current.delta.x.ReadValue() *
						  horizontalSensitivity *
						  sensitivityMultiplier;

		float yMovement = -Mouse.current.delta.y.ReadValue() *
						  verticalSensitivity *
						  sensitivityMultiplier;

		if (lookAffectsTransform)
		{
			viewCamera.targetDisplay = 0;
			// Normal looking.
			bodyYaw += xMovement;

			realRotation.x = Mathf.Clamp(
				realRotation.x + yMovement,
				minYRotation,
				maxYRotation
			);

			realRotation.y = bodyYaw;

			bodyTransform.eulerAngles = new Vector3(
				bodyTransform.eulerAngles.x,
				bodyYaw,
				bodyTransform.eulerAngles.z
			);
		}

		// No smoothing.
		realRotation.z = 0f;

		Vector3 lookRotation = realRotation;

		lookRotation.x += punchAngle.x;
		lookRotation.y += punchAngle.y;

		// Viewmodel camera does NOT get free-look.
		SyncViewCamera();
	}

	private void SyncViewCamera()
	{
		if (viewCamera == null)
			return;

		// Use the body's normal view rotation, NOT free-look rotation.
		Vector3 viewRotation = realRotation;

		viewRotation.y = bodyYaw;

		// Keep vertical look.
		viewRotation.x = realRotation.x;

		// Apply aimpunch to the viewmodel too.
		viewRotation.x += punchAngle.x;
		viewRotation.y += punchAngle.y;

		viewRotation.z = 0f;

		viewCamera.transform.eulerAngles = viewRotation;
	}

	public void ViewPunch(Vector2 punchAmount)
	{
		punchAngle = Vector2.zero;
		punchAngleVel -= punchAmount * 20f;
	}

	private void DecayPunchAngle()
	{
		if (punchAngle.sqrMagnitude > 0.001f ||
			punchAngleVel.sqrMagnitude > 0.001f)
		{
			punchAngle += punchAngleVel * Time.deltaTime;

			float damping = Mathf.Max(
				0f,
				1f - punchDamping * Time.deltaTime
			);

			punchAngleVel *= damping;

			punchAngleVel -=
				punchAngle *
				punchSpringConstant *
				Time.deltaTime;
		}
		else
		{
			punchAngle = Vector2.zero;
			punchAngleVel = Vector2.zero;
		}
	}
}
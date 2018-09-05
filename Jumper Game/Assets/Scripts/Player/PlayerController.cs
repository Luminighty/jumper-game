using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

	[Header("Components")]
	public Collider defaultCollider;

	[Tooltip("Camera that follows the player")]
	public Transform cameraTransform;
	[Header("Move Settings")]
	public float speed = 500f;
	public float rotationSpeed = 120f;

	[Header("Jump Settings")]
	
	public LayerMask groundLayers;
	public int jumpCount = 1;
	private int currentJumpCount = 0;
	public float[] jumpSize;
	public bool isGrounded = false;
	[Tooltip("Color: Yellow")]
	public Vector3 groundBoxOffset;
	public Vector3 boxSize;

	[Header("Crouch")]
	public float speedModifier;
	public Collider crouchCollider;

	[Header("Dive")]
	public bool isDiving = false;
	public Vector2 DiveSpeed;
	public Vector2 DiveGravity;
	public float DiveTime = 0.5f;
	private MoveForce currentDive;

	[Header("Debug")]
	public bool isInputLog;


	private Rigidbody rigid;
	private List<MoveForce> forces = new List<MoveForce>();


	Vector2 MoveInput {
		get { return GetAxisInputs("Horizontal", "Vertical"); }
	}
	
	Vector2 GetAxisInputs(string horizontal, string vertical, bool isRaw = false) {
		return (isRaw) ? new Vector2(Input.GetAxisRaw(horizontal), Input.GetAxisRaw(vertical)) : new Vector2(Input.GetAxis(horizontal), Input.GetAxis(vertical));
	}

	// Use this for initialization
	void Awake () {
		rigid = GetComponent<Rigidbody>();
	}
	
	// Update is called once per frame
	void Update () {
		Move();
		Jump();
		Dive();

		ApplyForces(Time.deltaTime);
		// Used for debugging purposes (Shows the current input in console)
		LogInput();


	}

	void ApplyForces(float time) {
		
		Vector3 velocity = rigid.velocity;

		for(int i = 0; i < forces.Count; i++) {
			velocity = forces[i].ApplyToVelocity(velocity, time);
			if(!forces[i].Tick(time))
				forces.RemoveAt(i);
		}

		rigid.velocity = velocity;
	}

	void Dive() {
		if(Input.GetAxisRaw("Dive") == 1) {
			if(isDiving)
				return;
		} else {
			if(isGrounded)
				isDiving = false;
			return;
		}
		Debug.Log("Dive");
		isDiving = true;

		Vector3 forward = transform.forward;

		Vector3 force = forward.normalized;
		Vector3 gravity = forward.normalized;
		force.x *= DiveSpeed.x;
		force.z *= DiveSpeed.x;
		force.y = DiveSpeed.y;
		
		gravity.x *= DiveGravity.x;
		gravity.z *= DiveGravity.x;
		gravity.y = DiveGravity.y;

		currentDive = new MoveForce(force, gravity, DiveTime, 0.5f, 0);

		forces.Add(currentDive);

	}

	void LogInput() {
		if(!isInputLog)
			return;
		if(Input.inputString != "")
		Debug.Log(Input.inputString);

		for(int i = 0; i < 20; i++)
			if(Input.GetKeyDown("joystick 1 button " + i))
				Debug.Log("joystick 1 button " + i);
	}

	void Jump() {
		Vector3 veloc = rigid.velocity;

		// Platformer jump
		if(rigid.velocity.y > 0 && !Input.GetButton("Jump")) {
			veloc.y *= 0.8f;
		}

		// Jump
		if(Input.GetButtonDown("Jump") && currentJumpCount < jumpCount) {
			veloc.y = jumpSize[currentJumpCount % jumpSize.Length];
			currentJumpCount++;
		}
		
		rigid.velocity = veloc;

		if(Physics.OverlapBox(transform.position + groundBoxOffset, boxSize, Quaternion.identity, groundLayers.value).Length > 0) {
			if(veloc.y <= 0) {
				currentJumpCount = 0;
				isGrounded = true;
			}
		} else {
			isGrounded = false;
			if(currentJumpCount == 0)
				currentJumpCount++;
		}

	}

	/// <summary>
	/// Callback to draw gizmos only if the object is selected.
	/// </summary>
	void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireCube(transform.position + groundBoxOffset, boxSize);
	}

	void Move() {
		
		Vector3 forward = transform.position - cameraTransform.position;
		forward.y = 0.0f;
		forward = Vector3.Normalize(forward);
		Vector3 right = new Vector3(forward.z, 0.0f, -forward.x);

		Vector2 input = MoveInput;

		//Max input lenght is 1
		if(input.magnitude > 1.0f) {
			input.Normalize();
		}

		Vector3 velocity = ((forward * input.y) + (right * input.x)) * Time.deltaTime * speed;

		if(velocity != Vector3.zero)
			transform.rotation = Quaternion.LookRotation(velocity, Vector3.up);

		velocity.y = rigid.velocity.y;
		rigid.velocity = velocity;

		if(input != Vector2.zero)
			Debug.Log(input);

		Debug.DrawRay(transform.position, velocity, Color.red);
	}

	[System.Serializable]
	class MoveForce {

		float multiplierX = 1.0f;
		float multiplierY = 1.0f;
		Vector3 force;
		Vector3 gravity;
		float liveTime = 0.0f;

		public MoveForce(Vector3 force, Vector3 gravity, float lifeTime = 1.0f, float multiplierX = 1.0f, float multiplierY = 1.0f) {
			this.gravity = gravity;
			this.force = force;
			this.liveTime = lifeTime;
			this.multiplierX = multiplierX;
			this.multiplierY = multiplierY;
		}

		public MoveForce(Vector3 force, float lifeTime = 1.0f, float multiplierX = 1.0f, float multiplierY = 1.0f) {
			this.gravity = Vector3.zero;
			this.force = force;
			this.liveTime = lifeTime;
			this.multiplierX = multiplierX;
			this.multiplierY = multiplierY;
		}

		/// <summary>
		///	Returns false if the lifetime expired
		/// </summary>
		public bool Tick(float tickTime) {
			if(liveTime <= 0.0f) {
				multiplierX = 1.0f;
				multiplierY = 1.0f;
				force = Vector3.zero;
				gravity = Vector3.zero;
				return false;
			}

			liveTime -= tickTime;
			force += gravity * tickTime;

			return true;
		}


		public Vector3 ApplyToVelocity(Vector3 velocity, float deltaTime) {
			Vector3 newVelocity = velocity;

			newVelocity.x *= multiplierX;
			newVelocity.y *= multiplierY;
			
			newVelocity += force * deltaTime;

			return newVelocity;
		}

	}
}

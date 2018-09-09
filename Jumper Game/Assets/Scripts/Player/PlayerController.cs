using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

	#region Variables

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
	public Vector3 groundBoxSize;

	[Header("Crouch")]
	public float speedModifier;
	public Collider crouchCollider;

	[Header("Dive")]
	public bool isDiving = false;
	public Vector2 DiveSpeed;
	public Vector2 DiveGravity;
	public float DiveTime = 0.5f;
	private MoveForce currentDive;


	[Header("Wall Climb")]
	public ClimbState wallClimb;
	public enum ClimbState { None, Climbing, Sliding }
	public float ClimbSpeed;
	public float ClimbDistance;
	public float LastWallAngle;
	public LayerMask wallLayers;
	public float wallDistance = 0.2f;
	public Vector3[] wallOffsets = new Vector3[0];
	private float climbStartY = 0.0f;
	public float wallSlideSpeed;


	[Header("Debug")]
	public bool isInputLog;
	public bool isAxisLog;


	private Rigidbody rigid;
	private List<MoveForce> forces = new List<MoveForce>();


	Vector2 MoveInput {
		get { return GetAxisInputs("Horizontal", "Vertical"); }
	}

	Vector3 forward {
		get { 
			Vector3 vec = transform.position - cameraTransform.position;
			vec.y = 0.0f;
			vec = Vector3.Normalize(vec);
			return vec;
		}
	}
	Vector3 right {
		get { 
			return new Vector3(forward.z, 0.0f, -forward.x);
		}
	}
	
	#endregion

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
		WallClimb();

		ApplyForces(Time.deltaTime);
		// Used for debugging purposes (Shows the current input in console)
		LogInput();
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

	void ApplyForces(float time) {
		
		Vector3 velocity = rigid.velocity;

		for(int i = 0; i < forces.Count; i++) {
			velocity = forces[i].ApplyToVelocity(velocity, time);
			if(!forces[i].Tick(time))
				forces.RemoveAt(i);
		}

		rigid.velocity = velocity;
	}

	void WallClimb() {
		Vector3 way = rigid.velocity;
		
		switch(wallClimb) {
			case ClimbState.None:
				if(way.y >= 0.0f || isGrounded)
					return;

				way.y = 0.0f;
				way.Normalize();
				
				bool foundWall = false;

				for(int i = 0; i < wallOffsets.Length && !foundWall; i++) {
					Vector3 pos = transform.forward * wallOffsets[i].z;
					pos.y = wallOffsets[i].y;
					pos += transform.position;
					foundWall = foundWall || Physics.Raycast(pos, transform.forward, wallDistance, wallLayers.value);
					Debug.DrawRay(pos, transform.forward * wallDistance, Color.green, 1.0f);
				}

				if(!foundWall)
					return;

				wallClimb = ClimbState.Climbing;
				climbStartY = transform.position.y;
				LastWallAngle = transform.eulerAngles.y;

			break;
			case ClimbState.Climbing:

				rigid.velocity = Vector3.up * ClimbSpeed * Time.deltaTime;

				if(Mathf.Abs(transform.position.y - climbStartY) > ClimbDistance)
					wallClimb = ClimbState.Sliding;

			break;
			case ClimbState.Sliding:
				if(isGrounded)
					wallClimb = ClimbState.None;

				rigid.velocity = Vector3.down * wallSlideSpeed * Time.deltaTime;

			break;
			default:
			break;
		}


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

		Collider[] cols = Physics.OverlapBox(transform.position + groundBoxOffset, groundBoxSize / 2.0f, Quaternion.identity, groundLayers.value);
		
		if(cols.Length > 0) {
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

	void Move() {

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

		if(input != Vector2.zero && isAxisLog)
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


	/// <summary>
	/// Callback to draw gizmos only if the object is selected.
	/// </summary>
	void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireCube(transform.position + groundBoxOffset, groundBoxSize);
		Gizmos.color = Color.blue;

		for(int i = 0; i < wallOffsets.Length; i++)
			Gizmos.DrawRay(transform.position + wallOffsets[i], transform.forward * wallDistance);
	}
}

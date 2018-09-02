using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

	[Tooltip("Camera that follows the player")]
	public Transform cameraTransform;
	[Header("Move Settings")]
	public float speed = 500f;
	public float rotationSpeed = 120f;

	[Header("Jump Settings")]
	
	public LayerMask groundLayers;
	public int jumpCount = 1;
	private int currentJumpCount = 0;
	public float jumpSize = 10f;
	[System.NonSerialized]
	public bool isGrounded = false;
	[Tooltip("Color: Yellow")]
	public Vector3 groundBoxOffset;
	public Vector3 boxSize;

	private Rigidbody rigid;

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
	}

	void Jump() {
		Vector3 veloc = rigid.velocity;

		// Platformer jump
		if(rigid.velocity.y > 0 && !Input.GetButton("Jump")) {
			veloc.y *= 0.8f;
		}

		// Jump
		if(Input.GetButtonDown("Jump") && currentJumpCount < jumpCount) {
			currentJumpCount++;
			veloc.y = jumpSize;
		}
		
		rigid.velocity = veloc;

		if(Physics.OverlapBox(transform.position + groundBoxOffset, boxSize, Quaternion.identity, groundLayers.value).Length > 0) {
			if(veloc.y <= 0) {
				currentJumpCount = 0;
				isGrounded = true;
			}
		} else {
			if(veloc.y <= 0 && isGrounded) {
				currentJumpCount++;
				isGrounded = false;	
			}
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
}

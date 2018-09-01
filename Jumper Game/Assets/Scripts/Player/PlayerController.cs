using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

	public Transform cameraTransform;
	public float speed = 500f;
	public float rotationSpeed = 120f;

	public float jumpSize = 10f;

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

		if(rigid.velocity.y > 0 && !Input.GetButton("Jump")) {
			veloc.y *= 0.8f;
		}

		if(Input.GetButtonDown("Jump"))
			veloc.y = jumpSize;
		
		rigid.velocity = veloc;
	}

	void Move() {
		
		Vector3 forward = Vector3.Normalize(transform.position - cameraTransform.position);
		forward.y = 0.0f;
		Vector3 right = new Vector3(forward.z, 0.0f, -forward.x);

		Vector3 velocity = Vector3.Normalize((forward * MoveInput.y) + (right * MoveInput.x)) * Time.deltaTime * speed;

		if(velocity != Vector3.zero)
			transform.rotation = Quaternion.LookRotation(velocity, Vector3.up);

		velocity.y = rigid.velocity.y;
		rigid.velocity = velocity;

		Debug.DrawRay(transform.position, velocity, Color.red);
	}
}

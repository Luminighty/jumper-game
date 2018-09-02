using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

	Vector3 positionOffset;
	public Transform lookAt;

	public Vector2 rotationSpeed;
	public bool inverseX = true;
	public bool inverseY = false;

	public bool mouseInput = false;

	[Space]
	public float MaxRotation = 45f;
	public float MinRotation = 340f;

	
	Vector2 GetAxisInputs(string horizontal, string vertical, bool isRaw = false) {
		return (isRaw) ? new Vector2(Input.GetAxisRaw(horizontal), Input.GetAxisRaw(vertical)) : new Vector2(Input.GetAxis(horizontal), Input.GetAxis(vertical));
	}

	Vector2 CameraInput {
		get {
			Vector2 input = GetAxisInputs("Horizontal2", "Vertical2");
			if(input == Vector2.zero && mouseInput)
				input = GetAxisInputs("Mouse X", "Mouse Y");
			return input;
		}
	}
	/// <summary>
	/// Awake is called when the script instance is being loaded.
	/// </summary>
	void Awake()
	{
		positionOffset = lookAt.transform.position - transform.position;
	}

	void Update () {
		transform.position = lookAt.position - positionOffset;
		Look();
	}
	/// <summary>
	/// Called every frame, Rotates the camera around the player when input is given;
	/// </summary>
	void Look() {

		if(CameraInput == Vector2.zero)
			return;

		Vector3 rotation = transform.eulerAngles;

		rotation.y += CameraInput.x * rotationSpeed.x * Time.deltaTime * (inverseX ? -1 : 1);
		rotation.x += CameraInput.y * rotationSpeed.y * Time.deltaTime * (inverseY ? -1 : 1);

		if(rotation.x < 180)
			rotation.x = Mathf.Min(MaxRotation, rotation.x);

		if(rotation.x > 180)
			rotation.x = Mathf.Max(MinRotation, rotation.x);

		transform.eulerAngles = rotation;
	}
}

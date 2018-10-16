using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AerialCamera : MonoBehaviour {

	const KeyCode FORWARD = KeyCode.W;
	const KeyCode BACK = KeyCode.S;
	const KeyCode RIGHT = KeyCode.D;
	const KeyCode LEFT = KeyCode.A;
	const KeyCode DOWN = KeyCode.DownArrow;
	const KeyCode UP = KeyCode.UpArrow;

	public float flySpeed = 0.5f;


	public Camera camera;

	public float minX, maxX, minY, maxY, minZ, maxZ;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		UpdateCameraPosition();
	}

	void UpdateCameraPosition() {

		if (Input.GetKey(UP)) {
			transform.Translate(Vector3.up * flySpeed * 0.5f);
			CheckCameraBounds();
		} else if (Input.GetKey(DOWN)) {
			transform.Translate(-Vector3.up * flySpeed * 0.5f);
			CheckCameraBounds();
		}

		// move in direction of camera.forward
		if (Input.GetKey(FORWARD) || Input.GetKey(BACK)) {
			if (Input.GetKey(FORWARD)) {
				transform.position += camera.transform.forward * flySpeed * 0.5f;
			} else if (Input.GetKey(BACK)) {
				transform.position -= camera.transform.forward * flySpeed * 0.5f;
			}
			CheckCameraBounds();
		}

		if (Input.GetKey(RIGHT)) {
			transform.Translate(Vector3.right * flySpeed * 0.5f);
			CheckCameraBounds();
		} else if (Input.GetKey(LEFT)) {
			transform.Translate(-Vector3.right * flySpeed * 0.5f);
			CheckCameraBounds();
		}
	}

	void CheckCameraBounds() {
		Vector3 pos = transform.position;
		if (pos.x < minX) {
			pos.x = minX;
		} else if (pos.x > maxX) {
			pos.x = maxX;
		}
		if (pos.y < minY) {
			pos.y = minY;
		} else if (pos.y > maxY) {
			pos.y = maxY;
		}
		if (pos.z < minZ) {
			pos.z = minZ;
		} else if (pos.z > maxZ) {
			pos.z = maxZ;
		}
		transform.position = pos;
	}
}

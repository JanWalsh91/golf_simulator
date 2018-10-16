using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Ball : MonoBehaviour {

	const KeyCode RESET = KeyCode.R;
	const KeyCode SHOOT = KeyCode.Space;
	const KeyCode RIGHT = KeyCode.D;
	const KeyCode LEFT = KeyCode.A;
	const KeyCode NEXT = KeyCode.Return;

	public GameObject arrow;
	public GameObject ballCam;
	public float camDist;
	public float arrowDist;

	public Slider slider;

	GameManager.Hole currentHole;

	public bool isCharging = false;
	float startChargingTime;
	float power;
	bool isMoving = false;
	bool inHole = false;
	public bool inWater = false;
	bool onGround = true;

	Rigidbody rigidbody;

	public Vector3 lastOutOfWaterPos;

	// Use this for initialization
	void Start() {
		Debug.Log("Ball start");
		rigidbody = GetComponent<Rigidbody>();
		currentHole = GameManager.instance.GetCurrentHole();
	}

	// Update is called once per frame
	void Update() {
		if (Input.GetKeyDown(RESET)) {
			Reset();
		}
		if (Input.GetKeyDown(SHOOT) && ballCam.activeSelf) {
			if (!isMoving) {
				if (!isCharging) {
					isCharging = true;
					startChargingTime = Time.time;
				} else {
					isCharging = false;
					Shoot();
					slider.value = 0;
				}
			}
		}
		if (isCharging) {
			power = Mathf.PingPong(startChargingTime - Time.time, 1f);
			slider.value = power;
		}
		if (Input.GetKey(RIGHT)) {
			RotateCameraAndArrow(true);
		} else if (Input.GetKey(LEFT)) {
			RotateCameraAndArrow(false);
		}
		if (rigidbody.velocity.magnitude > 0.1f) {
			FaceArrowAndCameraToHole();
			if (arrow.activeSelf) {
				arrow.SetActive(false);
				slider.gameObject.SetActive(false);
			}
			isMoving = true;
		} else if (!arrow.activeSelf){
			isMoving = false;
			arrow.SetActive(true);
			slider.gameObject.SetActive(true);
		}
		if (inHole && Input.GetKeyDown(NEXT)) {
			NextHole();
		}
		if (transform.position.y < -50) {
			currentHole.score++;
			Reset();
		}
	}

	void OnTriggerEnter(Collider other) {
		if (other.gameObject.CompareTag("EndPoints")) {
			if (other == currentHole.end && !inHole) {
				inHole = true;
				GameManager.instance.HoleSuccess();
				currentHole = GameManager.instance.GetCurrentHole();
			} else {
				Debug.Log("Wrong hole!!");
			}
		}
		if (other.gameObject.CompareTag("Green")) {
			//Debug.Log("Trigger in green");
			GameManager.instance.inGreenZone = true;
			GameManager.instance.NextClub();
		}
		if (other.gameObject.CompareTag("Bunker")) {
			GameManager.instance.inBunkerZone = true;
			GameManager.instance.NextClub();
			SetDrag();
		}
		if (other.gameObject.CompareTag("Water")) {
			GameManager.instance.InWater();
			inWater = true;
		}
	}

	void OnTriggerExit(Collider other) {
		if (other.gameObject.CompareTag("Green")) {
			GameManager.instance.inGreenZone = false;
			GameManager.instance.NextClub();
			SetDrag();
		}
		if (other.gameObject.CompareTag("Bunker")) {
			GameManager.instance.inBunkerZone = false;
			SetDrag();
		}
	}

	void OnCollisionEnter(Collision collision) {
		if (collision.gameObject.CompareTag("Terrain")) {
			onGround = true;
			SetDrag();
		}	
	}

	void OnCollisionExit(Collision collision) {
		//Debug.Log("OnCollisionExit");
		if (collision.gameObject.CompareTag("Terrain")) {
			onGround = false;
			SetDrag();
		}
	}

	void NextHole() {
		GameManager.instance.currentHole++;
		GameManager.instance.inGreenZone = false;
		GameManager.instance.inBunkerZone = false;
		currentHole = GameManager.instance.GetCurrentHole();
		Reset();
	}

	public void Reset() {
		GameManager.instance.inGreenZone = false;
		GameManager.instance.inBunkerZone = false;
		inHole = false;
		inWater = false;
		transform.position = currentHole.start.gameObject.transform.position;
		lastOutOfWaterPos = transform.position;
		rigidbody.velocity = Vector3.zero;
		FaceArrowAndCameraToHole();
	}

	void Shoot() {
		float debuf = 1f;
		if (GameManager.instance.inBunkerZone == true) {
			debuf = 0.8f;
		}
		lastOutOfWaterPos = transform.position;
		Vector3 force = (transform.position - ballCam.transform.position).normalized;
		GameManager.Club club = GameManager.instance.GetClub();
		force.y = club.height;
		rigidbody.AddForce(force.normalized * power * club.power * debuf);
		GameManager.instance.IncreaseScore();
	}

	public void FaceArrowAndCameraToHole() {
		Vector3 dirToHole = (currentHole.end.transform.position - transform.position).normalized;
		dirToHole.Normalize();
		Vector3 camPos = transform.position - dirToHole * camDist;
		if (camPos.y < transform.position.y) {
			camPos.y = transform.position.y + 3f;
		}
		// set camera position and direction
		ballCam.transform.position = camPos;
		ballCam.transform.LookAt(currentHole.end.transform.position);

		Vector3 arrowPos = transform.position + dirToHole * arrowDist;
		arrowPos.y = transform.position.y + 5f;
		arrow.transform.position = arrowPos;
		arrow.transform.LookAt(currentHole.end.transform.position);
		arrow.transform.Rotate(new Vector3(150, 0, 0));
	}

	void RotateCameraAndArrow(bool right) {
		float dir = right ? 1 : -1;
		ballCam.transform.RotateAround(transform.position, Vector3.up, dir * Time.deltaTime * 10);
		arrow.transform.RotateAround(transform.position, Vector3.up, dir * Time.deltaTime * 10);
	}

	void SetDrag() {
		//Debug.Log("SetDrag");
		if (!onGround) {
			//Debug.Log("\toff ground");
			rigidbody.drag = 0;
			rigidbody.angularDrag = 0;
		} else {
			if (GameManager.instance.inBunkerZone) {
				//Debug.Log("\ton bunker");
				rigidbody.drag = 5;
				rigidbody.angularDrag = 20;
			} else {
				//Debug.Log("\tnormal ground");
				rigidbody.drag = 1;
				rigidbody.angularDrag = 20;
			}
		}
	}
}

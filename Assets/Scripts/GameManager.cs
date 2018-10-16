using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {

	const KeyCode SWITCH_CAM = KeyCode.E;
	const KeyCode BACK_TO_BALLCAM = KeyCode.Space;
	const KeyCode CLOSE_MENU = KeyCode.Return;
	const KeyCode TAB = KeyCode.Tab;
	const KeyCode NEXT_CLUB = KeyCode.KeypadPlus;
	const KeyCode NEXT_CLUB2 = KeyCode.Equals;
	const KeyCode FAST = KeyCode.Period;

	public static GameManager instance = null;
	public GameObject aerialCam;
	public GameObject aerialCamRig;
	public GameObject ballCam;

	public GameObject PanelHoleSuccess;
	public Text SuccessCurrentHole;
	public Text SuccessPar;
	public Text SuccessScore;

	public Text TextCurrentHole;
	public Text TextPar;
	public Text TextScore;
	public Text TextClub;

	public GameObject PanelRecap;
	public GameObject PanelHoleRecap;

	public GameObject PanelInWater;

	public GameObject ball;

	[SerializeField]
	public List<Hole> holes = new List<Hole>();
	public int currentHole = 0;

	[SerializeField]
	public List<Club> clubs = new List<Club>();
	public int currentClub = 0;

	public bool inGreenZone = false;
	public bool inBunkerZone = false;

	void Awake() {
		if (instance == null) {
			instance = this;
		} else if (instance != this) {
			Destroy(gameObject);
		}
		DontDestroyOnLoad(gameObject);
	}

	// Use this for initialization
	void Start() {
		Debug.Log("Game Manager Start holes count: " + holes.Count);
		PanelHoleSuccess.SetActive(false);
		PanelRecap.SetActive(false);
		currentHole = 0;
		currentClub = 0;
		SetTextPar();
		SetTextScore();
		SetTextCurrentHole();
		SetTextClub();
	}

	// Update is called once per frame
	void Update() {
		if (Input.GetKeyDown(SWITCH_CAM)) {
			SwitchCam();
		} if (Input.GetKeyDown(BACK_TO_BALLCAM) && aerialCamRig.activeSelf) {
			SwitchCam();
			ball.GetComponent<Ball>().isCharging = false;
		}
		if (PanelInWater.activeSelf && Input.GetKeyDown(CLOSE_MENU)) {
			PanelInWater.SetActive(false);
			ball.transform.position = ball.GetComponent<Ball>().lastOutOfWaterPos;
			ball.GetComponent<Ball>().FaceArrowAndCameraToHole();
			ball.GetComponent<Ball>().inWater = false;
			holes[currentHole].score++;
			SetTextScore();
		}
		if (PanelHoleSuccess.activeSelf && Input.GetKeyDown(CLOSE_MENU)) {
			PanelHoleSuccess.SetActive(false);
			currentClub = 0;
		}
		if (Input.GetKeyDown(TAB)) {
			UpdatePanelRecap(false);
			PanelRecap.SetActive(true);
		}
		if (Input.GetKeyUp(TAB)) {
			PanelRecap.SetActive(false);
		}
		if (Input.GetKeyDown(NEXT_CLUB) || Input.GetKeyDown(NEXT_CLUB2)) {
			NextClub();
		}
		if (Input.GetKey(FAST)) {
			Time.timeScale = 10f;
		}
		if (Input.GetKeyUp(FAST)) {
			Time.timeScale = 1f;
		}
	}

	public void GoToScene(string name) {
		SceneManager.LoadScene(name);
	}

	void SwitchCam() {
		if (aerialCamRig.activeSelf) {
			aerialCamRig.SetActive(false);
			ballCam.SetActive(true);
		} else {
			aerialCamRig.SetActive(true);
			ballCam.SetActive(false);
		}
	}

	[System.Serializable]
	public class Hole {
		public Collider start;
		public Collider end;
		public int par;
		public int score;
	}

	public Hole GetCurrentHole() {
		SetTextPar();
		SetTextScore();
		SetTextCurrentHole();
		if (currentHole < holes.Count) {
			SetTextClub();
			return holes[currentHole];
		} else {
			UpdatePanelRecap(true);
			PanelRecap.SetActive(true);
			currentHole = 0;
			SetTextClub();
			Time.timeScale = 0f;
			return GetCurrentHole();
		}
	}

	public void HoleSuccess() {
		SuccessCurrentHole.text = "Hole number " + (currentHole + 1).ToString();
		SuccessPar.text = "Par: " + holes[currentHole].par;
		SuccessScore.text = "Score: " + GetScoreName(holes[currentHole].par, holes[currentHole].score);
		PanelHoleSuccess.SetActive(true);
	}

	void SetTextCurrentHole() {
		TextCurrentHole.text = "Current hole: " + (currentHole + 1).ToString();
	}

	void SetTextPar() {
		if (currentHole < holes.Count)
			TextPar.text = "Par: " + holes[currentHole].par.ToString();
	}

	void SetTextScore() {
		if (currentHole < holes.Count)
			TextScore.text = "Score: " + holes[currentHole].score.ToString();
	}

	void SetTextClub() {
		if (currentHole < holes.Count)
			TextClub.text = "Club: " + clubs[currentClub].name.ToString();
	}

	public void IncreaseScore() {
		holes[currentHole].score++;
		SetTextScore();
	}

	static string GetScoreName(int par, int score) {
		if (score == 1) return "Ace";
		if (score - par <= -3) return "Albatross";
		if (score - par == -2) return "Eagle";
		if (score - par == -1) return "Birdie";
		if (score - par == 0) return "Par";
		if (score - par == 1) return "Bogey";
		if (score - par == 2) return "Double Bogey";
		if (score - par == 3) return "Triple Bogey";
		return "+" + (score - par).ToString();
	}

	public void UpdatePanelRecap(bool isFinal) {
		int total = 0;
		foreach (Transform subpanel in PanelRecap.transform) {
			Destroy(subpanel.gameObject);
		}
		for (int i = 0; i < holes.Count; i++) {
			GameObject panelHoleRecap = Instantiate(PanelHoleRecap);
			panelHoleRecap.transform.SetParent(PanelRecap.transform);
			Text[] texts = panelHoleRecap.GetComponentsInChildren<Text>();
			texts[0].text = "Hole: " + (i + 1).ToString();
			texts[1].text = "Par: " + holes[i].par.ToString();
			texts[2].text = "Score: " + holes[i].score.ToString();
			total += holes[i].score;
		}
		if (isFinal) {
			GameObject panelHoleRecap = Instantiate(PanelHoleRecap);
			panelHoleRecap.transform.SetParent(PanelRecap.transform);
			Text[] texts = panelHoleRecap.GetComponentsInChildren<Text>();
			texts[0].text = "Total: " + (total).ToString();
			Destroy(texts[1].gameObject);
			Destroy(texts[2].gameObject);
		}
	}

	[System.Serializable]
	public class Club {
		public string name;
		public float power;
		public float height;
	}

	public Club GetClub() {
		return clubs[currentClub];
	}

	public void NextClub() {
		currentClub++;
		if (inBunkerZone) {
			currentClub = 2;
		}
		if (inGreenZone) {
			currentClub = 3;
		}
		if (currentClub >= clubs.Count) {
			currentClub = 0;
		}
		if (clubs[currentClub].name.Equals("Putter") && !inGreenZone) {
			currentClub = 0;
		}
		SetTextClub();
	}

	public void InWater() {
		PanelInWater.SetActive(true);
	}
}

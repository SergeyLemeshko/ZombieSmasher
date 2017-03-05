using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class ZombieController : MonoBehaviour {

	[SerializeField]
	protected GameObject ZombiePrefab;

	[SerializeField]
	protected GameObject Zombie2TypePrefab;

	[SerializeField]
	protected GameObject Zombie3TypePrefab;

	[SerializeField]
	protected GameObject StartDialogPrefab;

	[SerializeField]
	protected GameObject ResultDialogPrefab;

	[SerializeField]
	protected GameObject WinDialogPrefab;

	[SerializeField]
	protected GameObject ZombieIconPrefab;

	[SerializeField]
	protected GameObject ExplosionPrefab;

	[SerializeField]
	protected HorizontalLayoutGroup ZombiePanel;


	[SerializeField]
	protected Camera MainCamera;

	[SerializeField]
	protected Canvas MainCanvas;

	[SerializeField]
	protected Button bombButton;

	[SerializeField]
	protected Text BombCountText;

	[SerializeField]
	protected Text ScoreText;

	[SerializeField]
	protected AudioClip ExplosionAudioClip;

	private const int MAX_SURVIVED_ZOMBIE  = 3;
	private const int BOMB_KILL_RADIUS = 6;
	private const float TIME_ROUND = 60f;
	private const int ZOMBIE_COST = 10;

	private StartDialog StartDialog;
	private ResultDialog ResultDialog;
	private WinDialog WinDialog;

	private AudioSource AudioSource;

	private Vector3 center;
	private bool IsMassacreOn;

	private int SurvivedZombieCount;
	private int _BombCount;
	private int _Score;

	private float TimeLeft;

	private void Start () {
		center = MainCamera.ScreenToWorldPoint(new Vector3(Screen.width/2,Screen.height/2,0));
		bombButton.onClick.AddListener (() => {
			if (BombCount > 0) {
				BombCount--;
				StartCoroutine(DropBomb());
			}
		});

		AudioSource = GetComponent<AudioSource> ();

		OpenStartDialog ();
	}

	private void Update() {
		if (IsMassacreOn) {
			TimeLeft -= Time.deltaTime;
			if (TimeLeft <= 0) {
				IsMassacreOn = false;
				OpenWinDialog();
			}
		}
	}

	private IEnumerator DropBomb() {
		GameObject explosionGO = (GameObject)GameObject.Instantiate(ExplosionPrefab);
		explosionGO.transform.position = center;

		AudioSource.PlayOneShot(ExplosionAudioClip);

		CheckZombiesForKillByBomb ();

		yield return new WaitForSeconds (2);

		Object.Destroy (explosionGO);
	}

	private void CheckZombiesForKillByBomb() {

		GameObject[] zombiesGO = GameObject.FindGameObjectsWithTag("Zombie");
		foreach(GameObject zombieGo in zombiesGO) {
			float distance = Vector3.Distance(zombieGo.transform.position, new Vector3(center.x, center.y, zombieGo.transform.position.z));
			if (distance < BOMB_KILL_RADIUS) {
				zombieGo.GetComponent<Zombie>().State = Zombie.ZombieState.Dead;
			}
		}

	}

	public int Score {
		get {
			return _Score;
		}
		set {
			_Score = value;
			ScoreText.text = "SCORE " + value;
		}
	}
	
	public int BombCount {
		get {
			return _BombCount;
		}
		set {
			_BombCount = value;
			BombCountText.text = value.ToString();
		}
	}

	private void ClearZombies() {
		GameObject[] zombiesGO = GameObject.FindGameObjectsWithTag("Zombie");
		foreach(GameObject zombieGo in zombiesGO) {
			Object.Destroy(zombieGo);
		}

	}

	private void OpenStartDialog() {
		StartDialog = ((GameObject)GameObject.Instantiate (StartDialogPrefab)).GetComponent<StartDialog>();
		StartDialog.OnPlayClick += HandleOnPlayClick;
	}

	private void HandleOnPlayClick (){
		StartDialog.Close();
		StartMassacre ();
	}

	private void OpenResultDialog() {
		ResultDialog = ((GameObject)GameObject.Instantiate (ResultDialogPrefab)).GetComponent<ResultDialog>();
		ResultDialog.OnPlayClick += HandleOnResultDialogPlayClick;
	}

	private void HandleOnResultDialogPlayClick() {
		ResultDialog.Close();
		StartMassacre ();
	}

	private void OpenWinDialog() {
		WinDialog = ((GameObject)GameObject.Instantiate (WinDialogPrefab)).GetComponent<WinDialog>();
		WinDialog.OnPlayClick += HandleOnWinDialogPlayClick;
	}

	private void HandleOnWinDialogPlayClick() {
		WinDialog.Close();
		StartMassacre ();
	}


	private void StartMassacre() {
		IsMassacreOn = true;
		TimeLeft = TIME_ROUND;
		Score = 0;
		FillZombiePanel ();
		StartCoroutine (StartZombieCreateHellMachine());
	}

	private void FillZombiePanel() {
		int cnt = 3 - ZombiePanel.transform.childCount;
		for (var i = 0; i < cnt ; i++) {
			((GameObject)GameObject.Instantiate(ZombieIconPrefab)).transform.SetParent(ZombiePanel.transform);
		}
	}

	private void DescreaseZombiesFromZombiePanel(){
		Object.Destroy(ZombiePanel.transform.GetChild(0).gameObject);
	}

	private IEnumerator StartZombieCreateHellMachine() {
		ClearZombies ();
		SurvivedZombieCount = 0;
		BombCount = 3;
		while(IsMassacreOn) {
			CreateZombie();
			yield return new WaitForSeconds(1f);
		}
	}

	private void CreateZombie() {
		Zombie.ZombieType zombieType = GenerateZombieType ();
		GameObject zombieGO;
		if (zombieType == Zombie.ZombieType.Third) {
			zombieGO = ((GameObject)(GameObject.Instantiate (Zombie3TypePrefab)));
		} else if (zombieType == Zombie.ZombieType.First) {
			zombieGO = ((GameObject)(GameObject.Instantiate (ZombiePrefab)));
		}
		else {
			zombieGO = ((GameObject)(GameObject.Instantiate (Zombie2TypePrefab)));
		}
		Zombie zombie = zombieGO.GetComponent<Zombie>();
		Vector3 zeroScreenPoint = MainCamera.ScreenToWorldPoint (new Vector3 (0, 0, 0));
		float posX = Random.Range (zeroScreenPoint.x, MainCamera.ScreenToWorldPoint(new Vector3(Screen.width,0,0)).x);
		zombieGO.transform.position = new Vector3(posX, 0, -5);
		List<Vector3> wayPoints = new List<Vector3>();

		zombie.Type = zombieType;
		if (zombieType == Zombie.ZombieType.First) {
			wayPoints.Add(new Vector3(posX, zeroScreenPoint.y, -5));
		} else {
			wayPoints.Add(new Vector3(Random.Range (zeroScreenPoint.x, MainCamera.ScreenToWorldPoint(new Vector3(Screen.width,0,0)).x), zeroScreenPoint.y, -5));
		}
		zombie.StopEvent += HandleStopEvent;
		zombie.ChangeZombieStateEvent += HandleChangeZombieStateEvent;
		zombie.Move (wayPoints);
	}

	private void HandleChangeZombieStateEvent (Zombie zombie) {
		if (zombie.State == Zombie.ZombieState.Dead) {
			Score += ZOMBIE_COST;
			StartCoroutine(StartDestroyZombie(zombie));
			if ((IsMassacreOn) && (zombie.Type == Zombie.ZombieType.Third)) {
				StopMassacre();
			}
		}
	}

	private IEnumerator StartDestroyZombie(Zombie zombie) {
		yield return new WaitForSeconds (2f);
		if (zombie != null)
			DestroyZombie(zombie);
	}

	private Zombie.ZombieType GenerateZombieType() {
		int type = (int)Mathf.Round(Random.Range (1f, 3f));
		if (type == 1)
			return Zombie.ZombieType.First;
		else if (type == 2)
			return Zombie.ZombieType.Second;

		return Zombie.ZombieType.Third; 
	}

	private void HandleStopEvent (Zombie zombie){

		if ((zombie.State == Zombie.ZombieState.Alive) && (zombie.Type != Zombie.ZombieType.Third)) {
			if (IsMassacreOn) {
				SurvivedZombieCount++;
				DescreaseZombiesFromZombiePanel();
				if (SurvivedZombieCount == MAX_SURVIVED_ZOMBIE) {
					StopMassacre();
				}
			}
		}

		DestroyZombie(zombie);
	}

	private void StopMassacre() {
		IsMassacreOn = false;
		OpenResultDialog();
	}

	private void DestroyZombie(Zombie zombie) {
		Object.Destroy (zombie.gameObject);
	}
}

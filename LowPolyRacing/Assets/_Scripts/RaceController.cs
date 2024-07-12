using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;

public class RaceController : MonoBehaviour {

    [Header("References")]
    [SerializeField] private List<GameObject> checkpoints;
    [SerializeField] private List<Racer> racers = new List<Racer>();
    [SerializeField] private TextMeshProUGUI countdownTimerText;
    [SerializeField] private TextMeshProUGUI lapCounterText;
    [SerializeField] private TextMeshProUGUI checkpointCounterText;
    [SerializeField] private TextMeshProUGUI raceFinishedText;
    [SerializeField] private AudioClip[] countdownSound;

    [Header("Race Settings")]
    [SerializeField] private float raceCountdownTimer = 5f;
    [SerializeField] private float raceCountdownFadeOutTimer = 0.5f;
    [SerializeField] private string raceStartText = "GOOOO!";
    [SerializeField] private int maxLaps = 3;

    private bool setupFinished = false;
    private bool raceFinished = false;
    private int racingCars = 0;

    private float onceSecondTimer;
    public static float ONCE_PER_SECOND_INTERVAL = 1f;

    public static RaceController Instance;

    private void Awake() {
        if(Instance != null && Instance != null) {
            Destroy(this);
            return;
        }
        Instance = this;
    }

    private void Start() {
        raceFinished = false;
        raceFinishedText.enabled = false;
        SetupCheckpoints();
        SetupPlayers();
        onceSecondTimer = Time.time;
    }

    private void Update() {
        if(setupFinished) {
            if(raceCountdownTimer > 0) {
                raceCountdownTimer -= Time.deltaTime;

                if(Time.time >= onceSecondTimer) {
                    onceSecondTimer += ONCE_PER_SECOND_INTERVAL;
                    DisplayCountdownTime(raceCountdownTimer);
                }
            } else {
                DisplayCountdownTime(raceCountdownTimer);
                setupFinished = false;
                StartRace();
            }
        }
    }

    private void StartRace() {
        foreach(Racer racer in racers) {
            racer.GetController().SetIsActive(true);
        }
    }
    public bool IsRaceFinished() {
        return raceFinished;
    }

    private void DisplayCountdownTime(float raceCountdownTimer) {
        int seconds = Mathf.FloorToInt((raceCountdownTimer + 1) % 60);

        if(seconds <= 0) {
            countdownTimerText.text = raceStartText;
            StartCoroutine(FadeTextToZeroAlpha(raceCountdownFadeOutTimer, countdownTimerText));
            if(SoundFXManager.Instance != null) {
                SoundFXManager.Instance.PlaySoundFXClip(countdownSound[0], transform, 1f);
            }
        } else {
            countdownTimerText.text = seconds.ToString();
            StartCoroutine(FadeTextToZeroAlpha(raceCountdownFadeOutTimer, countdownTimerText));
            if(SoundFXManager.Instance != null) {
                SoundFXManager.Instance.PlaySoundFXClip(countdownSound[seconds], transform, 1f);
            }
        }
    }

    private IEnumerator FadeTextToZeroAlpha(float timer, TextMeshProUGUI text) {
        text.color = new Color(text.color.r, text.color.g, text.color.b);
        while(text.color.a > 0.0f) {
            text.color = new Color(text.color.r, text.color.g, text.color.b, text.color.a - (Time.deltaTime / timer));
            yield return null;
        }
    }

    private void SetupCheckpoints() {
        for(int i = 0; i < checkpoints.Count; i++) {
            checkpoints[i].GetComponent<Checkpoint>().SetId(i, i == 0);
        }
    }

    private void SetupPlayers() {
        CarController[] players = FindObjectsOfType<CarController>();
        for(int i = 0; i < players.Length; i++) {

            AddRacer(players[i]);
        }

        setupFinished = true;
    }

    public void AddRacer(CarController controller) {
        Racer racer = controller.GetComponent<Racer>();
        racer.SetId(racers.Count);
        racer.SetLaps(1);
        racer.SetCurrentCheckpoint(0);
        controller.racer = racer;
        racers.Add(racer);

        DisplayRaceStats(racer.GetLaps(), racer.GetCurrentCheckpoint());
        racingCars++;

        controller.SetIsActive(false);
    }

    public void CarThroughCheckpoint(CarController car, Checkpoint checkpoint) {
        Racer racer = racers.Find(r => r.GetId() == car.racer.GetId());
        if(racer != null) {
            int checkpointId = checkpoint.GetId();
            if(checkpointId == racer.GetCurrentCheckpoint() + 1 || (racer.GetCurrentCheckpoint() == checkpoints.Count - 1 && checkpoint.IsFinish())) {
                racer.SetCurrentCheckpoint(checkpointId);

                if(checkpoint.IsFinish()) {
                    if(racer.GetLaps() >= maxLaps) {
                        // Race won
                        car.SetIsActive(false);
                        racingCars--;

                        if(racingCars <= 0) {
                            raceFinished = true;
                            DisplayFinishedRace();
                        }
                    } else {
                        int laps = racer.GetLaps();
                        laps++;
                        racer.SetLaps(laps);
                    }
                }

                DisplayRaceStats(racer.GetLaps(), racer.GetCurrentCheckpoint());
            }
        }
    }

    private void DisplayRaceStats(int laps, int checkpoint) {

        lapCounterText.text = "Laps: " + laps.ToString() + " / " + maxLaps;
        checkpointCounterText.text = "CP: " + checkpoint.ToString() + " / " + (checkpoints.Count - 1);
    }

    private void DisplayFinishedRace() {
        raceFinishedText.enabled = true;
    }

}

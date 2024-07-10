using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;

public class RaceController : MonoBehaviour {

    [Header("References")]
    [SerializeField] private List<GameObject> checkpoints;
    [SerializeField] private List<RacerSO> racers;
    [SerializeField] private TextMeshProUGUI countdownTimerText;
    [SerializeField] private TextMeshProUGUI lapCounterText;
    [SerializeField] private TextMeshProUGUI checkpointCounterText;

    [Header("Race Settings")]
    [SerializeField] private float raceCountdownTimer = 5f;
    [SerializeField] private float raceCountdownFadeOutTimer = 0.5f;
    [SerializeField] private string raceStartText = "GOOOO!";
    [SerializeField] private int maxLaps = 3;

    private bool setupFinished = false;

    private float onceSecondTimer;
    public static float ONCE_PER_SECOND_INTERVAL = 1f;

    public static RaceController Instance;

    private void Awake() {
        if(Instance != null) {
            Destroy(this);
        }
        Instance = this;

        SetupCheckpoints();
        SetupPlayers();
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
        foreach(RacerSO racer in racers) {
            racer.controller.SetIsActive(true);
        }
    }

    private void DisplayCountdownTime(float raceCountdownTimer) {
        int seconds = Mathf.FloorToInt((raceCountdownTimer + 1) % 60);

        if(seconds <= 0) {
            countdownTimerText.text = raceStartText;
            StartCoroutine(FadeTextToZeroAlpha(raceCountdownFadeOutTimer, countdownTimerText));
        } else {
            countdownTimerText.text = seconds.ToString();
            StartCoroutine(FadeTextToZeroAlpha(raceCountdownFadeOutTimer, countdownTimerText));
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
        racers = new List<RacerSO>();
        CarController[] players = FindObjectsOfType<CarController>();
        for(int i = 0; i < players.Length; i++) {
            racers.Add(players[i].racerSO);
            players[i].racerSO.id = i;
            players[i].racerSO.controller = players[i];
            players[i].racerSO.laps = 1;
            players[i].racerSO.currentCheckpoint = 0;

            DisplayRaceStats(players[i].racerSO.laps, players[i].racerSO.currentCheckpoint);

            players[i].SetIsActive(false);
        }

        setupFinished = true;
    }

    public void CarThroughCheckpoint(CarController car, Checkpoint checkpoint) {
        RacerSO racer = racers.Find(r => r.id == car.GetId());
        if(racer != null) {
            int checkpointId = checkpoint.GetId();
            if(checkpointId == racer.currentCheckpoint + 1 || (racer.currentCheckpoint == checkpoints.Count - 1 && checkpoint.IsFinish())) {
                racer.SetCurrentCheckpoint(checkpointId);

                if(checkpoint.IsFinish()) {
                    racer.laps++;
                }

                DisplayRaceStats(racer.laps, racer.currentCheckpoint);

                if(racer.laps > maxLaps) {
                    // Race won
                    car.SetIsActive(false);
                }
            }
        }
    }

    private void DisplayRaceStats(int laps, int checkpoint) {

        lapCounterText.text = "Laps: " + laps.ToString() + " / " + maxLaps;
        checkpointCounterText.text = "CP: " + checkpoint.ToString() + " / " + (checkpoints.Count - 1);
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;
using UnityEngine.InputSystem;

public class RaceController : MonoBehaviour {

    [Header("References")]
    [SerializeField] private List<GameObject> checkpoints;
    [SerializeField] private List<Racer> racers = new List<Racer>();
    [SerializeField] private List<Racer> racerPositions = new List<Racer>();
    [SerializeField] private List<Transform> spawnpoints;
    [SerializeField] private TextMeshProUGUI raceFinishedText;
    [SerializeField] private AudioClip[] countdownSound;

    [Header("Race Settings")]
    [SerializeField] private float raceCountdownTimer = 5f;
    [SerializeField] private float raceCountdownFadeOutTimer = 0.5f;
    [SerializeField] private string raceStartText = "GOOOO!";
    [SerializeField] private int maxLaps = 3;

    private float timeTillRaceStarted;

    private bool setupFinished = false;
    private bool raceFinished = false;
    private int racingCars = 0;

    private float onceSecondTimer;
    public static float ONCE_PER_SECOND_INTERVAL = 1f;

    public static RaceController Instance;

    private void Awake() {
        if(Instance != this && Instance != null) {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        maxLaps = PlayerManager.Instance.selectedTrackLaps;
    }

    private void Start() {
        raceFinished = false;
        raceFinishedText.enabled = false;
        SetupCheckpoints();
        SetupSpawnpoints();
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
        timeTillRaceStarted = Time.time;
    }
    public bool IsRaceFinished() {
        return raceFinished;
    }

    private void DisplayCountdownTime(float raceCountdownTimer) {
        int seconds = Mathf.FloorToInt((raceCountdownTimer + 1) % 60);

        if(seconds <= 0) {
            racers.ForEach(r => r.GetController().GetPlayerUIStats().SetCountdownTimerText(raceStartText, raceCountdownFadeOutTimer));
            if(SoundFXManager.Instance != null) {
                SoundFXManager.Instance.PlaySoundFXClip(countdownSound[0], transform, 1f);
            }
        } else {
            racers.ForEach(r => r.GetController().GetPlayerUIStats().SetCountdownTimerText(seconds.ToString(), raceCountdownFadeOutTimer));
            if(SoundFXManager.Instance != null) {
                SoundFXManager.Instance.PlaySoundFXClip(countdownSound[seconds], transform, 1f);
            }
        }
    }

    private void SetupCheckpoints() {
        for(int i = 0; i < checkpoints.Count; i++) {
            checkpoints[i].GetComponent<Checkpoint>().SetId(i, i == 0);
        }
    }

    private void SetupSpawnpoints() {
        PlayerManager.Instance.SetupSpawnpoints(spawnpoints);
    }

    private void SetupPlayers() {
        /*CarController[] players = FindObjectsOfType<CarController>();
        for(int i = 0; i < players.Length; i++) {

            AddRacer(players[i]);
        }*/

        PlayerManager.Instance.SetupPlayerCars();

        setupFinished = true;
    }

    public void AddRacer(CarController controller) {
        Racer racer = controller.GetComponent<Racer>();

        PlayerManager.Instance.SetPlayerComponents(controller.gameObject.GetComponent<PlayerInput>(), true);

        racer.SetId(racers.Count);
        racer.SetLaps(1);
        racer.SetCurrentCheckpoint(0);
        controller.racer = racer;
        racers.Add(racer);
        racerPositions.Add(racer);

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
                racer.SetCheckpointTime(timeTillRaceStarted);

                if(checkpoint.IsFinish()) {
                    if(racer.GetLaps() >= maxLaps) {
                        // Race won
                        car.SetIsActive(false);
                        racingCars--;

                        List<CarController> playerControllers = PlayerManager.Instance.GetCarControllerFromAllPlayers();
                        if(playerControllers.All(c => !c.IsActive()) || racingCars <= 0) {
                            raceFinished = true;
                            DisplayFinishedRace(racer.GetCheckpointTime());
                        }                        
                    } else {
                        int laps = racer.GetLaps();
                        laps++;
                        racer.SetLaps(laps);
                    }
                }

                DisplayRaceStats(racer.GetLaps(), racer.GetCurrentCheckpoint());

                RecalculatePositions(racer);
            }
        }
    }

    private void RecalculatePositions(Racer racer) {
        racerPositions = racerPositions.OrderByDescending(r => r.GetLaps()).ThenByDescending(r => r.GetCurrentCheckpoint()).ThenBy(r => r.GetCheckpointTime()).ToList();
        racer.SetPosition(racerPositions.IndexOf(racer) + 1);
    }

    private void DisplayRaceStats(int laps, int checkpoint) {
        racers.ForEach(r => r.GetController().GetPlayerUIStats().SetLapCounterText(laps, maxLaps));
        racers.ForEach(r => r.GetController().GetPlayerUIStats().SetCheckpointCounterText(checkpoint.ToString(), checkpoints.Count - 1));
    }

    private void DisplayFinishedRace(float finishTime) {
        raceFinishedText.enabled = true;
    }

    public void PauseCarSounds(bool isPaused) {
        foreach(Racer racer in racers) {
            racer.GetController().PauseCarSounds(isPaused);
        }
    }

}

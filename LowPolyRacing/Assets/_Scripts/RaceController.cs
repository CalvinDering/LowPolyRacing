using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;

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
        SetupAI();
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

        foreach(Racer racer in racers) {
            if(racer.IsAIRacer()) {
                continue;
            }

            if(seconds <= 0) {
                racer.GetController().GetPlayerUIStats().SetCountdownTimerText(raceStartText);
                if(SoundFXManager.Instance != null) {
                    SoundFXManager.Instance.PlaySoundFXClip(countdownSound[0], transform, 1f);
                }
            } else {
                racer.GetController().GetPlayerUIStats().SetCountdownTimerText(seconds.ToString());
                if(SoundFXManager.Instance != null) {
                    SoundFXManager.Instance.PlaySoundFXClip(countdownSound[seconds], transform, 1f);
                }
            }
        }
    }

    private void SetupCheckpoints() {
        for(int i = 0; i < checkpoints.Count; i++) {
            checkpoints[i].GetComponent<Checkpoint>().SetId(i, i == 0);
        }
    }

    public GameObject GetFinishCheckpoint() {
        return checkpoints[0];
    }

    private void SetupSpawnpoints() {
        PlayerManager.Instance.SetupSpawnpoints(spawnpoints);
    }

    private void SetupAI() {
        PlayerManager.Instance.SetupAICars();
    }

    private void SetupPlayers() {

        PlayerManager.Instance.SetupPlayerCars();

        setupFinished = true;
    }

    public void AddRacer(CarController controller, bool isAI = false) {
        Racer racer = controller.GetComponent<Racer>();

        PlayerManager.Instance.SetPlayerComponents(racer, true, isAI);

        racer.SetId(racers.Count);
        racer.SetLaps(1);
        racer.SetCurrentCheckpoint(0);
        racer.SetIsAIRacer(isAI);
        controller.racer = racer;
        racers.Add(racer);
        racerPositions.Add(racer);

        DisplayRaceStats(racer);        
        RecalculatePositions(racer);
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
                            DisplayFinishedRace();
                        }                        
                    } else {
                        int laps = racer.GetLaps();
                        laps++;
                        racer.SetLaps(laps);
                    }
                }

                DisplayRaceStats(racer);
                DisplayRacerCheckpointTime(racer);

                RecalculatePositions(racer);
            }
        }
    }

    private void RecalculatePositions(Racer racer) {
        racerPositions = racerPositions.OrderByDescending(r => r.GetLaps()).ThenByDescending(r => r.GetCurrentCheckpoint()).ThenBy(r => r.GetCheckpointTime()).ThenBy(r => DistanceToNextCheckpoint(r)).ToList();
        racer.SetPosition(racerPositions.IndexOf(racer) + 1);
        DisplayAllPositions();
    }

    private float DistanceToNextCheckpoint(Racer racer) {
        int nextCheckpoint = racer.GetCurrentCheckpoint() + 1;
        if(nextCheckpoint >= checkpoints.Count) {
            nextCheckpoint = 0;
        }

        float distance = Vector3.Distance(racer.transform.position, checkpoints[nextCheckpoint].transform.position);
        return distance;
    }

    private void DisplayRaceStats(Racer racer) {
        if(racer.IsAIRacer()) {
            return;
        }

        PlayerUIStats racerStats = racer.GetController().GetPlayerUIStats();
        racerStats.SetLapCounterText(racer.GetLaps(), maxLaps);
        racerStats.SetCheckpointCounterText(racer.GetCurrentCheckpoint().ToString(), checkpoints.Count - 1);
    }

    private void DisplayRacerCheckpointTime(Racer racer) {
        if(racer.IsAIRacer()) {
            return;
        }

        PlayerUIStats racerStats = racer.GetController().GetPlayerUIStats();
        if(raceFinished) {
            racerStats.SetCheckpointTimeText(racer.GetCheckpointTime(), false);
        } else {
            racerStats.SetCheckpointTimeText(racer.GetCheckpointTime());
        }
    }

    private void DisplayAllPositions() {
        foreach(Racer racer in racers) {
            if(racer.IsAIRacer()) {
                continue;
            }

            racer.GetController().GetPlayerUIStats().SetPosition(racer.GetPosition());
        }
    }

    private void DisplayFinishedRace() {
        raceFinishedText.enabled = true;
    }

    public void PauseCarSounds(bool isPaused) {
        foreach(Racer racer in racers) {
            racer.GetController().PauseCarSounds(isPaused);
        }
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class RaceController : MonoBehaviour {

    [Header("References")]
    [SerializeField] private List<GameObject> checkpoints;
    [SerializeField] private List<RacerSO> racers;

    [Header("Race Settings")]
    [SerializeField] private int maxLaps = 3;

    public static RaceController Instance;

    private void Awake() {
        if(Instance != null) {
            Destroy(this);
        }
        Instance = this;

        SetupCheckpoints();
        SetupPlayers();
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
            players[i].racerSO.laps = 0;
            players[i].racerSO.currentCheckpoint = -1;
        }
    }

    public void CarThroughCheckpoint(CarController car, Checkpoint checkpoint) {
        RacerSO racer = racers.Find(r => r.id == car.GetId());
        if(racer != null) {
            int checkpointId = checkpoint.GetId();
            if(checkpointId == racer.currentCheckpoint + 1 || (racer.currentCheckpoint == checkpoints.Count - 1 && checkpoint.IsFinish())) {
                racer.SetCurrentCheckpoint(checkpointId);

                if(checkpoint.IsFinish()) {

                    racer.laps++;

                    if(racer.laps > maxLaps) {
                        // Race won
                        car.enabled = false;
                    }
                }
            }
        }
    }

}

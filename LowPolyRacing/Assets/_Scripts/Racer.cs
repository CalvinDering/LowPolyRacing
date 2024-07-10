using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Racer : MonoBehaviour {

    private int id;
    private CarController controller;
    private int laps;
    private int currentCheckpoint;

    private void Awake() {
        controller = GetComponent<CarController>();
    }

    public void SetId(int id) {
        this.id = id;
    }

    public int GetId() {
        return id;
    }

    public CarController GetController() {
        return controller;
    }

    public void SetLaps(int laps) {
        this.laps = laps;
    }

    public int GetLaps() {
        return laps;
    }

    public void SetCurrentCheckpoint(int currentCheckpoint) {
        this.currentCheckpoint = currentCheckpoint;
    }
    
    public int GetCurrentCheckpoint() {
        return currentCheckpoint;
    }
}

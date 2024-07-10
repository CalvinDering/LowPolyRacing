using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Racer")]
public class RacerSO : ScriptableObject {
    public int id;
    public CarController controller;
    public int laps;
    public int currentCheckpoint;

    public void SetCurrentCheckpoint(int id) {
        currentCheckpoint = id;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour {

    private int checkpointId = -1;
    private bool isFinish = false;

    public void SetId(int id, bool isFinish = false) {
        checkpointId = id;
        this.isFinish = isFinish;
    }

    public int GetId() {
        return checkpointId;
    }

    public bool IsFinish() {
        return isFinish;
    }

    private void OnTriggerEnter(Collider other) {
        if(other.transform.parent.TryGetComponent(out CarController car)) {
            RaceController.Instance.CarThroughCheckpoint(car, this);
        }
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaypointNode : MonoBehaviour {

    public float minDistanceToReachWaypoint = 5f;

    public float maxSpeed = 0;

    public WaypointNode[] nextWaypointNode;

    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, minDistanceToReachWaypoint);
    }

}

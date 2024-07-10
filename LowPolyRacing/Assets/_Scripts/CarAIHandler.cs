using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CarAIHandler : MonoBehaviour {

    public enum AIMode {
        followPlayer,
        followWaypoints
    }

    [Header("AI Settings")]
    public AIMode aiMode;
    public float maxSpeed = 100;

    private Vector3 targetPosition = Vector3.zero;
    private Transform targetTransform = null;

    private WaypointNode currentWaypoint = null;
    private WaypointNode[] allWaypoints;

    private CarController controller;

    private void Awake() {
        controller = GetComponent<CarController>();
        allWaypoints = FindObjectsOfType<WaypointNode>();
    }

    private void FixedUpdate() {
        Vector2 inputVector = Vector2.zero;

        switch(aiMode) {
            case AIMode.followPlayer:
                FollowPlayer();
                break;
            case AIMode.followWaypoints:
                FollowWaypoints();
                break;
        }

        inputVector.y = TurnTowardTarget();
        inputVector.x = ApplyThrottleOrBrake(inputVector.y);


        controller.SetInput(inputVector);
    }

    private void FollowPlayer() {
        if(targetTransform == null) {
            targetTransform = GameObject.FindGameObjectWithTag("Player").transform;
        }

        if(targetTransform != null) {
            targetPosition = targetTransform.position;
        }
    }

    private void FollowWaypoints() {
        if(currentWaypoint == null) {
            currentWaypoint = FindClosestWaypoint();
        }

        if(currentWaypoint != null) {
            targetPosition = currentWaypoint.transform.position;

            float distanceToWaypoint = (targetPosition - transform.position).magnitude;

            if(distanceToWaypoint <= currentWaypoint.minDistanceToReachWaypoint) {
                if(currentWaypoint.maxSpeed > 0) {
                    maxSpeed = currentWaypoint.maxSpeed;
                } else {
                    maxSpeed = 1000;
                }

                currentWaypoint = currentWaypoint.nextWaypointNode[Random.Range(0, currentWaypoint.nextWaypointNode.Length)];
            }
        }

    }

    private WaypointNode FindClosestWaypoint() {
        return allWaypoints.OrderBy(w => Vector3.Distance(transform.position, w.transform.position)).FirstOrDefault();
    }

    private float ApplyThrottleOrBrake(float steering) {
        if(controller.GetCurrentCarLocalVelocity().z > maxSpeed) {
            return 0;
        }
        return 1.20f - Mathf.Abs(steering) / 1.0f;
    }

    private float TurnTowardTarget() {

        Vector3 vectorToTarget = targetPosition - transform.position;

        float angleToTarget = Vector3.SignedAngle(vectorToTarget, transform.forward, Vector3.up);
        angleToTarget *= -1;

        float steerAmount = angleToTarget / 45.0f;
        steerAmount = Mathf.Clamp(steerAmount, -1.0f, 1.0f);

        return steerAmount;
    }

}

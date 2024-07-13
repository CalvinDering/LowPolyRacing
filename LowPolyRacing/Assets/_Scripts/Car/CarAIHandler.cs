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
    public float skillLevel = 1.0f;
    public float speedCap = 100;
    private float maxSpeed = 0;
    public float detectionRadius = 15f;
    public float maxDistance = 15f;
    public float waypointInfluence = 50f;
    public float waypointLineInfluenceDistance = 50f;
    public bool isAvoidingCars = true;
    public float jitterResponsiveness = 4f;
    public LayerMask layerMask;

    private Vector3 targetPosition = Vector3.zero;
    private Transform targetTransform = null;

    private Vector3 avoidanceVectorLerped = Vector3.zero;

    private Collider[] colliders;

    private WaypointNode currentWaypoint = null;
    private WaypointNode previousWaypoint = null;
    private WaypointNode[] allWaypoints;

    private CarController controller;

    private void Awake() {
        controller = GetComponent<CarController>();
        allWaypoints = FindObjectsOfType<WaypointNode>();
        colliders = GetComponentsInChildren<Collider>();

        maxSpeed = speedCap;
    }

    private void Start() {
        SetMaxSpeedBasedOnSkill(speedCap);
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
            currentWaypoint = FindFinishLineWaypoint();
            if(currentWaypoint == null) {
                currentWaypoint = FindClosestWaypoint();
            }
            previousWaypoint = currentWaypoint;
        }

        if(currentWaypoint != null) {
            targetPosition = currentWaypoint.transform.position;

            float distanceToWaypoint = (targetPosition - transform.position).magnitude;

            if(distanceToWaypoint > 50) {
                Vector3 nearestPoint = FindNearestPointOnLine(previousWaypoint.transform.position, currentWaypoint.transform.position, transform.position);

                float segment = distanceToWaypoint / waypointLineInfluenceDistance;

                targetPosition = (targetPosition + nearestPoint * segment) / (segment + 1);

                Debug.DrawLine(transform.position, targetPosition, Color.cyan);
            }

            if(distanceToWaypoint <= currentWaypoint.minDistanceToReachWaypoint) {
                if(currentWaypoint.maxSpeed > 0) {
                    SetMaxSpeedBasedOnSkill(currentWaypoint.maxSpeed);
                } else {
                    SetMaxSpeedBasedOnSkill();
                }

                previousWaypoint = currentWaypoint;

                currentWaypoint = currentWaypoint.nextWaypointNode[Random.Range(0, currentWaypoint.nextWaypointNode.Length)];
            }
        }
    }

    private WaypointNode FindFinishLineWaypoint() {
        return allWaypoints.First(w => w.isFinishLine);
    }

    private WaypointNode FindClosestWaypoint() {
        return allWaypoints.OrderBy(w => Vector3.Distance(transform.position, w.transform.position)).FirstOrDefault();
    }

    private float ApplyThrottleOrBrake(float steering) {
        if(controller.GetCurrentCarLocalVelocity().z > speedCap) {
            return 0;
        }

        float reduceSpeedDueToCornering = Mathf.Abs(steering) / 1.0f;
        float acceleration = 1.20f - reduceSpeedDueToCornering * skillLevel;
        acceleration = Mathf.Clamp(acceleration, 0f, 1f);

        return acceleration;
    }

    private void SetMaxSpeedBasedOnSkill(float newSpeed = 1000) {
        speedCap = Mathf.Clamp(newSpeed, 0f, maxSpeed);

        float skillBasedMaximumSpeed = Mathf.Clamp(skillLevel, 0.3f, 1.0f);
        speedCap = speedCap * skillBasedMaximumSpeed;
    }

    private float TurnTowardTarget() {

        Vector3 vectorToTarget = targetPosition - transform.position;

        if(isAvoidingCars) {
            AvoidCars(vectorToTarget, out vectorToTarget);
        }

        float angleToTarget = Vector3.SignedAngle(vectorToTarget, transform.forward, Vector3.up);
        angleToTarget *= -1;

        float steerAmount = angleToTarget / 45.0f;
        steerAmount = Mathf.Clamp(steerAmount, -1.0f, 1.0f);

        return steerAmount;
    }

    private Vector3 FindNearestPointOnLine(Vector3 startPosition, Vector3 endPosition, Vector3 point) {
        Vector3 headingVector = endPosition - startPosition;

        float maxDistance = headingVector.magnitude;
        headingVector.Normalize();

        Vector3 vectorStartToPoint = point - startPosition;
        float dotProduct = Vector3.Dot(vectorStartToPoint, headingVector);

        dotProduct = Mathf.Clamp(dotProduct, 0f, maxDistance);

        return startPosition + headingVector * dotProduct;
    }

    private void AvoidCars(Vector3 vectorToTarget, out Vector3 newVectorToTarget) {
        if(IsCarInFrontOfCar(out Vector3 otherCarPosition, out Vector3 otherCarForwardsVector)) {
            Vector3 avoidanceVector = Vector3.zero;

            avoidanceVector = Vector3.Reflect((otherCarPosition - transform.position).normalized, otherCarPosition + otherCarForwardsVector);

            float distanceToTarget = vectorToTarget.magnitude;
            float driveToTargetInfluence = waypointInfluence / distanceToTarget;

            driveToTargetInfluence = Mathf.Clamp(driveToTargetInfluence, 0.3f, 1.0f);
            float avoidanceInfluence = 1.0f - driveToTargetInfluence;

            avoidanceVectorLerped = Vector3.Lerp(avoidanceVectorLerped, avoidanceVector, Time.fixedDeltaTime * jitterResponsiveness);

            newVectorToTarget = vectorToTarget * driveToTargetInfluence + avoidanceVectorLerped * avoidanceInfluence;
            newVectorToTarget.Normalize();

            Debug.DrawRay(transform.position, avoidanceVector * 10, Color.yellow);

            Debug.DrawRay(transform.position, newVectorToTarget * 10, Color.green);

            return;
        }

        newVectorToTarget = vectorToTarget;
    }

    private bool IsCarInFrontOfCar(out Vector3 position, out Vector3 otherCarForwardVector) {
        RaycastHit hit;
        Physics.SphereCast(transform.position, detectionRadius, transform.forward, out hit, maxDistance, layerMask);

        if(hit.collider != null && !colliders.Contains(hit.collider)) {
            Debug.DrawRay(transform.position, transform.forward * maxDistance, Color.red);
            position = hit.collider.transform.position;
            otherCarForwardVector = hit.collider.transform.forward;
            Debug.DrawLine(position, position + otherCarForwardVector * 10, Color.magenta);

            return true;
        }

        position = Vector3.zero;
        otherCarForwardVector = Vector3.zero;

        return false;
    }

    private void OnDrawGizmos() {
        Gizmos.DrawWireSphere(transform.position + transform.forward * maxDistance, detectionRadius);
    }

}

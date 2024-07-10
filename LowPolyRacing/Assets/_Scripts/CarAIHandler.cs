using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarAIHandler : MonoBehaviour {

    public enum AIMode {
        followPlayer,
        followWaypoints
    }

    [Header("AI Settings")]
    public AIMode aiMode;

    private Vector3 targetPosition = Vector3.zero;
    private Transform targetTransform = null;

    private CarController controller;

    private void Awake() {
        controller = GetComponent<CarController>();
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

        inputVector.x = 1f;
        inputVector.y = TurnTowardTarget();


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

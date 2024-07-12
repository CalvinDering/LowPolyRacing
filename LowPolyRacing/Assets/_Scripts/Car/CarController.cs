using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour {

    [Header("References")]
    [SerializeField] private Rigidbody carRB;
    [SerializeField] private Transform[] rayPoints;
    [SerializeField] private LayerMask drivable;
    [SerializeField] private Transform accelerationPoint;
    [SerializeField] private GameObject[] tires = new GameObject[4];
    [SerializeField] private GameObject[] frontTireParents = new GameObject[2];

    [SerializeField] private TrailRenderer[] skidMarks = new TrailRenderer[2];
    [SerializeField] private ParticleSystem[] skidSmokes = new ParticleSystem[2];
    [SerializeField] private AudioSource engineSound;
    [SerializeField] private AudioSource skidSound;

    [Header("Suspension Settings")]
    [SerializeField] private float springStiffness;
    [SerializeField] private float damperStiffness;
    [SerializeField] private float restLength;
    [SerializeField] private float springTravel;
    [SerializeField] private float wheelRadius;

    [Header("Input")]
    public Racer racer;
    private float moveInput = 0;
    private float steerInput = 0;

    [Header("Car Settings")]
    [SerializeField] private float acceleration = 25f;

    [SerializeField] private float maxSpeed = 100f;
    [SerializeField] private float deceleration = 10f;
    [SerializeField] private float steerStrength = 15f;
    [SerializeField] private AnimationCurve turningCurve;
    [SerializeField] private float dragCoefficient = 1f;
    [SerializeField] private float brakingDeceleration = 100f;
    [SerializeField] private float brakingDragCoefficient = 0.5f;
    [SerializeField] private float gravityForce = -9.81f;
    [SerializeField] private float gravityMultiplier = 10f;

    private Vector3 currectCarLocalVelocity = Vector3.zero;
    private float carVelocityRatio = 0;

    private int[] wheelsIsGrounded = new int[4];
    private bool isGrounded = false;
    private bool isActive = false;
    private bool handbrakeActive = false;

    [Header("Visuals")]
    [SerializeField] private float tireRotSpeed = 3000f;
    [SerializeField] private float maxSteeringAngle = 30f;
    [SerializeField] private float minSideSkidVelocity = 10f;

    [Header("Audio")]
    [SerializeField] [Range(0, 1)] private float minPitch = 1f;
    [SerializeField] [Range(1, 5)] private float maxPitch = 5f;

    private void Awake() {
        carRB = GetComponent<Rigidbody>();

        if(gameObject.tag == "Player") {

        } else if(gameObject.tag == "AI") {
        
        }
    }

    private void FixedUpdate() {
        Suspension();
        GroundCheck();
        CalculateCarVelocity();
        Movement();
        TireVisuals();
        EngineSounds();
    }

    private void Update() {

        if(RaceController.Instance.IsRaceFinished() && Input.GetKeyDown(KeyCode.Escape)) {
            SceneHandler.Instance.LoadMenuScene();
        }
    }

    public void SetIsActive(bool isActive) {
        this.isActive = isActive;
    }

    #region Movement

    private void Movement() {
        if(!isActive) {
            SetZeroInput();
        }

        if(isGrounded) {
            Acceleration();
            Deceleration();
            Turn();
            SidewaysDrag();
        } else {
            DownwardsForce();
        }
    }

    private void Acceleration() {
        if(currectCarLocalVelocity.z < maxSpeed) {
            carRB.AddForceAtPosition(acceleration * moveInput * transform.forward, accelerationPoint.position, ForceMode.Acceleration);
        }
    }

    private void Deceleration() {
        carRB.AddForce((handbrakeActive ? brakingDeceleration : deceleration) * carVelocityRatio * -carRB.transform.forward, ForceMode.Acceleration);
    }

    private void Turn() {
        carRB.AddRelativeTorque(steerStrength * steerInput * turningCurve.Evaluate(Mathf.Abs(carVelocityRatio)) * Mathf.Sign(carVelocityRatio) * transform.up, ForceMode.Acceleration);
    }

    private void SidewaysDrag() {
        float currentSidewaysSpeed = currectCarLocalVelocity.x;
        float dragMagnitude = -currentSidewaysSpeed * (handbrakeActive ? brakingDragCoefficient : dragCoefficient);

        Vector3 dragForce = transform.right * dragMagnitude;

        carRB.AddForceAtPosition(dragForce, carRB.worldCenterOfMass, ForceMode.Acceleration);
    }

    private void DownwardsForce() {
        carRB.AddForce(Vector3.up * gravityForce * gravityMultiplier, ForceMode.Acceleration);
    }

    public Vector3 GetCurrentCarLocalVelocity() {
        return currectCarLocalVelocity;
    }

    #endregion

    #region Input

    public void SetInput(Vector2 input) {
        moveInput = input.x;
        steerInput = input.y;
    }

    private void SetZeroInput() {
        moveInput = 0;
        steerInput = 0;
    }

    public void SetHandbrake(bool value) {
        handbrakeActive = value;
    }

    #endregion

    #region Car Status

    private void GroundCheck() {
        int tempGroundedWheels = 0;

        for(int i = 0; i < wheelsIsGrounded.Length; i++) {
            tempGroundedWheels += wheelsIsGrounded[i];
        }

        if(tempGroundedWheels > 2) {
            isGrounded = true;
        } else {
            isGrounded = false;
        }
    }

    private void CalculateCarVelocity() {
        currectCarLocalVelocity = transform.InverseTransformDirection(carRB.velocity);
        carVelocityRatio = currectCarLocalVelocity.z / maxSpeed;
    }

    #endregion

    #region Suspension

    private void Suspension() {
        for(int i = 0; i < rayPoints.Length; i++) {
            RaycastHit hit;
            float maxDistance = restLength + springTravel;

            if(Physics.Raycast(rayPoints[i].position, -rayPoints[i].up, out hit, maxDistance + wheelRadius, drivable)) {
                wheelsIsGrounded[i] = 1;

                float currentSpringLength = hit.distance - wheelRadius;
                float springCompression = restLength - currentSpringLength / springTravel;

                float springVelocity = Vector3.Dot(carRB.GetPointVelocity(rayPoints[i].position), rayPoints[i].up);
                float dampForce = damperStiffness * springVelocity;

                float springForce = springStiffness * springCompression;

                float netForce = springForce - dampForce;

                carRB.AddForceAtPosition(netForce * rayPoints[i].up, rayPoints[i].position);

                SetTirePosition(tires[i], hit.point + rayPoints[i].up * wheelRadius);

                Debug.DrawLine(rayPoints[i].position, hit.point, Color.red);
            } else {
                wheelsIsGrounded[i] = 0;

                SetTirePosition(tires[i], rayPoints[i].position - rayPoints[i].up * maxDistance);

                Debug.DrawLine(rayPoints[i].position, rayPoints[i].position + (wheelRadius + maxDistance) * -rayPoints[i].up, Color.green);
            }
        }
    }

    #endregion

    #region Visuals

    private void TireVisuals() {
        float steeringAngle = maxSteeringAngle * steerInput;

        for(int i = 0; i < tires.Length; i++) {
            if(i < 2) {
                tires[i].transform.Rotate(Vector3.right, tireRotSpeed * carVelocityRatio * Time.deltaTime, Space.Self);

                frontTireParents[i].transform.localEulerAngles = new Vector3(frontTireParents[i].transform.localEulerAngles.x, steeringAngle, frontTireParents[i].transform.localEulerAngles.z);
            } else {
                if(moveInput == 0) {
                    tires[i].transform.Rotate(Vector3.right, tireRotSpeed * carVelocityRatio * Time.deltaTime, Space.Self);
                } else {
                    tires[i].transform.Rotate(Vector3.right, tireRotSpeed * moveInput * Time.deltaTime, Space.Self);
                }
            }
        }

        Vfx();
    }

    private void SetTirePosition(GameObject tire, Vector3 targetPosition) {
        tire.transform.position = targetPosition;
    }

    private void Vfx() {
        if(isGrounded && Mathf.Abs(currectCarLocalVelocity.x) > minSideSkidVelocity && carVelocityRatio > 0) {
            ToggleSkidMarks(true);
            ToggleSkidSmokes(true);
            ToggleSkidSounds(true);
        } else {
            ToggleSkidMarks(false);
            ToggleSkidSmokes(false);
            ToggleSkidSounds(false);
        }
    }

    private void ToggleSkidMarks(bool toggle) {
        foreach(TrailRenderer trail in skidMarks) {
            trail.emitting = toggle;
        }
    }
    
    private void ToggleSkidSmokes(bool toggle) {
        foreach(ParticleSystem smoke in skidSmokes) {
            if(toggle) {
                smoke.Play();
            } else {
                smoke.Stop();
            }
        }
    }

    private void ToggleSkidSounds(bool toggle) {
        skidSound.mute = !toggle;
    }

    private void EngineSounds() {
        engineSound.pitch = Mathf.Lerp(minPitch, maxPitch, Mathf.Abs(carVelocityRatio));
    }

    #endregion

}

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AdvancedCarController : MonoBehaviour {

    private Rigidbody carRB;

    public WheelColliders wheelColliders;
    public WheelMeshes wheelMeshes;
    public WheelParticles wheelParticles;

    private float gasInput;
    private float brakeInput;
    private float steeringInput;
    private bool handbrakeActive;

    public float motorPower;
    public float brakePower;
    private float slipAngle;
    private float speed;
    private float speedClamped;
    public float frontBrakingDifference = 0.7f;
    public float slipAllowance = 0.1f;

    public float[] gearRatios;
    public float differentialRatio;
    private float currentTorque;
    private float clutch;

    public float RPM;
    public float redLine;
    public float idleRPM;
    public float wheelRPM;
    public int currentGear;
    public float increaseGearRPM;
    public float decreaseGearRPM;
    private float changeGearTime = 0.5f;

    private GearState gearState;

    public TextMeshProUGUI rpmText;
    public TextMeshProUGUI gearText;
    public TextMeshProUGUI debugText;
    public float minNeedleRotation;
    public float maxNeedleRotation;
    public Transform rpmNeedle;

    public GameObject smokePrefab;

    public AnimationCurve steeringCurve;
    public AnimationCurve hpToRPMCurve;

    private void Start() {
        carRB = GetComponent<Rigidbody>();
        InstantiateSmoke();
        gearState = GearState.Running;
    }

    private void Update() {

        CalculateSpeed();
        ApplyMotorPower();
        ApplyBrake();
        ApplySteering();
        CheckWheelSmoke();
        ApplyWheelPosition();
        DisplayStats();
    }

    private void CalculateSpeed() {
        speed = wheelColliders.RRWheel.rpm * wheelColliders.RRWheel.radius * 2f * Mathf.PI / 10f;
        speedClamped = Mathf.Lerp(speedClamped, speed, Time.deltaTime);
    }

    private void ApplyMotorPower() {
        currentTorque = CalculateTorque();
        wheelColliders.RLWheel.motorTorque = currentTorque * gasInput;
        wheelColliders.RRWheel.motorTorque = currentTorque * gasInput;
    }

    private float CalculateTorque() {
        float torque = 0f;
        if(RPM < idleRPM + 200 && gasInput == 0 && currentGear == 0) {
            gearState = GearState.Neutral;
        }

        if(gearState == GearState.Running && clutch > 0) {
            if(RPM > increaseGearRPM) {
                StartCoroutine(ChangeGear(1));
            } else if(RPM < decreaseGearRPM) {
                StartCoroutine(ChangeGear(-1));
            }
        }

        if(clutch < 0.1f) {
            RPM = Mathf.Lerp(RPM, Mathf.Max(idleRPM, redLine * gasInput) + Random.Range(50, -50), Time.deltaTime);
        } else {
            wheelRPM = Mathf.Abs((wheelColliders.RLWheel.rpm + wheelColliders.RRWheel.rpm) / 2f) * gearRatios[currentGear] * differentialRatio;
            RPM = Mathf.Lerp(RPM, Mathf.Max(idleRPM - 100, wheelRPM), Time.deltaTime * 3f);
            torque = (hpToRPMCurve.Evaluate(RPM / redLine) * motorPower / RPM) * gearRatios[currentGear] * differentialRatio * 5252f * clutch;
        }
        return torque;
    }

    private void ApplySteering() {
        float steeringAngle = steeringInput * steeringCurve.Evaluate(speedClamped);
        debugText.text = speed.ToString();
        if(slipAngle < 120f) {
            steeringAngle += Vector3.SignedAngle(transform.forward, carRB.velocity + transform.forward, Vector3.up);
        }
        steeringAngle = Mathf.Clamp(steeringAngle, -90, 90f);

        wheelColliders.FLWheel.steerAngle = steeringAngle;
        wheelColliders.FRWheel.steerAngle = steeringAngle;
    }

    private void ApplyBrake() {
        wheelColliders.FLWheel.brakeTorque = brakeInput * brakePower * frontBrakingDifference;
        wheelColliders.FRWheel.brakeTorque = brakeInput * brakePower * frontBrakingDifference;

        wheelColliders.RLWheel.brakeTorque = brakeInput * brakePower * (1 - frontBrakingDifference);
        wheelColliders.RRWheel.brakeTorque = brakeInput * brakePower * (1 - frontBrakingDifference);

        if(handbrakeActive) {
            clutch = 0;
            wheelColliders.RLWheel.brakeTorque = brakePower * 1000f;
            wheelColliders.RRWheel.brakeTorque = brakePower * 1000f;
        }
    }

    private void ApplyWheelPosition() {
        UpdateWheel(wheelColliders.FLWheel, wheelMeshes.FLWheel);
        UpdateWheel(wheelColliders.FRWheel, wheelMeshes.FRWheel);
        UpdateWheel(wheelColliders.RLWheel, wheelMeshes.RLWheel);
        UpdateWheel(wheelColliders.RRWheel, wheelMeshes.RRWheel);
    }

    private void UpdateWheel(WheelCollider wheelCollider, MeshRenderer wheelMesh) {
        Quaternion quat;
        Vector3 pos;

        wheelCollider.GetWorldPose(out pos, out quat);

        wheelMesh.transform.position = pos;
        wheelMesh.transform.rotation = quat;
    }

    public void SetInput(float accelerateInput, float steerInput, float clutchInput, float handbrakeInput) {
        gasInput = accelerateInput;
        steeringInput = steerInput;

        slipAngle = Vector3.Angle(transform.forward, carRB.velocity - transform.forward);

        float movingDirection = Vector3.Dot(transform.forward, carRB.velocity);
        if(gearState != GearState.Changing) {
            if(gearState == GearState.Neutral) {
                clutch = 0;
                if(Mathf.Abs(gasInput) > 0) {
                    gearState = GearState.Running;
                }
            } else {

                clutch = Mathf.Abs(1 - clutchInput);
            }
        } else {
            clutch = 0;
        }

        if(movingDirection < -0.5f && gasInput > 0) {
            brakeInput = Mathf.Abs(gasInput);
        } else if(movingDirection > 0.5f && gasInput < 0) {
            brakeInput = Mathf.Abs(gasInput);
        } else {
            brakeInput = 0;
        }

        handbrakeActive = handbrakeInput > 0.5f;
    }

    public float GetSpeedRatio() {
        float gas = Mathf.Clamp(Mathf.Abs(gasInput), 0.5f, 1.0f);
        return RPM * gas / redLine;
    }

    private IEnumerator ChangeGear(int gearChange) {

        gearState = GearState.CheckingChange;
        if(currentGear + gearChange >= 0) {
            if(gearChange > 0) {
                yield return new WaitForSeconds(0.7f);
                if(RPM < increaseGearRPM || currentGear >= gearRatios.Length -1) {
                    gearState = GearState.Running;
                    yield break;
                }
            }
            if(gearChange < 0) {
                yield return new WaitForSeconds(0.1f);
                if(RPM > decreaseGearRPM|| currentGear <= 0) {
                    gearState = GearState.Running;
                    yield break;
                }
            }
            gearState = GearState.Changing;
            yield return new WaitForSeconds(changeGearTime);
            currentGear += gearChange;
        }
        if(gearState != GearState.Neutral) {
            gearState = GearState.Running;
        }
    }

    private void InstantiateSmoke() {
        wheelParticles.FLWheel = Instantiate(smokePrefab, wheelColliders.FLWheel.transform.position - Vector3.up * wheelColliders.FLWheel.radius,
            Quaternion.identity, wheelColliders.FLWheel.transform).GetComponent<ParticleSystem>();
        wheelParticles.FRWheel = Instantiate(smokePrefab, wheelColliders.FRWheel.transform.position - Vector3.up * wheelColliders.FRWheel.radius,
            Quaternion.identity, wheelColliders.FRWheel.transform).GetComponent<ParticleSystem>();
        wheelParticles.RLWheel = Instantiate(smokePrefab, wheelColliders.RLWheel.transform.position - Vector3.up * wheelColliders.RLWheel.radius,
            Quaternion.identity, wheelColliders.RLWheel.transform).GetComponent<ParticleSystem>();
        wheelParticles.RRWheel = Instantiate(smokePrefab, wheelColliders.RRWheel.transform.position - Vector3.up * wheelColliders.RRWheel.radius,
            Quaternion.identity, wheelColliders.RRWheel.transform).GetComponent<ParticleSystem>();
    }

    private void CheckWheelSmoke() {
        CheckSmokeForWheel(wheelColliders.FLWheel, wheelParticles.FLWheel);
        CheckSmokeForWheel(wheelColliders.FRWheel, wheelParticles.FRWheel);
        CheckSmokeForWheel(wheelColliders.RLWheel, wheelParticles.RLWheel);
        CheckSmokeForWheel(wheelColliders.RRWheel, wheelParticles.RRWheel);
    }

    private void CheckSmokeForWheel(WheelCollider wheelCollider, ParticleSystem wheelParticle) {
        WheelHit wheelHit;
        if(wheelCollider.GetGroundHit(out wheelHit)) {
            if(Mathf.Abs(wheelHit.sidewaysSlip) + Mathf.Abs(wheelHit.forwardSlip) > slipAllowance) {
                wheelParticle.Play();
            } else {
                wheelParticle.Stop();
            }
        }        
    }

    private void DisplayStats() {
        rpmText.text = RPM + " rpm";
        gearText.text = (gearState == GearState.Neutral) ? "N" : (currentGear + 1).ToString();
        rpmNeedle.localRotation = Quaternion.Euler(0, 0, Mathf.Lerp(minNeedleRotation, maxNeedleRotation, RPM / (redLine * 1.1f)));
    }
}

public enum GearState {
    Neutral,
    Running,
    CheckingChange,
    Changing
}

[System.Serializable]
public class WheelColliders {
    public WheelCollider FLWheel;
    public WheelCollider FRWheel;
    public WheelCollider RLWheel;
    public WheelCollider RRWheel;
}

[System.Serializable]
public class WheelMeshes {
    public MeshRenderer FLWheel;
    public MeshRenderer FRWheel;
    public MeshRenderer RLWheel;
    public MeshRenderer RRWheel;
}

[System.Serializable]
public class WheelParticles {
    public ParticleSystem FLWheel;
    public ParticleSystem FRWheel;
    public ParticleSystem RLWheel;
    public ParticleSystem RRWheel;
}

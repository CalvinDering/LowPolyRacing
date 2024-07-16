using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CarInputHandler : MonoBehaviour {

    private InputActionAsset inputActionAsset;
    private InputActionMap player;
    private InputAction accelerate;
    private InputAction steering;
    private InputAction handbrake;
    private InputAction clutch;

    public InputAction pause;
    public InputAction exit;

    private float accelerateInput;
    private float steeringInput;
    private float clutchInput;
    private float handbrakeInput;

    private float accelerateDamp;
    private float steerDamp;
    private float clutchDamp;
    public float dampenSpeed = 1f;

    public int playerNumber = 1;

    CarController carController;
    //AdvancedCarController carController;

    private void Awake() {
        carController = GetComponent<CarController>();
        //carController = GetComponent<AdvancedCarController>();

        inputActionAsset = GetComponent<PlayerInput>().actions;
        player = inputActionAsset.FindActionMap("Player");
    }

    private void Update() {
        /*if(pause.triggered) {
            PauseUIHandler.Instance.TogglePause();
        }

        if(PauseUIHandler.Instance.IsPaused()) {
            return;
        }*/

        Vector2 inputVector = Vector2.zero;
        accelerateDamp = DampenInput(accelerate.ReadValue<float>(), accelerateDamp);
        steerDamp = DampenInput(steering.ReadValue<float>(), steerDamp);
        //clutchDamp = DampenInput(clutch.ReadValue<float>(), clutchDamp);

        inputVector.x = accelerate.ReadValue<float>();
        inputVector.y = steering.ReadValue<float>();

        //carController.SetInput(accelerateDamp, steerDamp, clutchDamp, handbrakeInput);
        carController.SetInput(inputVector);

    }

    private void OnEnable() {
        accelerate = player.FindAction("Accelerate");
        steering = player.FindAction("Steer");
        handbrake = player.FindAction("Handbrake");
        clutch = player.FindAction("Clutch");
        pause = player.FindAction("Pause");
        exit = player.FindAction("Exit");

        accelerate.performed += ApplyAcceleration;
        accelerate.canceled += ReleaseAcceleration;
        steering.performed += ApplySteering;
        steering.canceled += ReleaseSteering;
        clutch.performed += ApplyClutch;
        clutch.canceled += ReleaseClutch;
        handbrake.performed += ApplyHandbrake;
        handbrake.canceled += ReleaseHandbrake;

        handbrake.started += context => carController.SetHandbrake(true);
        handbrake.canceled += context => carController.SetHandbrake(false);

        //clutch.started += context => carController.SetClutch(true);
        //clutch.canceled += context => carController.SetClutch(false);

        player.Enable();
    }
    
    private void OnDisable() {
        handbrake.started -= context => carController.SetHandbrake(true);
        handbrake.canceled -= context => carController.SetHandbrake(false);

        //clutch.started -= context => carController.SetClutch(true);
        //clutch.canceled -= context => carController.SetClutch(false);

        player.Disable();
    }

    private void ApplyAcceleration(InputAction.CallbackContext value) {
        accelerateInput = value.ReadValue<float>();
    }

    private void ReleaseAcceleration(InputAction.CallbackContext value) {
        accelerateInput = 0;
    }

    private void ApplySteering(InputAction.CallbackContext value) {
        steeringInput = value.ReadValue<float>();
    }

    private void ReleaseSteering(InputAction.CallbackContext value) {
        steeringInput = 0;
    }

    private void ApplyClutch(InputAction.CallbackContext value) {
        clutchInput = value.ReadValue<float>();
    }

    private void ReleaseClutch(InputAction.CallbackContext value) {
        clutchInput = 0;
    }

    private void ApplyHandbrake(InputAction.CallbackContext value) {
        handbrakeInput = value.ReadValue<float>();
    }

    private void ReleaseHandbrake(InputAction.CallbackContext value) {
        handbrakeInput = 0;
    }

    private float DampenInput(float input, float output) {
        return Mathf.Lerp(output, input, Time.deltaTime * dampenSpeed);
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CarInputHandler : MonoBehaviour {

    private InputActionAsset inputActionAsset;
    private InputActionMap player;
    private InputAction accelerate;
    private InputAction steer;
    private InputAction handbrake;

    public int playerNumber = 1;

    CarController carController;

    private void Awake() {
        carController = GetComponent<CarController>();

        inputActionAsset = GetComponent<PlayerInput>().actions;
        player = inputActionAsset.FindActionMap("Player");
    }

    private void Update() {
        Vector2 inputVector = Vector2.zero;

        inputVector.x = accelerate.ReadValue<float>();
        inputVector.y = steer.ReadValue<float>();

        carController.SetInput(inputVector);
    }

    private void OnEnable() {
        accelerate = player.FindAction("Accelerate");
        steer = player.FindAction("Steer");
        handbrake = player.FindAction("Handbrake");

        handbrake.started += context => carController.SetHandbrake(true);
        handbrake.canceled += context => carController.SetHandbrake(false);

        player.Enable();
    }
    
    private void OnDisable() {
        handbrake.started -= context => carController.SetHandbrake(true);
        handbrake.canceled -= context => carController.SetHandbrake(false);

        player.Disable();
    }

}

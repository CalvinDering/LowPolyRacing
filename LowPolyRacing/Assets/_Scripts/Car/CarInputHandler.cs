using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarInputHandler : MonoBehaviour {

    CarController carController;

    private void Awake() {
        carController = GetComponent<CarController>();
    }

    private void Update() {
        Vector2 inputVector = Vector2.zero;

        inputVector.x = Input.GetAxis("Vertical");
        inputVector.y = Input.GetAxis("Horizontal");

        carController.SetInput(inputVector);
    }

}

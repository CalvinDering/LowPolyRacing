using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Powerup : MonoBehaviour {

    public PowerupEffect powerupEffect;
    public bool isPermanent;

    private void OnTriggerEnter(Collider other) {
        GameObject parent = other.GetComponentInParent<CarEffectHandler>().gameObject;
        if(parent != null) {
            powerupEffect.Apply(parent);
            if(!isPermanent) {
                Destroy(gameObject);
            }
        }
    }

}

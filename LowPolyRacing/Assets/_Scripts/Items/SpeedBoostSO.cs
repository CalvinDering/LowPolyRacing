using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Powerups/Speedboost")]
public class SpeedBoostSO : PowerupEffect {

    [SerializeField] private float modifier;
    [SerializeField] private float decreaseStep;

    public override void Apply(GameObject target) {
        CarEffectHandler effectHandler = target.GetComponent<CarEffectHandler>();
        effectHandler.SetSpeedModifier(modifier);
        effectHandler.SetSpeedModifierDecreaseStep(decreaseStep);
        effectHandler.ActivateSpeedModifier();
    }

}

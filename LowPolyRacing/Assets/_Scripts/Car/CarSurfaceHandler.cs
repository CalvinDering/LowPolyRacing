using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarSurfaceHandler : MonoBehaviour {

    [Header("Surface Detection")]
    public LayerMask surfaceLayer;
    [SerializeField] private Transform centerPoint;
    [SerializeField] private Vector3 extendSize;
    [SerializeField] private float updateDistance = 0.75f;

    private Collider[] surfaceColliderHits = new Collider[10];
    Vector3 lastSampledSurfacePosition = Vector3.one * 100000;

    Surface.SurfaceType currentSurface = Surface.SurfaceType.Road;

    private void Awake() {

    }

    private void Update() {

        if((transform.position - lastSampledSurfacePosition).sqrMagnitude < updateDistance) {
            return;
        }
        int numberOfColliders = Physics.OverlapBoxNonAlloc(centerPoint.position, extendSize, surfaceColliderHits, Quaternion.identity, surfaceLayer, QueryTriggerInteraction.Collide);

        float lastSurfaceYValue = -1000;

        for(int i = 0; i < numberOfColliders; i++) {
            Surface surface = surfaceColliderHits[i].GetComponent<Surface>();

            if(surface.transform.position.y > lastSurfaceYValue) {
                currentSurface = surface.surfaceType;
                lastSurfaceYValue = surface.transform.position.y;
            }
        }

        if(numberOfColliders == 0) {
            currentSurface = Surface.SurfaceType.Road;
        }

        lastSampledSurfacePosition = transform.position;
    }

    public Surface.SurfaceType GetCurrentSurface() {
        return currentSurface;
    }

}

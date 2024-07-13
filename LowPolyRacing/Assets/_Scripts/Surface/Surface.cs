using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Surface : MonoBehaviour {

    public enum SurfaceType {
        Road, Sand, Grass, Water, Oil
    };

    [Header("Surface")]
    public SurfaceType surfaceType;

    private void Start() {
        
    }

}

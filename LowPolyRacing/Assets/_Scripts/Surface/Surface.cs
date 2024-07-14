using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Surface : MonoBehaviour {

    public enum SurfaceType {
        Road, Grass, Sand, Water, Oil
    };

    [Header("Surface")]
    public SurfaceType surfaceType;
    public static float roadSurfaceDrag = 0.2f;
    public static float grassSurfaceDrag = 1f;
    public static float sandSurfaceDrag = 2f;
    public static float waterSurfaceDrag = 0.1f;
    public static float oilSurfaceDrag = 0.01f;

    public static Color roadSurfaceSmokeColor = new Color(0.83f, 0.83f, 0.83f);
    public static Color grassSurfaceSmokeColor = new Color(0.15f, 0.545f, 0.027f);
    public static Color sandSurfaceSmokeColor = new Color(0.949f, 0.824f, 0.663f);
    public static Color waterSurfaceSmokeColor = new Color(0.34f, 0.844f, 0.962f);
    public static Color oilSurfaceSmokeColor = new Color(0.153f, 0.133f, 0.11f);

    public static float roadDriftModifier = 1f;
    public static float grassDriftModifier = 1.05f;
    public static float sandDriftModifier = 1.10f;
    public static float waterDriftModifier = 1f;
    public static float oilDriftModifier = 1f;

}

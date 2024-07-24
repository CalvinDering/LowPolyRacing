using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Splines;

[ExecuteInEditMode()]
public class SplineTrack : MonoBehaviour {

    private SplineContainer splineContainer;
    private Mesh mesh;

    [SerializeField] private TrackPart[] trackParts;

    private float3 position;
    private float3 forward;
    private float3 upVector;

    private List<Vector3> vertsP1;
    private List<Vector3> vertsP2;
    private List<Vector3> vertsA1;
    private List<Vector3> vertsA2;

    [SerializeField] private bool createDownVerticies = false;

    [SerializeField] private float roadHeight = 2f;
    [SerializeField] private Vector3 roadAngleVector = new Vector3(1, 1, 1);

    private void OnEnable() {
        splineContainer = GetComponent<SplineContainer>();
        Spline.Changed += OnSplineChanged;
        Rebuild();
    }

    private void OnDisable() {
        Spline.Changed -= OnSplineChanged;
    }

    private void OnSplineChanged(Spline arg1, int arg2, SplineModification arg3) {
        Rebuild();
    }

    public void Rebuild() {
        mesh = new Mesh();
        List<Vector3> verts = new List<Vector3>();

        GetVerts();

        mesh.subMeshCount = splineContainer.Splines.Count;
        int[][] triDex = new int[splineContainer.Splines.Count][];
        for(int s = 0; s < splineContainer.Splines.Count; s++) {
            BuildMesh(s, verts, triDex);
        }

        mesh.SetVertices(verts);
        for(int s = 0; s < splineContainer.Splines.Count; s++) {
            mesh.SetTriangles(triDex[s], s);
        }
        mesh.RecalculateNormals();

        MeshFilter meshFilter = GetComponent<MeshFilter>();
        if(meshFilter == null) {
            meshFilter = gameObject.AddComponent<MeshFilter>();
        }
        meshFilter.mesh = mesh;

        MeshCollider meshCollider = GetComponent<MeshCollider>();
        if(meshCollider == null) {
            meshCollider = gameObject.AddComponent<MeshCollider>();
        }
        meshCollider.sharedMesh = mesh;

        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        if(meshRenderer == null) {
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
        }
        Material[] combinedMaterials = new Material[splineContainer.Splines.Count];

        for(int s = 0; s < splineContainer.Splines.Count; s++) {
            combinedMaterials[s] = trackParts[s].trackMaterial;
        }
        meshRenderer.materials = combinedMaterials;
    }

    private void GetVerts() {

        vertsP1 = new List<Vector3>();
        vertsP2 = new List<Vector3>();
        vertsA1 = new List<Vector3>();
        vertsA2 = new List<Vector3>();

        for(int s = 0; s < splineContainer.Splines.Count; s++) {
            float step = 1f / (float) trackParts[s].resolution;
            for(int i = 0; i < trackParts[s].resolution; i++) {
                float t = step * i;

                SampleSplineWidth(s, t, out Vector3 p1, out Vector3 p2, out Vector3 p5, out Vector3 p6);
                vertsP1.Add(p1);
                vertsP2.Add(p2);
                vertsA1.Add(p5);
                vertsA2.Add(p6);
            }
        }
    }

    private void SampleSplineWidth(int splineIndex, float t, out Vector3 p1, out Vector3 p2, out Vector3 p5, out Vector3 p6) {
        splineContainer.Evaluate(splineIndex, t, out position, out forward, out upVector);

        float3 right = Vector3.Cross(forward, upVector).normalized;
        //float3 angleLeft = Vector3.Normalize(right * -upVector);
        p1 = position + (right * trackParts[splineIndex].roadWidth);
        p2 = position + (-right * trackParts[splineIndex].roadWidth);

        p5 = position + (right * trackParts[splineIndex].roadWidth + -upVector * roadHeight);
        p6 = position + (-right * trackParts[splineIndex].roadWidth + -upVector * roadHeight);
    }

    private void BuildMesh(int splineIndex, List<Vector3> verts, int[][] triDex) {
        List<int> tris = new List<int>();
        int offset = 0;

        int length = vertsP2.Count;
        int amountPerIndex = (int) trackParts[splineIndex].resolution;

        int lengthPoint = 0;
        for(int i = 0; i <= splineIndex; i++) {
            lengthPoint += (int) trackParts[i].resolution;
        }
        int previousAmount = lengthPoint - amountPerIndex;

        for(int i = 1 + previousAmount; i <= lengthPoint; i++) {
            Vector3 p1 = vertsP1[i - 1];
            Vector3 p2 = vertsP2[i - 1];
            Vector3 p3;
            Vector3 p4;

            Vector3 p5 = vertsA1[i - 1];
            Vector3 p6 = vertsA2[i - 1];
            Vector3 p7;
            Vector3 p8;

            if(i == length) {
                p3 = vertsP1[0];
                p4 = vertsP2[0];

                p7 = vertsA1[0];
                p8 = vertsA2[0];
            } else {
                p3 = vertsP1[i];
                p4 = vertsP2[i];

                p7 = vertsA1[i];
                p8 = vertsA2[i];
            }

            offset = (createDownVerticies ? 8 : 4) * (i - 1);

            //Top
            int t1 = offset + 0;
            int t2 = offset + 2;
            int t3 = offset + 3;

            int t4 = offset + 3;
            int t5 = offset + 1;
            int t6 = offset + 0;


            if(createDownVerticies) {
                //Left
                int t7 = offset + 0;
                int t8 = offset + 4;
                int t9 = offset + 2;

                int t10 = offset + 2;
                int t11 = offset + 4;
                int t12 = offset + 6;

                //Right
                int t13 = offset + 1;
                int t14 = offset + 3;
                int t15 = offset + 5;

                int t16 = offset + 3;
                int t17 = offset + 7;
                int t18 = offset + 5;

                //Bottom
                int t19 = offset + 7;
                int t20 = offset + 6;
                int t21 = offset + 4;

                int t22 = offset + 4;
                int t23 = offset + 5;
                int t24 = offset + 7;

                verts.AddRange(new List<Vector3> { p1, p2, p3, p4, p5, p6, p7, p8 });
                tris.AddRange(new List<int> {
                    t1, t2, t3, t4, t5, t6,
                    t7, t8, t9, t10, t11, t12,
                    t13, t14, t15, t16, t17, t18,
                    t19, t20, t21, t22, t23, t24
                });
            } else {
                verts.AddRange(new List<Vector3> { p1, p2, p3, p4 });
                tris.AddRange(new List<int> {
                    t1, t2, t3, t4, t5, t6
                });
            }

        }
        triDex[splineIndex] = tris.ToArray();
    }

}

[System.Serializable]
public class VertexWidth {
    public int knotIndex;
    public float vertexWidth;
}

[System.Serializable]
public class TrackPart {

    public Material trackMaterial;

    public float roadWidth;
    public float resolution;
}

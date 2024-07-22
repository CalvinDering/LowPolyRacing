using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

[ExecuteInEditMode()]
public class SplineTrack : MonoBehaviour {

    private SplineContainer splineContainer;
    private Mesh mesh;

    [SerializeField] private Material[] trackMaterials;

    [SerializeField] private float roadWidth;
    [SerializeField] private float resolution;

    private float3 position;
    private float3 forward;
    private float3 upVector;

    private List<Vector3> vertsP1;
    private List<Vector3> vertsP2;

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

        vertsP1 = new List<Vector3>();
        vertsP2 = new List<Vector3>();
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
        meshRenderer.materials = trackMaterials;

    }

    private void GetVerts() {

        vertsP1 = new List<Vector3>();
        vertsP2 = new List<Vector3>();

        for(int s = 0; s < splineContainer.Splines.Count; s++) {
            float step = 1f / (float) resolution;
            for(int i = 0; i < resolution; i++) {
                float t = step * i;

                SampleSplineWidth(s, t, out Vector3 p1, out Vector3 p2);
                vertsP1.Add(p1);
                vertsP2.Add(p2);
            }
        }
    }

    private void SampleSplineWidth(int splineIndex, float t, out Vector3 p1, out Vector3 p2) {
        splineContainer.Evaluate(splineIndex, t, out position, out forward, out upVector);

        float3 right = Vector3.Cross(forward, upVector).normalized;
        p1 = position + (right * roadWidth);
        p2 = position + (-right * roadWidth);
    }

    private void BuildMesh(int splineIndex, List<Vector3> verts, int[][] triDex) {
        List<int> tris = new List<int>();
        int offset = 0;

        int length = vertsP2.Count;
        int splineCount = splineContainer.Splines.Count;
        int amountPerSpline = length / splineCount;
        int amountPerIndex = amountPerSpline * splineIndex;
        int lengthPoint = amountPerIndex+ amountPerSpline;

        for(int i = 1 + amountPerIndex; i <= lengthPoint; i++) {
            Vector3 p1 = vertsP1[i - 1];
            Vector3 p2 = vertsP2[i - 1];
            Vector3 p3;
            Vector3 p4;

            if(i == length) {
                p3 = vertsP1[0];
                p4 = vertsP2[0];
            } else {
                p3 = vertsP1[i];
                p4 = vertsP2[i];
            }

            offset = 4 * (i - 1);

            int t1 = offset + 0;
            int t2 = offset + 2;
            int t3 = offset + 3;

            int t4 = offset + 3;
            int t5 = offset + 1;
            int t6 = offset + 0;

            verts.AddRange(new List<Vector3> { p1, p2, p3, p4 });
            tris.AddRange(new List<int> { t1, t2, t3, t4, t5, t6 });
        }
        triDex[splineIndex] = tris.ToArray();
    }

}

[System.Serializable]
public class VertexWidth {
    public int knotIndex;
    public float vertexWidth;
}

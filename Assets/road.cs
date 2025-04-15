using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class RoadMeshFromSpline : MonoBehaviour {
    [SerializeField] private SplineDone spline;
    [SerializeField] private float roadWidth = 1f;

    private Mesh mesh;

    void Start() {
        GenerateRoadMesh();
    }

    void GenerateRoadMesh() {
        if (spline == null) {
            Debug.LogError("Spline reference is missing!");
            return;
        }

        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        List<Vector3> splinePoints = spline.GetPointList();

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();

        for (int i = 0; i < splinePoints.Count; i++) {
            Vector3 forward;
            if (i < splinePoints.Count - 1) {
                forward = (splinePoints[i+1] - splinePoints[i]).normalized;
            } else {
                forward = (splinePoints[i] - splinePoints[i - 1]).normalized;
            }

            Vector3 left = Vector3.Cross(forward, Vector3.up) * (roadWidth / 2f);
            Vector3 right = -left;

            Vector3 leftPoint = splinePoints[i] + left;
            Vector3 rightPoint = splinePoints[i] + right;

            vertices.Add(transform.InverseTransformPoint(leftPoint));
            vertices.Add(transform.InverseTransformPoint(rightPoint));

            float v = i / (float)(splinePoints.Count - 1);
            uvs.Add(new Vector2(0, v));
            uvs.Add(new Vector2(1, v));

            if (i < splinePoints.Count - 1) {
                int start = i * 2;
                triangles.Add(start);
                triangles.Add(start + 2);
                triangles.Add(start + 1);

                triangles.Add(start + 1);
                triangles.Add(start + 2);
                triangles.Add(start + 3);
            }
        }

        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals();
    }
}

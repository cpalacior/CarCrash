using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class RoadMeshFromSpline : MonoBehaviour {
    [Header("Curve Settings")]
    [SerializeField] private Transform[] controlPoints;
    [SerializeField] private int resolutionPerSegment = 10;
    [SerializeField] private bool isLooping = true;

    [Header("Road Settings")]
    [SerializeField] private float roadWidth = 1f;
    [SerializeField] private float heightOffset = 0f; // Renamed to be more intuitive - positive values raise the road

    private Mesh mesh;

    void Start() {
        GenerateRoadMesh();
    }

    // Add a public method to regenerate the mesh during runtime if needed
    public void RegenerateRoad() {
        GenerateRoadMesh();
    }

    void GenerateRoadMesh() {
        if (controlPoints == null || controlPoints.Length < 4) {
            Debug.LogError("You need at least 4 control points for Catmull-Rom spline.");
            return;
        }

        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        List<Vector3> splinePoints = GenerateCatmullRomSpline(controlPoints, resolutionPerSegment);

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();

        for (int i = 0; i < splinePoints.Count; i++) {
            Vector3 forward;
            if (i < splinePoints.Count - 1) {
                forward = (splinePoints[i + 1] - splinePoints[i]).normalized;
            } else {
                forward = (splinePoints[i] - splinePoints[i - 1]).normalized;
            }

            Vector3 left = Vector3.Cross(forward, Vector3.up) * (roadWidth / 2f);
            Vector3 right = -left;

            // Changed to add height instead of subtracting
            Vector3 leftPoint = splinePoints[i] + left + Vector3.up * heightOffset;
            Vector3 rightPoint = splinePoints[i] + right + Vector3.up * heightOffset;

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

    List<Vector3> GenerateCatmullRomSpline(Transform[] points, int resolution) {
        List<Vector3> result = new List<Vector3>();
        int length = points.Length;

        int segmentCount = isLooping ? length : length - 3;

        for (int i = 0; i < segmentCount; i++) {
            int p0 = (i - 1 + length) % length;
            int p1 = i % length;
            int p2 = (i + 1) % length;
            int p3 = (i + 2) % length;

            for (int j = 0; j < resolution; j++) {
                float t = j / (float)resolution;
                Vector3 pos = 0.5f * (
                    2f * points[p1].position +
                    (-points[p0].position + points[p2].position) * t +
                    (2f * points[p0].position - 5f * points[p1].position + 4f * points[p2].position - points[p3].position) * t * t +
                    (-points[p0].position + 3f * points[p1].position - 3f * points[p2].position + points[p3].position) * t * t * t
                );
                result.Add(pos);
            }
        }

        return result;
    }

    // Optional: Add method to update in Editor (if you want to see changes immediately)
    private void OnValidate() {
        if (Application.isPlaying) {
            GenerateRoadMesh();
        }
    }
}
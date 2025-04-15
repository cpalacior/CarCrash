using System;
using System.Collections.Generic;
using UnityEngine;

public class SplineDone : MonoBehaviour
{
    [Serializable]
    public class Anchor
    {
        public Transform anchorTransform;
        public Transform handleATransform;
        public Transform handleBTransform;

        public Vector3 position => anchorTransform != null ? anchorTransform.position : Vector3.zero;
        public Vector3 handleAPosition => handleATransform != null ? handleATransform.position : position;
        public Vector3 handleBPosition => handleBTransform != null ? handleBTransform.position : position;
    }

    [SerializeField] private List<Anchor> anchorList = new List<Anchor>();
    [SerializeField] private bool closedLoop = false;

    public List<Anchor> GetAnchorList() => anchorList;
    public bool GetClosedLoop() => closedLoop;

    public Vector3 GetPositionAt(float t)
    {
        int anchorCount = anchorList.Count;
        if (anchorCount < 2) return Vector3.zero;

        int numCurves = closedLoop ? anchorCount : anchorCount - 1;

        float tCurve = t * numCurves;
        int curveIndex = Mathf.FloorToInt(tCurve);
        float localT = tCurve - curveIndex;

        int i0 = curveIndex % anchorCount;
        int i1 = (i0 + 1) % anchorCount;

        Anchor a = anchorList[i0];
        Anchor b = anchorList[i1];

        return CubicLerp(a.position, a.handleBPosition, b.handleAPosition, b.position, localT);
    }

    public Vector3 CubicLerp(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float t)
    {
        Vector3 ab = Vector3.Lerp(a, b, t);
        Vector3 bc = Vector3.Lerp(b, c, t);
        Vector3 cd = Vector3.Lerp(c, d, t);
        Vector3 abbc = Vector3.Lerp(ab, bc, t);
        Vector3 bccd = Vector3.Lerp(bc, cd, t);
        return Vector3.Lerp(abbc, bccd, t);
    }

    private void OnDrawGizmos()
    {
        if (anchorList == null || anchorList.Count < 2) return;

        Gizmos.color = Color.green;
        Vector3 prev = GetPositionAt(0);
        for (int i = 1; i <= 50; i++)
        {
            float t = i / 50f;
            Vector3 pos = GetPositionAt(t);
            Gizmos.DrawLine(prev, pos);
            prev = pos;
        }

        // Draw anchors and handles
        foreach (var anchor in anchorList)
        {
            if (anchor.anchorTransform == null) continue;

            Gizmos.color = Color.red;
            Gizmos.DrawSphere(anchor.position, 0.1f);

            if (anchor.handleATransform != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(anchor.position, anchor.handleAPosition);
            }

            if (anchor.handleBTransform != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(anchor.position, anchor.handleBPosition);
            }
        }
    }

    public List<Vector3> GetPointList(int resolution = 50)
    {
        List<Vector3> points = new List<Vector3>();

        for (int i = 0; i <= resolution; i++)
        {
            float t = i / (float)resolution;
            points.Add(GetPositionAt(t));
        }

        return points;
    }
}

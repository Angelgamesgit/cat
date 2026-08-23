using System.Collections.Generic;
using UnityEngine;

public class SphereTrailRecorder : MonoBehaviour
{
    [Header("Sphere")]
    [SerializeField] private Transform sphere;

    [Header("Trail Settings")]
    [SerializeField] private float recordDistance = 0.05f;
    [SerializeField] private int maxPoints = 500;

    private List<Vector3> positions = new List<Vector3>();

    public IReadOnlyList<Vector3> Positions => positions;

    private void Start()
    {
        if (sphere == null)
        {
            Debug.LogError("SphereTrailRecorder: Sphereが設定されていません。");
            return;
        }

        positions.Add(transform.position);
    }

    private void Update()
    {
        RecordPosition();
    }

    private void RecordPosition()
    {
        if (positions.Count == 0)
        {
            positions.Add(transform.position);
            return;
        }

        Vector3 lastPosition = positions[positions.Count - 1];

        if (Vector3.Distance(transform.position, lastPosition) >= recordDistance)
        {
            positions.Add(transform.position);

            if (positions.Count > maxPoints)
            {
                positions.RemoveAt(0);
            }
        }
    }
}
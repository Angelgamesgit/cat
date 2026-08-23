using UnityEngine;

public class Container_SphereTrailFollower : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SphereTrailRecorder recorder;
    [SerializeField] private Transform sphere;

    [Header("Follow Settings")]
    [SerializeField] private int trailDelay = 20;
    [SerializeField] private float moveSpeed = 8.0f;

    [Header("Surface Settings")]
    [SerializeField] private float surfaceOffset = 0.0f;

    private void Update()
    {
        if (recorder == null || sphere == null)
            return;

        var positions = recorder.Positions;

        if (positions.Count <= trailDelay)
            return;

        int targetIndex = positions.Count - 1 - trailDelay;

        Vector3 targetPosition = positions[targetIndex];

        MoveOnSphere(targetPosition);
    }

    private void MoveOnSphere(Vector3 targetPosition)
    {
        Vector3 sphereCenter = sphere.position;

        // Target位置から球体中心への方向
        Vector3 targetDirection =
            (targetPosition - sphereCenter).normalized;

        // Followerの現在位置から球体中心への方向
        Vector3 currentDirection =
            (transform.position - sphereCenter).normalized;

        // 球面上の方向を滑らかに補間
        Vector3 newDirection = Vector3.Slerp(
            currentDirection,
            targetDirection,
            moveSpeed * Time.deltaTime
        ).normalized;

        // 球体半径
        float radius =
            Vector3.Distance(sphereCenter, targetPosition);

        // 球体表面へ配置
        Vector3 newPosition =
            sphereCenter +
            newDirection * (radius + surfaceOffset);

        transform.position = newPosition;
    }
}
using UnityEngine;

public interface ITracking 
{
    Transform TrackingTransform { get; }

    Vector3 RayPoint { get; }
    LayerMask TargetLayer { get; }

    float Range { get; }
    float RotationSpeed { get; }
    float FOV { get; }

    private Vector3 TargetPosition => LevelManager.Instance.Player.transform.position;
    private Vector3 TargetDirection => TargetPosition - TrackingTransform.position;

    public float DistanceToTarget => Vector3.Distance(TrackingTransform.position, TargetPosition);
    private float MaxRotationDelta => RotationSpeed * Time.deltaTime;

    public bool InRange => DistanceToTarget < Range;

    bool CheckLineOfSight()
    {
        Physics.Raycast(RayPoint, TargetDirection, out RaycastHit hit, Range);

        if (hit.collider == null) return false;

        if (!hit.collider.CompareTag("Player")) return false;

        return true;
    }

    bool CheckFieldOfView()
    {
        float dot = Vector3.Dot(TrackingTransform.forward, TargetDirection.normalized);

        if (!(dot >= Mathf.Cos(FOV * Mathf.Deg2Rad * 0.5f))) return false;

        if (!Physics.Raycast(RayPoint, TargetDirection, out RaycastHit hit, Range, TargetLayer)) return false;

        if (!hit.collider.CompareTag("Player")) return false;

        return true;
    }

    void TrackTarget()
    {
        if (DistanceToTarget < Range)
        {
            TrackingTransform.LookAt(TargetPosition);
        }
    }

    void TrackTarget2D()
    {
        if (DistanceToTarget < Range)
        {
            TrackingTransform.LookAt(new Vector3(TargetPosition.x, TrackingTransform.position.y, TargetPosition.z));
        }
    }

    void LimitedTrackTarget()
    {
        Quaternion targetRotation = Quaternion.LookRotation(TargetDirection, Vector3.up);
        TrackingTransform.rotation = Quaternion.RotateTowards(TrackingTransform.rotation, targetRotation, MaxRotationDelta);
    }

    void LimitedTrackTarget2D()
    {
        Vector3 targetPosition = new Vector3(TargetPosition.x, TrackingTransform.position.y, TargetPosition.z);
        Quaternion targetRotation = Quaternion.LookRotation(targetPosition - TrackingTransform.position, Vector3.up);
        TrackingTransform.rotation = Quaternion.RotateTowards(TrackingTransform.rotation, targetRotation, MaxRotationDelta);
    }
}

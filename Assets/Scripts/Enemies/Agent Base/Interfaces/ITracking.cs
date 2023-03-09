using UnityEngine;

public interface ITracking 
{
    Transform TrackingTransform { get; }

    Transform RayPoint { get; }
    LayerMask TargetLayer { get; }

    float Range { get; }
    float RotationSpeed { get; }
    float FOV { get; }

    private Transform Target => LevelManager.Instance.Player.transform;
    private Vector3 TargetDirection => Target.position - TrackingTransform.position;
    private Vector3 RayTargetDirection => Target.position - RayPoint.position;
    public float DistanceToTarget => Vector3.Distance(TrackingTransform.position, Target.position);
    private float MaxRotationDelta => RotationSpeed * Time.deltaTime;

    public bool InRange => DistanceToTarget < Range;

    bool CheckLineOfSight()
    {
        Physics.Raycast(RayPoint.position, RayTargetDirection, out RaycastHit hit, Range);

        if (hit.collider == null) return false;

        if (!hit.collider.CompareTag("Player")) return false;

        return true;
    }

    bool CheckFieldOfView()
    {
        float dot = Vector3.Dot(TrackingTransform.forward, TargetDirection.normalized);

        if (!(dot >= Mathf.Cos(FOV * Mathf.Deg2Rad * 0.5f))) return false;

        if (!Physics.Raycast(RayPoint.position, RayTargetDirection, out RaycastHit hit, Range)) return false;

        if (!hit.collider.CompareTag("Player")) return false;

        return true;
    }

    void TrackTarget()
    {
        if (DistanceToTarget < Range)
        {
            TrackingTransform.LookAt(Target);
        }
    }

    void TrackTarget2D()
    {
        if (DistanceToTarget < Range)
        {
            TrackingTransform.LookAt(new Vector3(Target.position.x, TrackingTransform.position.y, Target.position.z));
        }
    }

    void LimitedTrackTarget()
    {
        Quaternion targetRotation = Quaternion.LookRotation(TargetDirection, Vector3.up);
        TrackingTransform.rotation = Quaternion.RotateTowards(TrackingTransform.rotation, targetRotation, MaxRotationDelta);
    }

    void LimitedTrackTarget2D()
    {
        Vector3 targetPosition = new Vector3(Target.position.x, TrackingTransform.position.y, Target.position.z);
        Quaternion targetRotation = Quaternion.LookRotation(targetPosition - TrackingTransform.position, Vector3.up);
        TrackingTransform.rotation = Quaternion.RotateTowards(TrackingTransform.rotation, targetRotation, MaxRotationDelta);
    }
}

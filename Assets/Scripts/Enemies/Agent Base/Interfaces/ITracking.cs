using UnityEngine;

public interface ITracking 
{
    Transform TrackingTransform { get; }

    Transform RayPoint { get; }
    LayerMask SightLayers { get; }

    float Range { get; }
    float RotationSpeed { get; }
    float FOV { get; }

    private Transform Target => LevelManager.Instance.Player.transform;
    private Vector3 TargetDirection => Target.position - TrackingTransform.position;
    private Vector3 RayTargetDirection => Target.position - RayPoint.position;
    public float DistanceToTarget => Vector3.Distance(TrackingTransform.position, Target.position);
    private float MaxRotationDelta => RotationSpeed * Time.deltaTime;

    public bool InRange => DistanceToTarget < Range;

    /// <summary>
    /// Raycast check without regard to FOV
    /// </summary>
    /// <returns></returns>
    bool CheckLineOfSight()
    {
        Physics.Raycast(RayPoint.position, RayTargetDirection, out RaycastHit hit, Range);

        if (hit.collider == null) return false;

        if (!hit.collider.CompareTag("Player")) return false;

        return true;
    }

    /// <summary>
    /// Raycast check with regard to FOV
    /// </summary>
    /// <returns></returns>
    bool CheckFieldOfView()
    {
        float dot = Vector3.Dot(TrackingTransform.forward, TargetDirection.normalized);

        if (!(dot >= Mathf.Cos(FOV * Mathf.Deg2Rad * 0.5f))) return false;

        if (!Physics.Raycast(RayPoint.position, RayTargetDirection, out RaycastHit hit, Range, SightLayers)) return false;

        if (!hit.collider.CompareTag("Player")) return false;

        return true;
    }

    /// <summary>
    /// Look at target if within range
    /// </summary>
    void TrackTarget()
    {
        TrackingTransform.LookAt(Target);
    }

    /// <summary>
    /// Look at target on 2D plane (will not look up or down)
    /// </summary>
    void TrackTarget2D()
    {
        TrackingTransform.LookAt(new Vector3(Target.position.x, TrackingTransform.position.y, Target.position.z));
    }

    /// <summary>
    /// Attempt to look at target with limited rotation speed
    /// </summary>
    void LimitedTrackTarget()
    {
        Quaternion targetRotation = Quaternion.LookRotation(TargetDirection);
        TrackingTransform.rotation = Quaternion.RotateTowards(TrackingTransform.rotation, targetRotation, MaxRotationDelta);

    }

    /// <summary>
    /// Attempt to look at target with limited rotation speed on a 2D plane
    /// </summary>
    void LimitedTrackTarget2D()
    {
        Vector3 targetPosition = new Vector3(Target.position.x, TrackingTransform.position.y, Target.position.z);
        Quaternion targetRotation = Quaternion.LookRotation(targetPosition - TrackingTransform.position);
        TrackingTransform.rotation = Quaternion.RotateTowards(TrackingTransform.rotation, targetRotation, MaxRotationDelta);
    }

}

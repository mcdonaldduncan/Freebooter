using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.Progress;

public class Follower : MonoBehaviour
{
    [SerializeField] float damage;
    [SerializeField] float maxSpeed;
    [SerializeField] float maxForce;

    Vector3 velocity;
    Vector3 acceleration;

    bool isInitialized = false;

    private Transform _Transform;
    private Transform _Target;
    private Inquisitor _Inquisitor;
    private List<IDamageable> _IDamageables = new List<IDamageable>();

    public void Init(Transform target, Inquisitor inquisitor)
    {
        _Target = target;
        _Inquisitor = inquisitor;
        _Transform.position = new Vector3(_Target.position.x, 15f, _Target.position.z);
        isInitialized = true;
    }

    Vector3 CalculateSteering(Vector3 currentTarget)
    {
        Vector3 desired = currentTarget - _Transform.position;
        desired = desired.normalized;
        desired *= maxSpeed;
        Vector3 steer = desired - velocity;
        steer = steer.normalized;
        steer *= maxForce;
        return steer;
    }

    void ApplySteering()
    {
        acceleration += CalculateSteering(_Target.position);
        velocity += acceleration;
        _Transform.position += velocity * Time.deltaTime;
        acceleration = Vector3.zero;
    }

    private void OnEnable()
    {
        _Transform = transform;
    }

    private void Update()
    {
        
        //_IDamageables = _IDamageables.Where(x => x != null).ToList();
        ApplySteering();

        //if (_IDamageables == null || _IDamageables.Count == 0)
        //    return;

        //for (int i = 0; i < _IDamageables.Count; i++)
        //{
        //    if (_IDamageables[i] == null)
        //        continue;
        //    _IDamageables[i].TakeDamage(damage * Time.deltaTime);
        //}
        if (!isInitialized)
            return;
        if (_Inquisitor == null)
            Destroy(gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.TryGetComponent<IDamageable>(out IDamageable damageable))
        {
            
        }
        else
        {
            _Inquisitor.isTracking = false;
            Destroy(gameObject);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        try
        {
            IDamageable damageable = other.transform.GetComponent<IDamageable>();
            damageable.TakeDamage(damage * Time.deltaTime);
            Debug.Log($"{other.transform.name}: {damageable.Health}");
        }
        catch
        {
            return;
        }
    }

    //private void OnTriggerEnter(Collider other)
    //{
    //    Debug.Log("triggered");
    //    try
    //    {
    //        IDamageable damageable = other.transform.GetComponent<IDamageable>();
    //        _IDamageables.Add(damageable);
    //        Debug.Log($"{other.transform.name}: {damageable.Health}");
    //    }
    //    catch
    //    {
    //        return;
    //    }
    //}

    //private void OnTriggerExit(Collider other)
    //{
    //    try
    //    {
    //        IDamageable damageable = other.gameObject.GetComponent<IDamageable>();
    //        _IDamageables.Remove(damageable);
    //    }
    //    catch
    //    {
    //        return;
    //    }
    //}

    //private void OnCollisionExit(Collision collision)
    //{
    //    if (collision.gameObject.TryGetComponent<IDamageable>(out IDamageable damageable))
    //    {
    //        try
    //        {
    //            _IDamageables.Remove(damageable);
    //        }
    //        catch
    //        {
    //            return;
    //        }
            
    //    }
    //}
}

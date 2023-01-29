
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

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

    private Collider col;

    private void OnEnable()
    {
        _Transform = transform;
        col = GetComponent<Collider>();
    }

    public void Init(Transform target, Inquisitor inquisitor, Vector3 position)
    {
        gameObject.SetActive(true);
        _Target = target;
        _Inquisitor = inquisitor;
        _Transform.position = position;
        isInitialized = true;
    }

    public void Despawn()
    {
        _Inquisitor.SetTracking(false);
        isInitialized = false;
        if (gameObject == null) return;
        
        gameObject.SetActive(false);
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
        _Transform.LookAt(_Transform.position + velocity);
        acceleration = Vector3.zero;
    }

    private void Update()
    {
        if (!isInitialized)
            return;
        ApplySteering();
    }

    //private void OnCollisionEnter(Collision collision)
    //{
    //    if (collision.gameObject.CompareTag("Player"))
    //    {
    //        Physics.IgnoreCollision(collision.collider, col);
    //    }
    //    else
    //    {
    //        _Inquisitor.isTracking = false;
    //        Destroy(gameObject);
    //    }
    //}

    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log("trigger enter");
        if (other.gameObject.CompareTag("ContactlessProjectile")) return;
        if (other.gameObject.CompareTag("Player")) return;
        if (other.gameObject.CompareTag("Ground")) return;

        try
        {
            IDamageable damageable = other.transform.GetComponent<IDamageable>();
            damageable.TakeDamage(damage * Time.deltaTime);
            if (damageable == null) Despawn();
            return;
        }
        catch
        {
            //Debug.Log("Hit non idamageable");
            Despawn();
            return;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        try
        {
            IDamageable damageable = other.transform.GetComponent<IDamageable>();
            damageable.TakeDamage(damage * Time.deltaTime);
            //Debug.Log($"{other.transform.name}: {damageable.Health}");
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

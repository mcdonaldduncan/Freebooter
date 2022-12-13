using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Attractor : MonoBehaviour
{
    // Objects assigned in the inspector
    [SerializeField] Rigidbody starShip;
    [SerializeField] GameObject limitIndicator;

    // Fields designated in the inspector
    [SerializeField] float density;
    [SerializeField] float maxDistance;
    [SerializeField] float rotationalAmplifier;
    [SerializeField] bool attractAll;
    [SerializeField] bool shouldLaunch;
    [SerializeField] bool limitDistance;
    

    // rb must be public for accesibility but does not need to be visible in inspector
    [System.NonSerialized] public Rigidbody rb;

    // Objects assigned at runtime
    SphereCollider sphereCollider;
    //OptionManager optionManager;

    // Arbitrarily defined gravitational constant
    const float G = .67f;

    // values to determine strength of gravitational attraction
    float radius;
    float volume;
    internal float mass;

    // Cache relevant objects and scripts
    void OnEnable()
    {
        //optionManager = GameObject.Find("OptionManager").GetComponent<OptionManager>();
        rb = gameObject.GetComponent<Rigidbody>();
        sphereCollider = gameObject.GetComponent<SphereCollider>();
        rb.mass = CalculateMass();
    }

    void Start()
    {
        Launch();
        InstantiateLimit();
    }

    void FixedUpdate()
    {
        SimulateAttraction();
        VelocityRotation();
    }

    // Rotate body based on velocity
    void VelocityRotation()
    {
        transform.Rotate(rb.velocity * Time.deltaTime * rotationalAmplifier);
    }

    // Create a visual representation of the limited range of attraction
    void InstantiateLimit()
    {
        if (limitDistance)
        {
            GameObject indicator = Instantiate(limitIndicator);
            indicator.transform.position = transform.position;
            indicator.transform.localScale = new Vector3(maxDistance * 2f, maxDistance * 2f, maxDistance * 2f);
            indicator.transform.SetParent(gameObject.transform);
        }
    }

    // Launch the body if shouldLaunch is selected, this is only used for the sanbox example
    void Launch()
    {
        if (shouldLaunch)
        {
            Vector2 initialForce = InitialVector();
            rb.velocity = initialForce;
            rb.AddForce(initialForce, ForceMode.Impulse);
        }
    }

    // Calculate an initial vector to launch at perpendicular to the central star
    Vector3 InitialVector()
    {
        float scale = Random.Range(-1f, 1f);

        if (scale >= 0)
        {
            scale = GaussianRange(5f, 6f);
        }
        else
        {
            scale = GaussianRange(-6f, -5f);
        }

        // reduce scaling factor for high mass objects
        if (mass > 100f)
        {
            scale *= .2f;
        }
        else if (mass > 10f)
        {
            scale *= .3f;
        }

        Vector3 initialForce = Mathf.Sqrt(mass) * scale * Vector2.Perpendicular(DataManager.instance.star.transform.position - transform.position) / Vector3.Magnitude(DataManager.instance.star.transform.position - transform.position);
        return initialForce;
    }

    // Method for simulating the attraction of this body on the spaceship/projectile and onto all other bodies if option is selected
    void SimulateAttraction()
    {
        if (starShip != null)
        {
            // If gravitational atraction on the starship should be limited only apply within designated distance
            if (limitDistance)
            {
                if (Vector3.Distance(rb.position, starShip.position) < maxDistance)
                    ApplyGravity(starShip);
            }
            else
            {
                ApplyGravity(starShip);
            }
        }

        // Return if the attractor should only attract the spaceship, else attract each body in list
        if (!attractAll)
            return;

        // Loop through all bodies in the Data Manager
        for (int i = 0; i < DataManager.instance.bodies.Length; i++)
        {
            if (DataManager.instance.bodies[i] == null)
                continue;

            if (DataManager.instance.bodies[i].rb != rb)
            {
                ApplyGravity(DataManager.instance.bodies[i].rb);
            }
        }
    }

    // Method to call application of calculated force
    void ApplyGravity(Rigidbody target)
    {
        Vector3 force = Attract(target);
        target.AddForce(force, ForceMode.Force);
    }

    // Calculate gravitational attraction of one body on another
    Vector3 Attract(Rigidbody toAttract)
    {
        // Vector between bodies
        Vector3 force = rb.position - toAttract.position;
        float distance = force.magnitude;
        distance = Mathf.Clamp(distance, .1f, 100f);
        force.Normalize();
        float strength = G * (mass * toAttract.mass) / Mathf.Pow(distance, 2);
        force *= strength;
        return force;
    }

    // Calculate the mass of a planetary body by calculating volume * density
    float CalculateMass()
    {
        radius = sphereCollider.radius * transform.localScale.x / 2f;
        volume = (4f / 3f) * Mathf.PI * Mathf.Pow(radius, 3);
        mass = volume * density;
        return mass;
    }

    // Generate a standard deviation of numbers by taking a random range over two random ranges
    float GaussianRange(float min, float max)
    {
        return Random.Range(Random.Range(min, max), Random.Range(min, max));
    }

    #region Triggers

    // Check trigger collision, destroy on impact with central star or all other objects if selected
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Star"))
            Destroy(gameObject);

        //if (!optionManager.destroyOnImpact)
        //    return;

        if (other.gameObject.transform.localScale.x > gameObject.transform.localScale.x)
            Destroy(gameObject);
    }

    #endregion
}



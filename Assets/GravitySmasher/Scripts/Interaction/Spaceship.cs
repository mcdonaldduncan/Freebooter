using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Utility;
//using Cinemachine;

public class Spaceship : MonoBehaviour
{
    // Material and cinemachine brain assigned in inspector
    [SerializeField] Material launchMaterial;
    //[SerializeField] CinemachineBrain brain;

    // Additional space for screen bounds
    [SerializeField] Vector2 extraBounds;

    // Launch force assigned in inspector
    [SerializeField] float launchForce;
    [SerializeField] float resetDelay;
    [SerializeField] float accelerationMult;

    // Objects assigned at runtime
    Rigidbody rb;
    LineRenderer line;
    UIManager uiManager;
    WaitForSeconds wait;

    // Starting position of the spaceship assigned at runtime, maximum bounds of screen assigned at runtime, maximum bounds not used in current build
    Vector2 startingPosition;
    Vector2 maximumPosition;

    // Boolean values to handle mouse being dragged and input for powerups
    bool mouseDrag;
    bool shouldAccelerate;
    bool shouldSplit;
    bool spawnOrbits;

    private void OnEnable()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
    }

    // Cache relevant objects and assign runtime variables
    void Start()
    {
        uiManager = AssignUIManager();
        maximumPosition = FindWindowLimits() + extraBounds;
        line = gameObject.AddComponent<LineRenderer>();
        wait = new WaitForSeconds(resetDelay);
        line.material = launchMaterial;
        startingPosition = rb.position;
    }

    void Update()
    {
        CheckWindowLimits();
        CheckBrainActivation();
    }

    void FixedUpdate()
    {
        Accelerate();
    }

    #region Powerups    
    // assorted powerups to be implemented

    /*void CheckForAccelerate()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            shouldAccelerate = true;
        }
    }*

    void Split()
    {
        if (shouldSplit)
        {

        }
    }

    void OrbitingChildren()
    {
        if (spawnOrbits)
        {

        }
    }

    void FireMissiles()
    {

    }*/

    void Accelerate()
    {
        if (shouldAccelerate)
        {
            rb.AddForce(rb.velocity * accelerationMult);
            shouldAccelerate = false;
        }
    }

    #endregion Powerups

    // Check if the cinemachine brain should be activated
    void CheckBrainActivation()
    {
        /*if (brain.enabled == true)
            return;

        if (transform.position.x >= startingPosition.x)
        {
            brain.enabled = true;
        }*/
    }

    // Snap object to mouse position after clicking on object, generate a line to indicate anticipated launch
    void DragProjectile()
    {
        /*if (mouseDrag)
        {
            //brain.enabled = false;
            Vector2 mp = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            transform.position = mp;
            Vector3 anticipatedForce = transform.position - (Vector3)startingPosition;
            line.SetPosition(0, transform.position);
            line.SetPosition(1, startingPosition - (Vector2)anticipatedForce * 4f);
            line.endWidth = .01f;
        }*/
    }

    // On mouse click over object set mouseDrag to true and enable line
    private void OnMouseDown()
    {
        mouseDrag = true;
        line.enabled = true;
    }

    // On mouse release set mouseDrag to false, disable line and launch object
    private void OnMouseUp()
    {
        line.enabled = false;
        mouseDrag = false;
        rb.isKinematic = false;
        Launch();
    }

    // Calculate launch vector by taking starting position less the current position
    void Launch()
    {
        Vector2 force = startingPosition - (Vector2)rb.position;
        float distance = force.magnitude;
        distance = Mathf.Clamp(distance, 5f, 20f);
        force.Normalize();
        force *= distance * launchForce;
        rb.AddForce(force, ForceMode.Impulse);
        IncrementProjectilesUsed();
    }

    // Check if the projectile has left the window limits
    void CheckWindowLimits()
    {
        if (transform.position.x > maximumPosition.x || transform.position.x < -maximumPosition.x)
        {
            ResetProjectile();
        }

        if (transform.position.y > maximumPosition.y || transform.position.y < -maximumPosition.y)
        {
            ResetProjectile();
        }
    }

    // Reset the projectile to start position and set it to be ready for launch
    void ResetProjectile()
    {
        rb.isKinematic = true;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        transform.position = startingPosition;
    }
   

    // Reset projectile on collision
    private void OnCollisionEnter(Collision collision)
    {
        ResetProjectile();
        
    }

    // Increment the number of projectiles used and update UI
    void IncrementProjectilesUsed()
    {
        DataManager.instance.projectilesUsed++;
        uiManager.UpdateScore();
    }


    #region unused

    //private void OnTriggerEnter(Collider other)
    //{
    //    if (resetActive)
    //        return;

    //    StartCoroutine(ResetAfterDelay());
    //}

    //IEnumerator ResetAfterDelay()
    //{
    //    resetActive = true;
    //    yield return wait;
    //    ResetProjectile();
    //    resetActive = false;
    //}

    #endregion
}

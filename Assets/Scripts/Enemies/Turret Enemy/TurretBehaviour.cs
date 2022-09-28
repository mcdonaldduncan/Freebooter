using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretBehaviour : MonoBehaviour
{
    [SerializeField] private GameObject target, body, tip;
    [SerializeField] private float speed, range;
    private enum TurretState {LookingForTarget, ShootTarget};
    [SerializeField] private TurretState state;
    [SerializeField] private float delayBeforeFirstShot;

    Vector3 targetDiretion;
    Quaternion rotation;
    
    private float lastShot, ShootRate = .3f;
    bool resetBacktoFindingEnemies;
    

    void Start()
    {
       state = TurretState.LookingForTarget; 
    }
    void Update()
    {
        switch (state) //handles what the turret shhould be doing at cetain states.
        {
            case TurretState.LookingForTarget:
                Aim();
                LookForLineOfSight();
                break;
            case TurretState.ShootTarget:
                Aim();
                Shoot();
                break;
            default:
                break;
        }
    }
    void Aim() //This is pointing the torret towards the player as long as he is in range
    {
        if (Vector3.Distance(this.transform.position, target.transform.position) < range)
        {
            targetDiretion = transform.position - target.transform.position;
           
            rotation = Quaternion.LookRotation(targetDiretion);
            transform.rotation = Quaternion.Lerp(transform.rotation, rotation, speed * Time.deltaTime);
        }
       
    }
    void LookForLineOfSight() //Shoots raycasts at the player and if it hits the player then it has line of sight
    {
        RaycastHit hit;
        Debug.DrawRay(tip.transform.position, -targetDiretion, Color.green);
        Physics.Raycast(tip.transform.position, -targetDiretion, out hit, range);
        if (hit.collider.tag == target.tag)
        {
            Debug.Log("Player Detected");
            StartCoroutine(Delay());
        }
        else if (hit.collider.tag != target.tag)
        {
            Debug.Log("Player NOT Detected");
        }        
    }
    void Shoot() //Shoots at the player
    {
            RaycastHit hit;
            Debug.DrawRay(tip.transform.position, -targetDiretion, Color.red);
            Physics.Raycast(tip.transform.position, -targetDiretion, out hit, range);
        if (hit.collider.tag == target.tag || hit.collider == target)
        {
            if (Time.time > ShootRate+lastShot)
            {

            Debug.Log("Player was shot, dealing damage.");
            lastShot = Time.time;
            }
        }
        else if (hit.collider.tag != target.tag || hit.collider != target) //if shooting at the player and not hitting the player, swap state to looking for target.
        {
            StateLookingForTarget(); 
        }
    }
    void StateLookingForTarget() //swaps state to looking for target
    {
        state = TurretState.LookingForTarget;
        resetBacktoFindingEnemies = !resetBacktoFindingEnemies;
    }
    void StateShootTarget() //swaps state to shooting target
    {
        state = TurretState.ShootTarget;
    }
    IEnumerator Delay() //Creating a delay before the first shot to give the player time to react
    {
        yield return new WaitForSeconds(delayBeforeFirstShot);
        StateShootTarget();
    }
}

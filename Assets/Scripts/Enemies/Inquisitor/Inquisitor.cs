using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Inquisitor : MonoBehaviour, IDamageable
{
    [SerializeField] GameObject follower_GO;
    [SerializeField] public List<FakeOrbit> orbits;
    [SerializeField] float StartingHealth;
    [SerializeField] float cooldown;

    Follower follower;

    public float Health { get; set; }

    public List<Transform> potentialTargets;

    public bool isTracking = false;

    float cooldownProgress;

    private void OnEnable()
    {
        potentialTargets = FindObjectsOfType<FirstPersonController>().Select(item => item.transform).ToList();
    }

    public void TakeDamage(float damageTaken)
    {
        if (orbits.Any())
            return;
        Debug.Log("Inquisitor Damaged");
        Health -= damageTaken;
        CheckForDeath();
    }

    public void CheckForDeath()
    {
        if (Health <= 0)
        {
            Debug.Log("Inquisitor Destroyed");
            follower.Despawn();
            gameObject.SetActive(false);
        }
    }

    void Start()
    {
        Health = StartingHealth;
    }

    void Update()
    {
        SpawnFollower();
    }

    void SpawnFollower()
    {
        if (isTracking)
            return;

        cooldownProgress += Time.deltaTime;

        if (cooldownProgress < cooldown)
            return;

        if (follower == null)
        {
            follower = Instantiate(follower_GO).GetComponent<Follower>();
            follower.Init(potentialTargets[0], this);
        }
        else
        {
            follower.Init(potentialTargets[0], this);
        }
        
        isTracking = true;
        cooldownProgress = 0;
    }
}

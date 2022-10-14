using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Inquisitor : MonoBehaviour, IDamageable
{
    [SerializeField] GameObject follower;
    [SerializeField] public List<FakeOrbit> orbits;
    [SerializeField] float StartingHealth;
    [SerializeField] float cooldown;

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
            this.gameObject.GetComponent<CheckForDrops>().DropOrNot();
            Debug.Log("Inquisitor Destroyed");
            Destroy(gameObject);
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

        Follower temp = Instantiate(follower).GetComponent<Follower>();
        temp.Init(potentialTargets[0], this);
        isTracking = true;
        cooldownProgress = 0;
    }
}

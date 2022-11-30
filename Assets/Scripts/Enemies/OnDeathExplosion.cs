using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnDeathExplosion : MonoBehaviour
{
    [SerializeField] float blinkrate = .3f, lastblink, fallspeed;
    Renderer m;
    bool dead = false, landed = false, explosion = false;
    Rigidbody rb;
    public float ExplosionDuration = 1.5f;

    [SerializeField] private GameObject explosionGO;

    int deathFrames;

    //private void Start()
    //{
    //    rb = transform.parent.GetComponent<Rigidbody>();
        
    //    rb.isKinematic = true;
    //    rb.useGravity = false;
    //}

    private void Update()
    {
        if (dead == true) // you do not need to evaluate as true dead is a boolean so if (dead == true) is the same thing as if (dead)
        {
            blink();
            FallOnDeath();

        }
        if (explosion == true)
        {
            if (ExplosionDuration > 0)
            {
                ExplosionDuration -= Time.deltaTime;
            }
            if (ExplosionDuration < 0)
            {
                stopExplosion();
            }
        }
        
    }

    public void OnDeathVariables()
    {
        m = this.gameObject.GetComponent<Renderer>();
        dead = true;
        fallspeed = 1;
    }  
    
    void blink()
    {
        if (Time.time > blinkrate + lastblink && m.material.color == Color.white)
        {
            m.material.color = Color.red;
            lastblink = Time.time;
        }
        if (Time.time > blinkrate + lastblink && m.material.color == Color.red)
        {
            m.material.color = Color.white;
            lastblink = Time.time;
        }
    }

    void FallOnDeath()
    {
        if (deathFrames == 0)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            Vector3 explosiveForce = new Vector3(Random.Range(-5f, 5f), Random.Range(3f, 7f), Random.Range(-5f, 5f));
            rb.AddForce(explosiveForce, ForceMode.Impulse);
        }
        

        //rb.useGravity = true;
        //rb.isKinematic = false;
        deathFrames++;
        
        //if (landed == false)
        //{
        //    transform.position = new Vector3(this.transform.position.x, this.transform.position.y - (fallspeed * Time.deltaTime), this.transform.position.z);
        //}
    }

    void ExplodeOnImpact()
    {
        explosion = true;
        explosionGO.GetComponent<SphereCollider>().enabled = true;
    }

    void stopExplosion()
    {
        explosionGO.GetComponent<SphereCollider>().enabled = false;
        Destroy(transform.parent.gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!dead) return;
        landed = true;
        ExplodeOnImpact();

    }

    private void OnTriggerEnter(Collider other)
    {
        if (!dead) return;
        landed = true;
        ExplodeOnImpact();
    }

}

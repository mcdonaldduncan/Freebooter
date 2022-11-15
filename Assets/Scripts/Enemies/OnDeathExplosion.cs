using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnDeathExplosion : MonoBehaviour
{
    [SerializeField] float blinkrate = .3f, lastblink, fallspeed;
    Renderer m;
    bool dead = false, landed = false, explosion = false;

    public float ExplosionDuration = 1.5f;

    [SerializeField] private GameObject explosionGO;
 
    private void Update()
    {
        if (dead == true)
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
        if (landed == false)
        {
            transform.position = new Vector3(this.transform.position.x, this.transform.position.y - (fallspeed * Time.deltaTime), this.transform.position.z);
        }
    }

    void ExplodeOnImpact()
    {
        explosion = true;
        explosionGO.GetComponent<SphereCollider>().enabled = true;
    }

    void stopExplosion()
    {
        explosionGO.GetComponent<SphereCollider>().enabled = false;
        Destroy(gameObject.transform.parent.gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
            landed = true;
            ExplodeOnImpact();

    }

    private void OnTriggerEnter(Collider other)
    {
            landed = true;
            ExplodeOnImpact();
    }

}

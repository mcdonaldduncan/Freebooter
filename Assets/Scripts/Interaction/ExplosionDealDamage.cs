using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionDealDamage : MonoBehaviour
{
    public float damage;
    List<IDamageable> AlreadyHitTargets = new List<IDamageable>();
    private void OnTriggerEnter(Collider other)
    {
        if (!AlreadyHitTargets.Contains(other.gameObject.GetComponent<IDamageable>()))
        {
            try
            {
                other.gameObject.GetComponent<IDamageable>().TakeDamage(damage);
                AlreadyHitTargets.Add(other.gameObject.GetComponent<IDamageable>());
            }
            catch (System.Exception)
            {
                //Debug.Log("Not Idamageble");
            }
        }
    }
}

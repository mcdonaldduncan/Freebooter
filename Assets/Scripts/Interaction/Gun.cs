using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Gun : MonoBehaviour
{
    [Header("Gun Parameters")]
    [SerializeField] private float gunDamage = 10f;
    [SerializeField] private float range;
    [SerializeField] private Transform shootFrom;

    private void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Shoot(InputAction.CallbackContext context)
    {
        RaycastHit hitInfo;
        if (Physics.Raycast(shootFrom.transform.position, shootFrom.transform.forward, out hitInfo, range))
        {
            if (hitInfo.transform.name != "Player")
            {
                Debug.Log(hitInfo.transform.name);
                try
                {
                    IDamageable damageableObject = hitInfo.transform.GetComponent<IDamageable>();
                    damageableObject.Damage(gunDamage);
                }
                catch
                {
                    Debug.Log("Not an IDamageable");        
                }
            }
        }
    }
}

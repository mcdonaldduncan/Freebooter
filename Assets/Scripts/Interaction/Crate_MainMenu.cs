using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Crate_MainMenu : MonoBehaviour, IDamageable
{
    [SerializeField]
    private float health;

    public float Health { get { return health; } set { health = value; } }

    public UnityEvent TriggerEventOnDestroy;

    [SerializeField]
    private GameObject myPrefab;

    [SerializeField]
    private Vector3 prefabPosition;

    public void CheckForDeath()
    {
        if (Health <= 0)
        {
            Destroy(gameObject);
        }
        else
        {
            return;
        }
    }

    public void Damage(float damageTaken)
    {
        Health -= damageTaken;
        CheckForDeath();
    }

    private void OnDestroy()
    {
        if (!this.gameObject.scene.isLoaded) return;
        TriggerEventOnDestroy.Invoke();
    }

    public void InstantiatePrefab()
    {
        Instantiate(myPrefab, prefabPosition, Quaternion.identity);
    }
}

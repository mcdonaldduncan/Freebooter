using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class Crate_MainMenu : MonoBehaviour, IDamageable
{
    [SerializeField]
    private float health;

    public float Health { get { return health; } set { health = value; } }

    public GameObject DamageTextPrefab { get; set; }
    public Transform TextSpawnLocation { get; set; }
    public float FontSize { get; set; }
    public bool ShowDamageNumbers { get; set; }
    public TextMeshPro Text { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

    public UnityEvent TriggerEventOnDisable;

    //[SerializeField]
    //private GameObject myPrefab;

    //[SerializeField]
    //private Vector3 gameObjectPosition;

    private RigidbodyConstraints defaultConstraints;
    private Vector3 defaultPosition;

    private void Start()
    {
        if (this.gameObject.GetComponent<Rigidbody>() != null)
        {
            defaultConstraints = this.gameObject.GetComponent<Rigidbody>().constraints;
        }
        defaultPosition = this.gameObject.transform.position;
    }

    public void CheckForDeath()
    {
        if (Health <= 0)
        {
            DisableAllComponents();
            TriggerEventOnDisable.Invoke();
        }
        else
        {
            return;
        }
    }

    public void TakeDamage(float damageTaken, HitBoxType hitbox, Vector3 hitPoint = default(Vector3))
    {
        Health -= damageTaken;
        CheckForDeath();
    }

    private void DisableAllComponents()
    {
        this.gameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
        this.gameObject.GetComponent<MeshRenderer>().enabled = false;
        this.gameObject.GetComponent<MeshCollider>().enabled = false;
    }

    public void ReturnToDefaults()
    {
        Health = 1f;
        this.gameObject.GetComponent<Rigidbody>().constraints = defaultConstraints;
        this.gameObject.transform.position = defaultPosition;
        this.gameObject.GetComponent<MeshRenderer>().enabled = true;
        this.gameObject.GetComponent<MeshCollider>().enabled = true;
    }

    public void LoadScene(string sceneToLoad)
    {
        SceneManager.LoadScene(sceneToLoad);
    }
}

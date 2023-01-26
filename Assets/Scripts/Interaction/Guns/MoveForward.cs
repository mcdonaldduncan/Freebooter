using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveForward : MonoBehaviour
{
    public float speed = 15;
    public Quaternion origin;
    public Vector3 target;
    Vector3 targetDiretion;
    //public float damage;
    // Start is called before the first frame update
    void Start()
    {
        try
        {
            targetDiretion = target - transform.position;
            transform.rotation = origin;
            
        }
        catch (System.Exception)
        {
        }
       //Destroy(this.gameObject, 4);
    }

    // Update is called once per frame
    void Update()
    {
        this.transform.position += targetDiretion * speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.name.Contains(gameObject.name))
        {
            Destroy(this.gameObject, 0.2f);
        }
    }

}

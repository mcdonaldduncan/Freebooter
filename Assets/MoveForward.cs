using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveForward : MonoBehaviour
{
    public float speed = 15;
    public Quaternion origin;
    public GameObject target;
    Vector3 targetDiretion;
    //public float damage;
    // Start is called before the first frame update
    void Start()
    {
        try
        {
            targetDiretion = target.transform.position - transform.position;
            transform.rotation = origin;
            Destroy(this, 4);
        }
        catch (System.Exception)
        {
        }
    }

    // Update is called once per frame
    void Update()
    {
        this.transform.position += targetDiretion * speed * Time.deltaTime;
    }
    
}

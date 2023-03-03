using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class PopUpBehaviour : MonoBehaviour, IPoolable
{
    public GameObject Prefab { get;  set; }
    public float speed, popupDuration,fontScaleSpeed;

    private void Start()
    {
        Destroy(gameObject, popupDuration);
    }
    // Update is called once per frame
    void Update()
    {
        gameObject.transform.position += Vector3.up * Time.deltaTime * speed;
        transform.rotation = Camera.main.transform.rotation;
        this.gameObject.GetComponent<TextMeshPro>().fontSize += Time.deltaTime * fontScaleSpeed;
    }
}

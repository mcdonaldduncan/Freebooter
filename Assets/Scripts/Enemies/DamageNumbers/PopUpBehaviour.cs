using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class PopUpBehaviour : MonoBehaviour, IPoolable
{
    public GameObject Prefab { get;  set; }
    public float speed, popupDuration,fontScaleSpeed;
    private TextMeshPro m_TextMeshPro;

    private void Start()
    {
        ProjectileManager.Instance.ReturnToPool(this.gameObject);
        m_TextMeshPro = gameObject.GetComponent<TextMeshPro>();
    }

    // Update is called once per frame
    void Update()
    {
        gameObject.transform.position += Vector3.up * Time.deltaTime * speed;
        transform.rotation = Camera.main.transform.rotation;
        m_TextMeshPro.fontSize += Time.deltaTime * fontScaleSpeed;
    }
}

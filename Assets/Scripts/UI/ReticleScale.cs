using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;

public class ReticleScale : MonoBehaviour
{
    private RectTransform Reticle;

    CanvasRenderer reticleRenderer;

    private CanvasGroup reticleCanvas;

    [Range(100f, 150)]

    public float size; 

    void Start()
    {
        Reticle = GetComponent<RectTransform>();
        reticleRenderer = GetComponent<CanvasRenderer>();
        reticleCanvas = GetComponent<CanvasGroup>();

        reticleCanvas.alpha = 1f;
        
    }

    private void Update()
    {
        Reticle.sizeDelta = new Vector2(size, size);
    }
}

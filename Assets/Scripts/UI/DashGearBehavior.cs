using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DashGearBehavior : MonoBehaviour
{
    public bool IsSpinning { get { return isSpinning; } }

    [SerializeField] private float rotationSpeed;
    [SerializeField] private Image blueCore;

    private bool isSpinning;
    private float spinTime;
    private float spinStartTime;
    private RectTransform gearTransform;
    private FirstPersonController player;
    private float coreFillTime;

    private void Start()
    {
        gearTransform = GetComponent<RectTransform>();
        player = LevelManager.Instance.Player;
        //blueCore = GetComponentInChildren<Image>();
        spinTime = player.DashCooldownTime;
    }

    private void Update()
    {
        if (isSpinning && player.DashShouldCooldown)
        {
            gearTransform.Rotate(Vector3.back * Time.deltaTime * rotationSpeed);
            coreFillTime += Time.deltaTime;
            blueCore.fillAmount = Mathf.Lerp(0, 1, coreFillTime / spinTime);
            if (spinStartTime + spinTime < Time.time)
            {
                isSpinning = false;
                UpdateDash.DashCooldownCompleted?.Invoke();
            }
        }
    }

    public void SpinGear()
    {
        isSpinning = true;
        spinStartTime = Time.time;
        coreFillTime = 0;

        //if (spinStartTime + spinTime < Time.time)
        //{
        //    SpinGear();
        //}
        //else
        //{
        //    isSpinning = false;
        //    return;
        //}
    }

    public void StopGear()
    {
        isSpinning = false;
    }
}

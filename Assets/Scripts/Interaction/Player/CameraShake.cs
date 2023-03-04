using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    [Header("Shake Settings")]
    //private static float shakeDuration = 0.5f; // The duration of the shake effect
    private static float shakeMagnitude = 0.1f; // The magnitude of the shake effect
    private static float dampingSpeed = 1.0f; // The speed at which the shake effect dampens over time

    private Vector3 initialPosition; // The initial position of the camera before shaking
    private static float shakeTimer = 0.0f; // The current timer for the shake effect

    private void Start()
    {
        initialPosition = transform.localPosition;
    }

    private void Update()
    {
        if (shakeTimer > 0)
        {
            // Move the camera randomly within a certain range
            transform.localPosition = initialPosition + Random.insideUnitSphere * shakeMagnitude;

            // Dampen the shake effect over time
            shakeTimer -= Time.deltaTime * dampingSpeed;
        }
        else
        {
            // Reset the camera to its initial position
            shakeTimer = 0.0f;
            transform.localPosition = initialPosition;
        }
    }

    public static void ShakeCamera(float duration, float magnitude, float damping)
    {
        // Trigger the shake effect by setting the shake timer
        shakeTimer = duration;
        shakeMagnitude = magnitude;
        dampingSpeed = damping;
    }
}

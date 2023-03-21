using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleTravelToPlayer : MonoBehaviour
{
    GameObject Player;

    private ParticleSystem _particleSystem;
    private ParticleSystem.Particle[] _particles = new ParticleSystem.Particle[1000];
    // Start is called before the first frame update
    void Start()
    {
        Player = LevelManager.Instance.Player.gameObject;
        _particleSystem = GetComponent<ParticleSystem>();
    }

    public void LateUpdate()
    {
       
        int length = _particleSystem.GetParticles(_particles);
        Vector3 attractorPosition = Player.transform.position;
        for (int i = 0; i < length; i++)
        {
            _particles[i].position = _particles[i].position + (attractorPosition - _particles[i].position) / (_particles[i].remainingLifetime) * Time.deltaTime;
        }
        _particleSystem.SetParticles(_particles, length);

    }
}

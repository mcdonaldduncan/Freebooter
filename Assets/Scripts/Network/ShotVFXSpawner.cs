using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class ShotVFXSpawner : Singleton<ShotVFXSpawner>
{
    [SerializeField] private TrailRenderer m_Trail;
    [SerializeField] private float m_Speed;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Shoot(Vector3 shootFrom, Vector3 targetPos, Vector3 aimSpot)
    {
        TrailRenderer trail = Instantiate(m_Trail, shootFrom, Quaternion.identity);
        StartCoroutine(SpawnTrail(trail, targetPos, aimSpot));
    }

    private IEnumerator SpawnTrail(TrailRenderer trail, Vector3 endPoint, Vector3 aimSpot)
    {
        float time = 0;

        trail.transform.LookAt(aimSpot);

        Vector3 startPosition = trail.transform.position;
        while (trail.transform.position != endPoint)
        {
            trail.transform.position = Vector3.MoveTowards(trail.transform.position, endPoint, m_Speed * Time.deltaTime);   // Vector3.Lerp(startPosition, endPoint, time);
            time += Time.deltaTime / trail.time;

            yield return null;
        }

        trail.transform.position = endPoint;

        Destroy(trail);
    }
}

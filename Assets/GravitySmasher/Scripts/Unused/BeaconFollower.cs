using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Summary
 * This script is unused, it was made during class when you first mentioned the idea of a beacon follower
 * Will remove before final build
 */


public class BeaconFollower : MonoBehaviour
{
    [SerializeField] List<GameObject> beacons;

    float maxSpeed = 10f;

    int index = 0;

    // Start is called before the first frame update
    void Start()
    {
        index = FindClosestIndex();
    }

    // Update is called once per frame
    void Update()
    {
        FollowBeacons();
    }

    int FindClosestIndex()
    {
        float shortest = 1000f;
        int temp = 0;
        for (int i = 0; i < beacons.Count; i++)
        {
            if (Vector3.Distance(transform.position, beacons[i].transform.position) < shortest)
            {
                shortest = Vector3.Distance(transform.position, beacons[i].transform.position);
                temp = i;
            }
        }
        return temp;
    }

    void FollowBeacons()
    {
        float step = maxSpeed * Time.deltaTime;

        transform.position = Vector3.MoveTowards(transform.position, beacons[index].transform.position, step);

        if (Vector3.Distance(transform.position, beacons[index].transform.position) < .5f)
        {
            beacons.Remove(beacons[index]);
            index = FindClosestIndex();
        }
    }
}

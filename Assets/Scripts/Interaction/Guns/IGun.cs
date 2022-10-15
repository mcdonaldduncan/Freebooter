using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IGun
{
    IEnumerator Reload(GunHandler instance, WaitForSeconds reloadWait);
    //IEnumerator SpawnTrail(TrailRenderer trail, RaycastHit hit);
}

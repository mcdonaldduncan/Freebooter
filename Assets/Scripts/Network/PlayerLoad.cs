using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
public class PlayerLoad : NetworkBehaviour
{
    [SerializeField] GameObject playerPrefab;
    [SerializeField] NetworkObject avatar;
    [SerializeField] Vector3 startPosition;

    NetworkObject player;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
            return;
        if (!IsClient)
            return;

        player = Instantiate(Resources.Load<NetworkObject>("NetFirstPersonController"), startPosition, Quaternion.identity);
        SetParentServerRPC();
    }

    [ServerRpc]
    public void SetParentServerRPC()
    {
        this.gameObject.transform.SetParent(player.transform, false);
    }


}

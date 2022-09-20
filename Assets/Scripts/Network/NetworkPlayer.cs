using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkPlayer : NetworkBehaviour
{
    public NetworkVariable<Vector3> Position = new NetworkVariable<Vector3>();

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            InitializePos();
        }
    }

    public void InitializePos()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            var randomPosition = GetRandomPositionOnPlane();
            transform.position = randomPosition;
            Position.Value = randomPosition;
        }
        else
        {
            SubmitPositionRequestServerRpc();
        }
    }

    public void Move(Vector3 value)
    {
        if (NetworkManager.Singleton.IsServer)
        {
            Position.Value = Position.Value + value;
        }
        else
        {
            SubmitMoveRequestServerRpc(value);
        }
    }

    [ServerRpc]
    void SubmitPositionRequestServerRpc(ServerRpcParams rpcParams = default)
    {
        Position.Value = GetRandomPositionOnPlane();
    }

    [ServerRpc]
    void SubmitMoveRequestServerRpc(Vector3 value, ServerRpcParams rpcParams = default)
    {
        Position.Value = Position.Value + value;
    }

    static Vector3 GetRandomPositionOnPlane()
    {
        return new Vector3(Random.Range(-3f, 3f), 0, Random.Range(-3f, 3f));
    }

    void Update()
    {
        transform.position = Position.Value;
    }
}

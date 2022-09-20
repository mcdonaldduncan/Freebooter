using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class HelloWorldManager : MonoBehaviour
{
    Vector3 moveValue;
    
    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 300));
        if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
        {
            StartButtons();
        }
        else
        {
            StatusLabels();
            SubmitNewPosition();
            
        }

        GUILayout.EndArea();
    }

    static void StartButtons()
    {
        if (GUILayout.Button("Host")) NetworkManager.Singleton.StartHost();
        if (GUILayout.Button("Client")) NetworkManager.Singleton.StartClient();
        if (GUILayout.Button("Server")) NetworkManager.Singleton.StartServer();
    }

    static void StatusLabels()
    {
        var mode = NetworkManager.Singleton.IsHost ?
            "Host" : NetworkManager.Singleton.IsServer ? "Server" : "Client";

        GUILayout.Label("Transport: " +
            NetworkManager.Singleton.NetworkConfig.NetworkTransport.GetType().Name);
        GUILayout.Label("Mode: " + mode);
    }

    static void SubmitNewPosition()
    {
        if (GUILayout.Button(NetworkManager.Singleton.IsServer ? "Move" : "Request Position Change"))
        {
            if (NetworkManager.Singleton.IsServer && !NetworkManager.Singleton.IsClient)
            {
                foreach (ulong uid in NetworkManager.Singleton.ConnectedClientsIds)
                    NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(uid).GetComponent<NetworkPlayer>().InitializePos();
            }
            else
            {
                var playerObject = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
                var player = playerObject.GetComponent<NetworkPlayer>();
                player.InitializePos();
            }
        }
    }
    
    static void SubmitNewMove(Vector3 value)
    {
        if (NetworkManager.Singleton.IsServer && !NetworkManager.Singleton.IsClient)
        {
            foreach (ulong uid in NetworkManager.Singleton.ConnectedClientsIds)
                NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(uid).GetComponent<NetworkPlayer>().Move(value);
        }
        else
        {
            var playerObject = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
            var player = playerObject.GetComponent<NetworkPlayer>();
            player.Move(value);
        }
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.W))
        {
            moveValue = Vector3.forward;
            SubmitNewMove(moveValue);
        }
        if (Input.GetKey(KeyCode.S))
        {
            moveValue = Vector3.back;
            SubmitNewMove(moveValue);
        }
        if (Input.GetKey(KeyCode.A))
        {
            moveValue = Vector3.left;
            SubmitNewMove(moveValue);
        }
        if (Input.GetKey(KeyCode.D))
        {
            moveValue = Vector3.right;
            SubmitNewMove(moveValue);
        }
    }
}

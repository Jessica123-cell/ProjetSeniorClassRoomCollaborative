using Unity.Netcode;
using UnityEngine;

public class PlayerSpawner : NetworkBehaviour
{
    public GameObject avatarPrefab;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    }

    private void OnClientConnected(ulong clientId)
    {
        // NE PAS spawn d'avatar pour le host PC
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            Debug.Log("Host détecté → aucun avatar généré.");
            return;
        }

        Debug.Log($"Client {clientId} connecté → en attente de sa position XR…");
    }

    // *** Nouvelle syntaxe RPC (Netcode 1.8+) ***
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void RequestSpawnAvatarRpc(Vector3 pos, Quaternion rot, ulong clientId)
    {
        Debug.Log($"Spawn avatar pour le client {clientId} à {pos}");

        GameObject avatar = Instantiate(avatarPrefab, pos, rot);
        avatar.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
    }
}

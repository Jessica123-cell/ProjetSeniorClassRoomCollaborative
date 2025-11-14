using Unity.Netcode;
using UnityEngine;

public class PlayerSpawner : NetworkBehaviour
{
    public GameObject avatarPrefab;
    public Transform playerSpawnPoint; // on Assigne dans l'inspecteur

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        Debug.Log("PlayerSpawner prêt sur le serveur");
    }

    private void OnClientConnected(ulong clientId)
    {
        // Ne pas spawn d'avatar pour le Host PC
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            Debug.Log("Host détecté donc aucun avatar généré.");
            return;
        }

        Debug.Log($"Participant {clientId} connecté.");

        // --- SPAWN DU JOUEUR ---
        GameObject avatar = Instantiate(
            avatarPrefab,
            playerSpawnPoint.position,
            playerSpawnPoint.rotation
        );

        avatar.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
    }
}

//using Unity.Netcode;
//using UnityEngine;

//public class PlayerSpawner : NetworkBehaviour
//{
//    public GameObject avatarPrefab;
//    public Transform playerSpawnPoint; // on Assigne dans l'inspecteur

//    public override void OnNetworkSpawn()
//    {
//        if (!IsServer) return;

//        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
//        Debug.Log("PlayerSpawner prêt sur le serveur");
//    }

//    private void OnClientConnected(ulong clientId)
//    {
//        // Ne pas spawn d'avatar pour le Host PC
//        if (clientId == NetworkManager.Singleton.LocalClientId)
//        {
//            Debug.Log("Host détecté donc aucun avatar généré.");
//            return;
//        }

//        Debug.Log($"Participant {clientId} connecté.");

//        // --- SPAWN DU JOUEUR ---
//        GameObject avatar = Instantiate(
//            avatarPrefab,
//            playerSpawnPoint.position,
//            playerSpawnPoint.rotation
//        );

//        avatar.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
//    }
//}
using Unity.Netcode;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Management;  // Pour détecter si un casque VR est actif

public class PlayerSpawner : NetworkBehaviour
{
    public GameObject avatarPrefab;
    public Transform playerSpawnPoint;

    // --- Détection simple : vrai si un casque VR (Meta Link / Quest Link / Air Link) est actif ---
    private bool IsHeadsetActive()
    {
        bool viaXRManagement = false;
        var xrSettings = XRGeneralSettings.Instance;
        if (xrSettings != null && xrSettings.Manager != null)
        {
            viaXRManagement = xrSettings.Manager.activeLoader != null;
        }

        bool viaLegacyXR = XRSettings.isDeviceActive;

        bool detected = viaXRManagement || viaLegacyXR;

        Debug.Log($"[PlayerSpawner] Détection casque → XRManagement={viaXRManagement}, LegacyXR={viaLegacyXR}, Final={detected}");
        return detected;
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        Debug.Log("PlayerSpawner prêt sur le serveur");
    }

    private void OnClientConnected(ulong clientId)
    {
        // --- CAS DU HOST ---
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            bool headsetDetected = IsHeadsetActive();

            if (!headsetDetected)
            {
                Debug.Log("Host détecté → aucun avatar (pas de casque connecté via Meta Link).");
                return;
            }
            else
            {
                Debug.Log("Host avec casque VR détecté → avatar généré.");
            }
        }
        else
        {
            Debug.Log($"Participant {clientId} connecté.");
        }
        var client = NetworkManager.Singleton.ConnectedClients[clientId];

        if (client.PlayerObject != null)
        {
            Debug.Log("[PlayerSpawner] Le client a déjà un PlayerObject → skip");
            return;
        }
        // --- SPAWN AVATAR ---
        GameObject avatar = Instantiate(
            avatarPrefab,
            playerSpawnPoint.position,
            playerSpawnPoint.rotation
        );

        avatar.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
    }
}


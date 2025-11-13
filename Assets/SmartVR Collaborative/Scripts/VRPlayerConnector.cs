using Unity.Netcode;
using UnityEngine;

public class VRPlayerConnector : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        Debug.Log("VRPlayerConnector Spawn -- IsOwner=" + IsOwner + " / IsServer=" + IsServer);

        if (!IsOwner || IsServer)
        {
            Debug.Log("VRPlayerConnector ignoré (pas owner ou c'est le serveur).");
            return;
        }

        Transform xrOrigin = transform;

        Debug.Log("Position XR détectée : " + xrOrigin.position);

        PlayerSpawner spawner = Object.FindFirstObjectByType<PlayerSpawner>();

        if (spawner == null)
        {
            Debug.LogError("VRPlayerConnector : PlayerSpawner introuvable dans la scène !");
            return;
        }

        Debug.Log("Envoi du RPC avec la position XR…");

        spawner.RequestSpawnAvatarRpc(
            xrOrigin.position,
            xrOrigin.rotation,
            OwnerClientId
        );
    }
}

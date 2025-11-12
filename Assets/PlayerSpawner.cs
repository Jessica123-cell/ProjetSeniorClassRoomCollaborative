using Unity.Netcode;
using UnityEngine;

public class PlayerSpawner : NetworkBehaviour
{
    [Header("Prefab de l'avatar joueur")]
    public GameObject avatarPrefab;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            //  D'abord on spawn le host (serveur local)
            SpawnPlayer(OwnerClientId);

            //  Puis on écoute les prochaines connexions (clients)
            NetworkManager.Singleton.OnClientConnectedCallback += SpawnPlayer;
        }
    }

    private void SpawnPlayer(ulong clientId)
    {
        GameObject avatar = Instantiate(avatarPrefab);
        avatar.transform.position = new Vector3(Random.Range(-2f, 2f), 1f, Random.Range(-2f, 2f));

        var netObj = avatar.GetComponent<NetworkObject>();
        netObj.SpawnAsPlayerObject(clientId);

        Debug.Log($" Joueur {clientId} spawné à {avatar.transform.position}");
    }
}

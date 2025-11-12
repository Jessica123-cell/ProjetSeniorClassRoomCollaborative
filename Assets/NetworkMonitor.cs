using UnityEngine;
using Unity.Netcode;

public class NetworkMonitor : MonoBehaviour
{
    private void OnEnable()
    {
        // Ces événements existent à la fois pour le host et les clients
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
    }

    private void OnDisable()
    {
        if (NetworkManager.Singleton == null) return;
        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
    }

    private void OnClientConnected(ulong clientId)
    {
        int count = NetworkManager.Singleton.ConnectedClientsList.Count;
        Debug.Log($"Client connecté : ID={clientId} | Total joueurs = {count}");

        // Affiche un message spécial si c’est un autre joueur (pas le host)
        if (NetworkManager.Singleton.IsServer)
        {
            if (clientId != NetworkManager.Singleton.LocalClientId)
                Debug.Log($" Un nouveau joueur vient de rejoindre la session !");
        }
    }

    private void OnClientDisconnected(ulong clientId)
    {
        int count = NetworkManager.Singleton.ConnectedClientsList.Count;
        Debug.Log($"Client déconnecté : ID={clientId} | Restants = {count}");
    }
}

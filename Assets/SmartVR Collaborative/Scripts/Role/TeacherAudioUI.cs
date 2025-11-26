using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;
using System.Collections;
using System.Collections.Generic;

public class TeacherAudioUI : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject playerEntryPrefab;
    public Transform playerListContainer;

    private Dictionary<ulong, AudioRoleController> players = new();
    private bool isVisible = false;

    private void Awake()
    {
        // IMPORTANT : le GameObject reste actif pour que Start soit appelé
        gameObject.SetActive(true);
        // On le "cache" en mettant l'échelle à 0
        transform.localScale = Vector3.zero;
    }

    private void Start()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;

        Debug.Log("[TeacherUI] Initialized");
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton == null) return;

        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
    }

    private void OnClientConnected(ulong clientId)
    {
        if (isVisible)
            StartCoroutine(DelayedRefresh());
    }

    private void OnClientDisconnected(ulong clientId)
    {
        if (isVisible)
            StartCoroutine(DelayedRefresh());
    }

    private IEnumerator DelayedRefresh()
    {
        // Laisse Netcode finir de spawner les PlayerObject
        yield return null;
        yield return null;

        DoRefreshList();
    }

    private void DoRefreshList()
    {
        // Clear anciens éléments
        foreach (Transform child in playerListContainer)
            Destroy(child.gameObject);

        players.Clear();

        foreach (var kvp in NetworkManager.Singleton.ConnectedClients)
        {
            ulong clientId = kvp.Key;
            var client = kvp.Value;

            if (client.PlayerObject == null)
            {
                Debug.LogWarning($"[TeacherUI] Player {clientId} n’a pas encore de PlayerObject.");
                continue;
            }

            var controller = client.PlayerObject.GetComponent<AudioRoleController>();
            if (controller == null)
            {
                Debug.LogWarning($"[TeacherUI] Player {clientId} n’a PAS AudioRoleController.");
                continue;
            }

            players[clientId] = controller;
            CreatePlayerRow(clientId, controller);
        }

        Debug.Log("[TeacherUI] List refreshed");
    }

    private void CreatePlayerRow(ulong clientId, AudioRoleController controller)
    {
        GameObject row = Instantiate(playerEntryPrefab, playerListContainer);

        var ui = row.GetComponent<PlayerEntryUI>();
        if (ui == null)
        {
            Debug.LogError("[TeacherUI] PlayerEntryUI manquant sur le prefab PlayerEntryPrefab !");
            return;
        }

        // Nom du joueur
        ui.nameText.text = "Player " + clientId;

        // MUTER
        ui.muteButton.onClick.AddListener(() =>
        {
            AudioRoleController.Local?.TeacherMutePlayer(clientId, true);
        });

        // DÉMUTER
        ui.unmuteButton.onClick.AddListener(() =>
        {
            AudioRoleController.Local?.TeacherMutePlayer(clientId, false);
        });
    }

    public void ShowIfTeacher(bool show)
    {
        isVisible = show;

        if (show)
        {
            transform.localScale = new Vector3(0.002f, 0.002f, 0.002f);
            StartCoroutine(DelayedRefresh());     // met à jour la liste
        }
        else
        {
            transform.localScale = Vector3.zero;  // cache le panneau mais garde le script actif
        }
    }
}

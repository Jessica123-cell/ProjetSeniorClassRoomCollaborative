using System;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.Networking;

public class MultiplayerManager : MonoBehaviour
{
    [Header("Configuration")]
    public string sessionName = "SalleDeClasse";
    public int maxPlayers = 10;
    public bool autoStart = true;

    private ISession currentSession;

    private async void Start()
    {
        Debug.Log(" Vérification du réseau avant initialisation...");

        // ✅ Vérifie la connectivité Internet avant tout
        if (!await WaitForInternet())
        {
            Debug.LogError(" Aucun accès Internet détecté. Impossible d'initialiser Unity Services.");
            return;
        }

        await InitializeUGS();

        if (autoStart)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            Debug.Log(" Mode CLIENT (Quest) - Connexion automatique...");
            await StartClientAsync();
#else
            Debug.Log("Mode HOST (PC) - Création automatique de session...");
            await StartHostAsync();
#endif
        }
    }

    // ✅ Vérifie que le casque a Internet avant de lancer Unity Services
    private async Task<bool> WaitForInternet()
    {
        for (int i = 0; i < 5; i++)
        {
            using (var req = UnityWebRequest.Head("https://www.google.com"))
            {
                var op = req.SendWebRequest();
                await op;

                if (req.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log(" Internet disponible, initialisation possible.");
                    return true;
                }
                else
                {
                    Debug.LogWarning($" Tentative {i + 1}/5 : Internet indisponible, nouvel essai...");
                    await Task.Delay(2000);
                }
            }
        }
        return false;
    }

    // ✅ Initialise les services Unity
    private async Task InitializeUGS()
    {
        Debug.Log("Initialisation Unity Services...");

        try
        {
            if (UnityServices.State == ServicesInitializationState.Uninitialized)
                await UnityServices.InitializeAsync();

            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                Debug.Log($"Authentifié : {AuthenticationService.Instance.PlayerId}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Erreur d'initialisation Unity Services : {e.GetType().Name} - {e.Message}");
        }
    }

    // ✅ Mode HOST
    private async Task StartHostAsync()
    {
        try
        {
            Debug.Log("Création d'une session publique...");

            var options = new SessionOptions
            {
                Name = sessionName,
                MaxPlayers = maxPlayers,
                IsPrivate = false
            }.WithRelayNetwork();

            IHostSession hostSession = await MultiplayerService.Instance.CreateSessionAsync(options);
            currentSession = hostSession;

            Debug.Log($" Session créée : {hostSession.Id}");
            Debug.Log($"Code de session : {hostSession.Code}");
            Debug.Log($" Nom : {hostSession.Name}");
            Debug.Log($"Partcipants max : {hostSession.MaxPlayers}");

            NetworkManager.Singleton.StartHost();
            Debug.Log("HOST LANCÉ ! En attente de partcipants...");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Erreur création session : {e.GetType().Name} - {e.Message}");
        }
    }

    // ✅ Mode CLIENT
    private async Task StartClientAsync()
    {
        try
        {
            Debug.Log("Recherche de sessions publiques...");

            await Task.Delay(2000);

            var queryOptions = new QuerySessionsOptions
            {
                Count = 10
            };

            QuerySessionsResults results = await MultiplayerService.Instance.QuerySessionsAsync(queryOptions);

            if (results.Sessions == null || results.Sessions.Count == 0)
            {
                Debug.LogError("Aucune session trouvée ! Réessai dans 3 secondes...");
                await Task.Delay(3000);
                await StartClientAsync();
                return;
            }

            ISessionInfo targetSession = null;
            foreach (var session in results.Sessions)
            {
                if (session.Name == sessionName)
                {
                    targetSession = session;
                    break;
                }
            }

            if (targetSession == null)
            {
                Debug.LogError($" Aucune session nommée '{sessionName}' trouvée ! Réessai...");
                await Task.Delay(3000);
                await StartClientAsync();
                return;
            }

            Debug.Log($"Session trouvée : {targetSession.Name}");
            Debug.Log($"ID : {targetSession.Id}");
            Debug.Log($"Places disponibles : {targetSession.AvailableSlots}");

            currentSession = await MultiplayerService.Instance.JoinSessionByIdAsync(targetSession.Id);
            Debug.Log($"Session rejointe : {currentSession.Id}");

            NetworkManager.Singleton.StartClient();
            Debug.Log("PARTICIPANT CONNECTÉ !");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Erreur connexion client : {e.GetType().Name} - {e.Message}");
            await Task.Delay(3000);
            await StartClientAsync();
        }
    }

    // ✅ Nettoyage propre
    private async void OnDestroy()
    {
        try
        {
            if (currentSession != null)
            {
                // Vérifie que le service n’est pas déjà en train d’être détruit
                if (UnityServices.State == ServicesInitializationState.Initialized)
                {
                    await currentSession.LeaveAsync();
                    Debug.Log(" Session quittée proprement");
                }
                else
                {
                    Debug.Log(" Services Unity déjà arrêtés, skip LeaveAsync()");
                }
            }
        }
        catch (ObjectDisposedException)
        {
            Debug.LogWarning("Tentative de quitter une session déjà fermée (ignorée)");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Erreur en quittant : {e.Message}");
        }
    }

}

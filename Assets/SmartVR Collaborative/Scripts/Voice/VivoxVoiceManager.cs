using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Vivox;
using UnityEngine;

public class VivoxVoiceManager : MonoBehaviour
{
    public static VivoxVoiceManager Instance;

    [Header("Vivox")]
    public string channelName = "SalleDeClasse";

    [Header("Positional audio")]
    public int audibleDistance = 25;
    public int conversationalDistance = 2;
    public float fadeIntensity = 1f;

    private bool vivoxInitialized;
    private bool joinedChannel;

    public string LocalDisplayName { get; private set; }

    private async void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        await WaitForUGSReady();
        await WaitForLocalAvatarOrTimeout();
        await InitializeVivoxOnly();
    }

    // 1) Attendre que UGS soit prêt (Authentication OK)
    private async Task WaitForUGSReady()
    {
        while (UnityServices.State != ServicesInitializationState.Initialized ||
               !AuthenticationService.Instance.IsSignedIn)
        {
            await Task.Delay(200);
        }

        Debug.Log("[Vivox] UGS prêt, user signé.");
    }

    // 2) Attendre un éventuel avatar local (pour les casques XR)
    private async Task WaitForLocalAvatarOrTimeout()
    {
        float timer = 0f;
        const float timeout = 2.0f;

        while (timer < timeout)
        {
            var avatars = FindObjectsByType<PlayerAvatar>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);

            foreach (var a in avatars)
            {
                if (a.IsOwner)
                {
                    Debug.Log("[Vivox] Avatar local trouvé, on peut démarrer Vivox.");
                    return;
                }
            }

            await Task.Delay(200);
            timer += 0.2f;
        }

        Debug.Log("[Vivox] Aucun avatar local trouvé (PC sans XR ?) → on continue quand même.");
    }

    // 3) Initialisation Vivox seule
    private async Task InitializeVivoxOnly()
    {
        if (!vivoxInitialized)
        {
            await VivoxService.Instance.InitializeAsync();
            vivoxInitialized = true;
            Debug.Log("[Vivox] Service initialisé.");
        }

        // S’abonner aux events
        VivoxService.Instance.ParticipantAddedToChannel += OnParticipantAdded;
        VivoxService.Instance.ParticipantRemovedFromChannel += OnParticipantRemoved;

        await LoginVivox();
        await JoinChannel();

        // Activation MIC pour être entendu
        VivoxService.Instance.UnmuteInputDevice();
        VivoxService.Instance.SetInputDeviceVolume(100);   // 0–100

        // Activation OUTPUT pour entendre les autres
        VivoxService.Instance.UnmuteOutputDevice();
        VivoxService.Instance.SetOutputDeviceVolume(100);  // 0–100

        Debug.Log("[Vivox] Input & Output activés, volumes = 100%.");
    }

    // 4) Login Vivox
    private async Task LoginVivox()
    {
        // On utilise le PlayerId UGS comme identifiant/diplayName
        LocalDisplayName = AuthenticationService.Instance.PlayerId;

        var options = new LoginOptions { DisplayName = LocalDisplayName };
        await VivoxService.Instance.LoginAsync(options);

        Debug.Log("[Vivox] Login OK : " + LocalDisplayName);
    }

    // 5) Joindre le canal 3D
    private async Task JoinChannel()
    {
        if (joinedChannel) return;

        var props = new Channel3DProperties(
            audibleDistance,
            conversationalDistance,
            fadeIntensity,
            AudioFadeModel.InverseByDistance);

        await VivoxService.Instance.JoinPositionalChannelAsync(
            channelName,
            ChatCapability.AudioOnly,
            props);

        await VivoxService.Instance.SetChannelTransmissionModeAsync(
            TransmissionMode.Single,
            channelName);

        joinedChannel = true;
        Debug.Log("[Vivox] Canal rejoint : " + channelName);
    }

    // 6) Participant ajouté dans un canal
    private void OnParticipantAdded(VivoxParticipant participant)
    {
        Debug.Log($"[Vivox] Participant joint : {participant.DisplayName} (IsSelf={participant.IsSelf})");

        // IMPORTANT : ne jamais créer de TAP audio pour le local → évite
        // que sa propre voix soit routée dans un AudioSource Unity.
        if (participant.IsSelf)
        {
            // Si Vivox a créé un TAP par défaut, on le détruit.
            participant.DestroyVivoxParticipantTap();
            Debug.Log("[Vivox] Local participant → TAP détruit (pas de retour sur soi).");
            return;
        }

        // PARTICIPANT DISTANT → on crée son TAP audio
        GameObject audioRoot = GameObject.Find("VivoxAudioRoot");
        if (audioRoot == null)
        {
            audioRoot = new GameObject("VivoxAudioRoot");
            DontDestroyOnLoad(audioRoot);
        }

        var voice = audioRoot.AddComponent<VoicePlayerAudio>();
        voice.InitializeForParticipant(participant, audioRoot.transform);

        Debug.Log("[Vivox] TAP audio attaché à VivoxAudioRoot pour " + participant.DisplayName);
    }

    // 7) Participant retiré
    private void OnParticipantRemoved(VivoxParticipant participant)
    {
        Debug.Log($"[Vivox] Participant quitté : {participant.DisplayName} (IsSelf={participant.IsSelf})");

        // Dans tous les cas on supprime le TAP associé
        participant.DestroyVivoxParticipantTap();
    }

    // 8) Mise à jour position 3D du local (pour le spatial 3D Vivox)
    // Appeler ça depuis ton avatar local avec la tête / centre du joueur.
    public void UpdateLocal3DPosition(GameObject listenerObject)
    {
        if (!joinedChannel || listenerObject == null)
            return;

        VivoxService.Instance.Set3DPosition(listenerObject, channelName, true);
    }
}



//using System.Collections.Generic;
//using System.Threading.Tasks;
//using Unity.Services.Authentication;
//using Unity.Services.Core;
//using Unity.Services.Vivox;
//using UnityEngine;

//public class VivoxVoiceManager : MonoBehaviour
//{
//    public static VivoxVoiceManager Instance;

//    [Header("Vivox")]
//    public string channelName = "SalleDeClasse";

//    [Header("Positional audio")]
//    public int audibleDistance = 25;
//    public int conversationalDistance = 2;
//    public float fadeIntensity = 1f;

//    private bool vivoxInitialized;
//    private bool joinedChannel;
//    private bool isMuted;

//    public string LocalDisplayName { get; private set; }
//    // --- Gestion des avatars fantômes pour les utilisateurs PC / spectateurs ---
//    private readonly Dictionary<string, GameObject> fakeAvatars = new Dictionary<string, GameObject>();
//    //fin ajout

//    private async void Awake()
//    {
//        if (Instance != null && Instance != this)
//        {
//            Destroy(gameObject);
//            return;
//        }

//        Instance = this;
//        DontDestroyOnLoad(gameObject);
//        // ----- MODE SPECTATEUR PC (ÉDITEUR UNITY) -----
//        // Permet au PC de rejoindre Vivox même sans avatar
//        if (Application.isEditor)
//        {
//            Debug.Log("[Vivox] Mode spectateur PC : connexion Vivox sans XR.");
//            _ = AutoJoinVivoxPC();
//        }


//        // IMPORTANT : attendre UGS + ton avatar local + ensuite Vivox
//        await WaitForUGSReady();
//        await WaitForLocalAvatarReady();
//        await InitializeVivoxOnly();
//    }

//    // --------------------
//    // 1) Attendre UGS
//    // --------------------
//    private async Task WaitForUGSReady()
//    {
//        Debug.Log("[Vivox] Attente UGS...");

//        while (UnityServices.State != ServicesInitializationState.Initialized ||
//               !AuthenticationService.Instance.IsSignedIn)
//        {
//            await Task.Delay(200);
//        }

//        Debug.Log("[Vivox] ✔ UGS OK, proceeding...");
//    }

//    // --------------------
//    // 2) Attendre que l'avatar local soit spawn par NGO
//    // --------------------
//    private async Task WaitForLocalAvatarReady()
//    {
//        Debug.Log("[Vivox] Attente de l'avatar réseau local...");

//        PlayerAvatar avatar = null;

//        while (avatar == null)
//        {
//            var avatars = FindObjectsByType<PlayerAvatar>(
//                FindObjectsInactive.Include,
//                FindObjectsSortMode.None);

//            foreach (var a in avatars)
//            {
//                if (a.IsOwner) // Notre avatar local
//                {
//                    avatar = a;
//                    break;
//                }
//            }

//            await Task.Delay(200);
//        }

//        Debug.Log("[Vivox] ✔ Avatar local trouvé !");
//    }

//    // --------------------
//    // 3) Initialiser Vivox (sans toucher UGS)
//    // --------------------
//    private async Task InitializeVivoxOnly()
//    {
//        try
//        {
//            if (!vivoxInitialized)
//            {
//                await VivoxService.Instance.InitializeAsync();
//                vivoxInitialized = true;
//            }

//            VivoxService.Instance.ParticipantAddedToChannel += OnParticipantAdded;
//            VivoxService.Instance.ParticipantRemovedFromChannel += OnParticipantRemoved;

//            await LoginVivox();
//            await JoinChannel();

//        }
//        catch (System.Exception e)
//        {
//            Debug.LogError("[Vivox] ❌ Erreur Init Vivox : " + e);
//        }
//    }

//    // --------------------
//    // 4) Login Vivox
//    // --------------------
//    private async Task LoginVivox()
//    {
//        LocalDisplayName = "User" + Random.Range(1000, 9999);

//        var options = new LoginOptions { DisplayName = LocalDisplayName };

//        await VivoxService.Instance.LoginAsync(options);

//        Debug.Log("[Vivox] ✔ Login OK : " + LocalDisplayName);
//    }

//    // --------------------
//    // 5) Joindre le canal 3D
//    // --------------------
//    private async Task JoinChannel()
//    {
//        if (joinedChannel) return;

//        var props = new Channel3DProperties(
//            audibleDistance,
//            conversationalDistance,
//            fadeIntensity,
//            AudioFadeModel.InverseByDistance
//        );

//        await VivoxService.Instance.JoinPositionalChannelAsync(
//            channelName,
//            ChatCapability.AudioOnly,
//            props
//        );

//        joinedChannel = true;
//        Debug.Log("[Vivox] ✔ Canal vocal rejoint : " + channelName);
//    }

//    // --------------------
//    // 6) Participant ajouté
//    // --------------------
//    private void OnParticipantAdded(VivoxParticipant participant)
//    {
//        Debug.Log($"[Vivox] Participant joint : {participant.DisplayName}");

//        var avatars = FindObjectsByType<PlayerAvatar>(
//            FindObjectsInactive.Include,
//            FindObjectsSortMode.None);

//        foreach (var avatar in avatars)
//        {
//            if (avatar.VivoxDisplayName == participant.DisplayName)
//            {
//                if (avatar.VoiceAttachPoint == null)
//                {
//                    Debug.LogError("[Vivox] ❌ VoiceAttachPoint est NULL sur l'avatar ! Vérifie ton prefab !");
//                    return;
//                }

//                var voice = avatar.GetComponent<VoicePlayerAudio>();
//                if (voice == null)
//                    voice = avatar.gameObject.AddComponent<VoicePlayerAudio>();

//                voice.InitializeForParticipant(participant, avatar.VoiceAttachPoint);
//                Debug.Log("[Vivox] ✔ Audio 3D assigné !");
//                return;
//            }
//        }
//        // Ne pas créer d'avatar fantôme pour l'utilisateur local
//        if (participant.IsSelf)
//        {
//            Debug.Log("[Vivox] Participant local → pas de fake avatar.");
//            return;
//        }

//        Debug.LogWarning("[Vivox] Aucun avatar correspondant trouvé.");
//        // ------------------------------------------------------------
//        //  CRÉATION D’UN AVATAR FANTÔME (pour tests PC / pas d'avatar réel)
//        // ------------------------------------------------------------
//        if (!fakeAvatars.ContainsKey(participant.DisplayName))
//        {
//            Debug.LogWarning("[Vivox] → Création d’un avatar audio fantôme pour : " + participant.DisplayName);

//            GameObject ghost = new GameObject("FakeAvatar_" + participant.DisplayName);
//            ghost.transform.position = new Vector3(0, 1.7f, 0); // hauteur humaine par défaut

//            // Attacher la source audio Vivox
//            var voice = ghost.AddComponent<VoicePlayerAudio>();
//            voice.InitializeForParticipant(participant, ghost.transform);

//            fakeAvatars.Add(participant.DisplayName, ghost);

//            Debug.Log("[Vivox] ✔ Avatar fantôme créé et audio activé !");
//        }
//        //fin ajout
//    }

//    // --------------------
//    // 7) Participant retiré
//    // --------------------
//    private void OnParticipantRemoved(VivoxParticipant participant)
//    {
//        Debug.Log($"[Vivox] Participant quitté : {participant.DisplayName}");

//        var voices = FindObjectsByType<VoicePlayerAudio>(
//            FindObjectsInactive.Include,
//            FindObjectsSortMode.None);

//        foreach (var v in voices)
//        {
//            if (v.ParticipantName == participant.DisplayName)
//            {
//                Destroy(v.gameObject);
//                break;
//            }
//        }
//        // Suppression de l'avatar fantôme si existant
//        if (fakeAvatars.ContainsKey(participant.DisplayName))
//        {
//            Destroy(fakeAvatars[participant.DisplayName]);
//            fakeAvatars.Remove(participant.DisplayName);
//            Debug.Log("[Vivox] ✔ Avatar fantôme détruit.");
//        }
//        //in ajout
//        participant.DestroyVivoxParticipantTap();
//    }

//    // --------------------
//    // 8) Mise à jour de la position 3D locale
//    // --------------------
//    public void UpdateLocal3DPosition(GameObject listenerObject)
//    {
//        if (!joinedChannel) return;
//        VivoxService.Instance.Set3DPosition(listenerObject, channelName, true);
//    }
//    //Ajout pour PC fantome en mode spectateur
//    private async Task AutoJoinVivoxPC()
//    {
//        // Attendre initialisation Vivox (déjà faite dans Awake)
//        while (!vivoxInitialized)
//            await Task.Delay(200);

//        string pcName = "PCUser_" + Random.Range(1000, 9999);

//        var options = new LoginOptions { DisplayName = pcName };
//        await VivoxService.Instance.LoginAsync(options);

//        Debug.Log("[Vivox] ✔ PC connecté à Vivox en tant que : " + pcName);

//        // Canal vocal 3D
//        var props = new Channel3DProperties(
//            audibleDistance,
//            conversationalDistance,
//            fadeIntensity,
//            AudioFadeModel.InverseByDistance
//        );

//        await VivoxService.Instance.JoinPositionalChannelAsync(
//            channelName,
//            ChatCapability.AudioOnly,
//            props
//        );

//        Debug.Log("[Vivox] ✔ PC a rejoint le canal : " + channelName);
//    }

//}

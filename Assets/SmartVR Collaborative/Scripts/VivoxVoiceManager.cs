using UnityEngine;
using Unity.Services.Vivox;
using Unity.Services.Authentication;
using Unity.Services.Core;
using System.Threading.Tasks;

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
    private bool isMuted;

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

        // IMPORTANT : attendre UGS + ton avatar local + ensuite Vivox
        await WaitForUGSReady();
        await WaitForLocalAvatarReady();
        await InitializeVivoxOnly();
    }

    // --------------------
    // 1) Attendre UGS
    // --------------------
    private async Task WaitForUGSReady()
    {
        Debug.Log("[Vivox] Attente UGS...");

        while (UnityServices.State != ServicesInitializationState.Initialized ||
               !AuthenticationService.Instance.IsSignedIn)
        {
            await Task.Delay(200);
        }

        Debug.Log("[Vivox] ✔ UGS OK, proceeding...");
    }

    // --------------------
    // 2) Attendre que l'avatar local soit spawn par NGO
    // --------------------
    private async Task WaitForLocalAvatarReady()
    {
        Debug.Log("[Vivox] Attente de l'avatar réseau local...");

        PlayerAvatar avatar = null;

        while (avatar == null)
        {
            var avatars = FindObjectsByType<PlayerAvatar>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);

            foreach (var a in avatars)
            {
                if (a.IsOwner) // Notre avatar local
                {
                    avatar = a;
                    break;
                }
            }

            await Task.Delay(200);
        }

        Debug.Log("[Vivox] ✔ Avatar local trouvé !");
    }

    // --------------------
    // 3) Initialiser Vivox (sans toucher UGS)
    // --------------------
    private async Task InitializeVivoxOnly()
    {
        try
        {
            if (!vivoxInitialized)
            {
                await VivoxService.Instance.InitializeAsync();
                vivoxInitialized = true;
            }

            VivoxService.Instance.ParticipantAddedToChannel += OnParticipantAdded;
            VivoxService.Instance.ParticipantRemovedFromChannel += OnParticipantRemoved;

            await LoginVivox();
            await JoinChannel();

        }
        catch (System.Exception e)
        {
            Debug.LogError("[Vivox] ❌ Erreur Init Vivox : " + e);
        }
    }

    // --------------------
    // 4) Login Vivox
    // --------------------
    private async Task LoginVivox()
    {
        LocalDisplayName = "User" + Random.Range(1000, 9999);

        var options = new LoginOptions { DisplayName = LocalDisplayName };

        await VivoxService.Instance.LoginAsync(options);

        Debug.Log("[Vivox] ✔ Login OK : " + LocalDisplayName);
    }

    // --------------------
    // 5) Joindre le canal 3D
    // --------------------
    private async Task JoinChannel()
    {
        if (joinedChannel) return;

        var props = new Channel3DProperties(
            audibleDistance,
            conversationalDistance,
            fadeIntensity,
            AudioFadeModel.InverseByDistance
        );

        await VivoxService.Instance.JoinPositionalChannelAsync(
            channelName,
            ChatCapability.AudioOnly,
            props
        );

        joinedChannel = true;
        Debug.Log("[Vivox] ✔ Canal vocal rejoint : " + channelName);
    }

    // --------------------
    // 6) Participant ajouté
    // --------------------
    private void OnParticipantAdded(VivoxParticipant participant)
    {
        Debug.Log($"[Vivox] Participant joint : {participant.DisplayName}");

        var avatars = FindObjectsByType<PlayerAvatar>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None);

        foreach (var avatar in avatars)
        {
            if (avatar.VivoxDisplayName == participant.DisplayName)
            {
                if (avatar.VoiceAttachPoint == null)
                {
                    Debug.LogError("[Vivox] ❌ VoiceAttachPoint est NULL sur l'avatar ! Vérifie ton prefab !");
                    return;
                }

                var voice = avatar.GetComponent<VoicePlayerAudio>();
                if (voice == null)
                    voice = avatar.gameObject.AddComponent<VoicePlayerAudio>();

                voice.InitializeForParticipant(participant, avatar.VoiceAttachPoint);
                Debug.Log("[Vivox] ✔ Audio 3D assigné !");
                return;
            }
        }

        Debug.LogWarning("[Vivox] Aucun avatar correspondant trouvé.");
    }

    // --------------------
    // 7) Participant retiré
    // --------------------
    private void OnParticipantRemoved(VivoxParticipant participant)
    {
        Debug.Log($"[Vivox] Participant quitté : {participant.DisplayName}");

        var voices = FindObjectsByType<VoicePlayerAudio>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None);

        foreach (var v in voices)
        {
            if (v.ParticipantName == participant.DisplayName)
            {
                Destroy(v.gameObject);
                break;
            }
        }

        participant.DestroyVivoxParticipantTap();
    }

    // --------------------
    // 8) Mise à jour de la position 3D locale
    // --------------------
    public void UpdateLocal3DPosition(GameObject listenerObject)
    {
        if (!joinedChannel) return;
        VivoxService.Instance.Set3DPosition(listenerObject, channelName, true);
    }
}

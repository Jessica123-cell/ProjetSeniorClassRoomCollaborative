using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Vivox;
using System.Threading.Tasks;

public class VivoxVoiceManager : MonoBehaviour
{
    public static VivoxVoiceManager Instance;

    [Header("Vivox")]
    public string channelName = "SalleDeClasse";

    private bool isMuted = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private async void Start()
    {
        await InitializeVivoxAsync();
        await LoginVivoxAsync();
        await JoinVoiceChannelAsync();
    }

    private async Task InitializeVivoxAsync()
    {
        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
        await VivoxService.Instance.InitializeAsync();
        Debug.Log("[Vivox] Initialisé");
    }

    private async Task LoginVivoxAsync()
    {
        var options = new LoginOptions
        {
            DisplayName = "User" + Random.Range(1000, 9999),
            EnableTTS = false
        };

        await VivoxService.Instance.LoginAsync(options);
        Debug.Log("[Vivox] Connecté en tant que " + options.DisplayName);
    }

    /// <summary>
    /// Version sûre : rejoint un canal d'écho audio.
    /// (Tu peux ensuite remplacer par JoinChannelAsync quand ton SDK le propose vraiment.)
    /// </summary>
    private async Task JoinVoiceChannelAsync()
    {
        await VivoxService.Instance.JoinEchoChannelAsync(
            channelName,
            ChatCapability.AudioOnly
        );

        Debug.Log("[Vivox] Canal audio rejoint : " + channelName);
    }

    /// <summary>
    /// Toggle logique du mute. Pour l’instant, on ne fait qu’un log,
    /// car l’API exacte (Transmission / InputDevices) dépend de ta sous-version.
    /// Tu pourras brancher l’appel réel une fois que tu vois la méthode dans l’IntelliSense.
    /// </summary>
    public void ToggleMute()
    {
        isMuted = !isMuted;

        // TODO : quand ton IntelliSense montre l’API, tu pourras faire par ex. :
        // await VivoxService.Instance.SetChannelTransmissionModeAsync(
        //     isMuted ? Transmission.None : Transmission.All, channelName);

        Debug.Log("[Vivox] Micro (logique) : " + (isMuted ? "MUTE" : "ON"));
    }

    public bool IsMuted => isMuted;
}

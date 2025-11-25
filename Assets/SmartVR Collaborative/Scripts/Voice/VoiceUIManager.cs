using UnityEngine;
using UnityEngine.UI;
using Unity.Services.Vivox;
using TMPro;

public class VoiceUIManagerTwoButtons : MonoBehaviour
{
    [Header("UI Buttons")]
    public Button muteButton;
    public Button unmuteButton;

    [Header("UI Label (optional)")]
    public TMP_Text statusText;

    private void Start()
    {
        if (muteButton != null)
            muteButton.onClick.AddListener(MuteMicrophone);

        if (unmuteButton != null)
            unmuteButton.onClick.AddListener(UnmuteMicrophone);

        UpdateStatus("Microphone active");
    }

    public void MuteMicrophone()
    {
        VivoxService.Instance.MuteInputDevice();
        Debug.Log("[VoiceUI] Micro MUTÉ pour ce joueur.");
        UpdateStatus("Microphone muted");
    }

    public void UnmuteMicrophone()
    {
        VivoxService.Instance.UnmuteInputDevice();
        Debug.Log("[VoiceUI] Micro DÉMUTÉ pour ce joueur.");
        UpdateStatus("Microphone active");
    }

    private void UpdateStatus(string text)
    {
        if (statusText != null)
            statusText.text = text;
    }
}

using UnityEngine;
using UnityEngine.UI;

public class MicMuteToggleUI : MonoBehaviour
{
    [SerializeField] private Toggle toggle;

    private string deviceName = null;

    private void Start()
    {
        if (toggle == null)
            toggle = GetComponent<Toggle>();

        toggle.onValueChanged.AddListener(OnToggleChanged);

        toggle.isOn = true; // Micro ON par défaut
        SetMute(false);
    }

    private void OnToggleChanged(bool isOn)
    {
        SetMute(!isOn);
    }

    private void SetMute(bool mute)
    {
        if (mute)
        {
            Microphone.End(deviceName);
            Debug.Log("Micro coupé (Unity → Vivox ne reçoit plus rien)");
        }
        else
        {
            // relance la capture pour Vivox
            Microphone.Start(deviceName, true, 1, 48000);
            Debug.Log("Micro activé (Unity → Vivox reçoit la voix)");
        }
    }
}

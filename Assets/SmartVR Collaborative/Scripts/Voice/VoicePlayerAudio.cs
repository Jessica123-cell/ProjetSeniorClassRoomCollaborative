using UnityEngine;
using Unity.Services.Vivox;

public class VoicePlayerAudio : MonoBehaviour
{
    private AudioSource vivoxAudioSource;
    private Transform audioAttachPoint;

    // Utilisé par VivoxVoiceManager pour identifier ce participant
    public string ParticipantName { get; private set; }

    /// <summary>
    /// Initialise l'audio pour un participant Vivox
    /// et attache le son à un point (main droite, tête, etc.).
    /// </summary>
    public void InitializeForParticipant(VivoxParticipant participant, Transform attachPoint)
    {
        ParticipantName = participant.DisplayName;
        audioAttachPoint = attachPoint;

        GameObject tapGO = participant.CreateVivoxParticipantTap("VivoxAudio_" + ParticipantName);

        if (tapGO == null)
        {
            Debug.LogError("[VoicePlayerAudio] TAP GameObject est NULL pour " + ParticipantName);
            return;
        }

        vivoxAudioSource = tapGO.GetComponent<AudioSource>();

        if (vivoxAudioSource == null)
        {
            // Vivox place parfois l'AudioSource dans un enfant
            vivoxAudioSource = tapGO.GetComponentInChildren<AudioSource>();
        }

        if (vivoxAudioSource == null)
        {
            Debug.LogError("[VoicePlayerAudio] Aucun AudioSource trouvé dans le TAP pour " + ParticipantName);
            return;
        }

        // Config audio (tu peux ajuster ces valeurs)
        vivoxAudioSource.spatialBlend = 1f;              // 3D complet
        vivoxAudioSource.rolloffMode = AudioRolloffMode.Logarithmic;
        vivoxAudioSource.minDistance = 1f;
        vivoxAudioSource.maxDistance = 25f;

        Debug.Log("[VoicePlayerAudio] TAP OK pour " + ParticipantName);
    }

    private void LateUpdate()
    {
        if (vivoxAudioSource == null || audioAttachPoint == null)
            return;

        // On place le GameObject créé par Vivox sur le point choisi (tête / main)
        vivoxAudioSource.transform.position = audioAttachPoint.position;
        vivoxAudioSource.transform.rotation = audioAttachPoint.rotation;
    }

    private void OnDestroy()
    {
        // Nettoyage (évite les leaks si ce composant est détruit)
        if (vivoxAudioSource != null)
        {
            Destroy(vivoxAudioSource.gameObject);
        }
    }
}

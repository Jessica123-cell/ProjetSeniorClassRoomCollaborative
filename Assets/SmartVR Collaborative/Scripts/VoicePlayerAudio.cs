using UnityEngine;
using Unity.Services.Vivox;

public class VoicePlayerAudio : MonoBehaviour
{
    private AudioSource vivoxAudioSource;
    private Transform audioAttachPoint;

    // Utilisé par VivoxVoiceManager pour identifier ce participant
    public string ParticipantName { get; private set; }

    /// <summary>
    /// Initialise l'audio spatial pour un participant Vivox,
    /// et attache le son à un point (main droite ou tête).
    /// </summary>
    public void InitializeForParticipant(VivoxParticipant participant, Transform attachPoint)
    {
        ParticipantName = participant.DisplayName;
        audioAttachPoint = attachPoint;

        GameObject go = participant.CreateVivoxParticipantTap("VivoxAudio_" + ParticipantName);

        if (go == null)
        {
            Debug.LogError("[VoicePlayerAudio] ❌ TAP GO est NULL !");
            return;
        }

        vivoxAudioSource = go.GetComponent<AudioSource>();

        if (vivoxAudioSource == null)
        {
            // Vivox place parfois l'AudioSource dans un enfant
            vivoxAudioSource = go.GetComponentInChildren<AudioSource>();
        }

        if (vivoxAudioSource == null)
        {
            Debug.LogError("[VoicePlayerAudio] ❌ Aucun AudioSource trouvé dans le TAP !");
            return;
        }

        // Config audio
        vivoxAudioSource.spatialBlend = 0f;  // On mettra 3D après debug
        vivoxAudioSource.rolloffMode = AudioRolloffMode.Linear;
        vivoxAudioSource.minDistance = 1f;
        vivoxAudioSource.maxDistance = 25f;

        Debug.Log("[VoicePlayerAudio] ✔ TAP OK pour " + ParticipantName);
    }


    private void LateUpdate()
    {
        if (vivoxAudioSource == null || audioAttachPoint == null)
            return;

        // Déplace le GO Vivox sur la main/tête du joueur
        vivoxAudioSource.transform.position = audioAttachPoint.position;
        vivoxAudioSource.transform.rotation = audioAttachPoint.rotation;
    }

    private void OnDestroy()
    {
        // Nettoyage (évite les leaks)
        if (vivoxAudioSource != null)
        {
            Destroy(vivoxAudioSource.gameObject);
        }
    }
}

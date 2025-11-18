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

        // Crée l'objet audio Vivox (le "tap")
        GameObject go = participant.CreateVivoxParticipantTap("VivoxAudio_" + ParticipantName);

        vivoxAudioSource = go.GetComponent<AudioSource>();

        if (vivoxAudioSource != null)
        {
            vivoxAudioSource.spatialBlend = 1f;   // 3D audio
            vivoxAudioSource.rolloffMode = AudioRolloffMode.Logarithmic;
            vivoxAudioSource.minDistance = 0.8f;
            vivoxAudioSource.maxDistance = 20f;
        }
        else
        {
            Debug.LogWarning("[VoicePlayerAudio] Aucun AudioSource trouvé dans VivoxParticipantTap !");
        }
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

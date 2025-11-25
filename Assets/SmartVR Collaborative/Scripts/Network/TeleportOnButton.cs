using UnityEngine;
using UnityEngine.InputSystem;
using Unity.XR.CoreUtils; // Pour XROrigin

public class TeleportOnButtonX : MonoBehaviour
{
    [Header("Teleport Points")]
    public Transform teleportAnchor;   // Zone spéciale
    public Transform spawnPoint;       // Position d'apparition du joueur

    [Header("XR Origin")]
    public XROrigin xrOrigin;

    [Header("Input Action (Button X)")]
    public InputActionProperty xButtonAction;

    private bool isAtAnchor = false;   // Toggle

    private void OnEnable()
    {
        if (xButtonAction != null)
            xButtonAction.action.Enable();
    }

    private void OnDisable()
    {
        if (xButtonAction != null)
            xButtonAction.action.Disable();
    }

    private void Update()
    {
        if (xButtonAction != null && xButtonAction.action.WasPressedThisFrame())
        {
            ToggleTeleport();
        }
    }

    private void ToggleTeleport()
    {
        if (xrOrigin == null || teleportAnchor == null || spawnPoint == null)
        {
            Debug.LogWarning("Assign XR Origin, TeleportAnchor et SpawnPoint !");
            return;
        }

        if (!isAtAnchor)
        {
            // Va vers l'anchor
            TeleportTo(teleportAnchor);
            isAtAnchor = true;
        }
        else
        {
            // Retourne au spawn
            TeleportTo(spawnPoint);
            isAtAnchor = false;
        }
    }

    private void TeleportTo(Transform target)
    {
        // --- Déplace ---
        xrOrigin.MoveCameraToWorldLocation(target.position);

        // --- Oriente horizontalement ---
        Vector3 forward = target.forward;
        forward.y = 0f;
        forward.Normalize();

        xrOrigin.MatchOriginUpCameraForward(Vector3.up, forward);
    }
}

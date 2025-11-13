using Unity.Netcode;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(GrabbableObject))]
public class MultiplayerGrabInteractable : XRGrabInteractable
{
    private GrabbableObject grabbable;

    protected override void Awake()
    {
        base.Awake();
        grabbable = GetComponent<GrabbableObject>();
    }

    // Quand un joueur attrape l'objet
    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);

        // vérifie que le client est bien en réseau
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsConnectedClient)
        {
            ulong localId = NetworkManager.Singleton.LocalClientId;
            grabbable.OnGrab(localId);
        }
    }

    // Quand un joueur relâche l'objet
    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        base.OnSelectExited(args);

        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsConnectedClient)
        {
            grabbable.OnRelease();
        }
    }
}

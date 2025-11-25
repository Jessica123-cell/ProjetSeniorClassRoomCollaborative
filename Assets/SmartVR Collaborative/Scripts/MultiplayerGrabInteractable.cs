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

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);

        // si le client est réseau
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsConnectedClient)
        {
            ulong id = NetworkManager.Singleton.LocalClientId;
            grabbable.ClientRequestGrab(id);
        }
    }

    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        base.OnSelectExited(args);

        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsConnectedClient)
        {
            grabbable.ClientRequestRelease();
        }
    }
}

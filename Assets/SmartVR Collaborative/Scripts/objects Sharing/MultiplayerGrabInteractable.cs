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
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsConnectedClient)
        {
            ulong localId = NetworkManager.Singleton.LocalClientId;

            // Empêche un deuxième joueur de prendre l'objet
            if (grabbable.IsLocked && grabbable.NetworkObject.OwnerClientId != localId)
            {
                if (interactionManager != null)
                {
                    interactionManager.SelectExit(args.interactorObject, this);
                }
                return;
            }

            // Grab local normal
            base.OnSelectEntered(args);

            // Demande au serveur d'attribuer l'ownership
            grabbable.ClientRequestGrab(localId);
        }
        else
        {
            // Mode hors réseau
            base.OnSelectEntered(args);
        }
    }

    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsConnectedClient)
        {
            base.OnSelectExited(args);

            // Relâche proprement côté serveur
            grabbable.ClientRequestRelease();
        }
        else
        {
            base.OnSelectExited(args);
        }
    }
}

//using Unity.Netcode;
//using UnityEngine;
//using UnityEngine.XR.Interaction.Toolkit;
//using UnityEngine.XR.Interaction.Toolkit.Interactables;

//[RequireComponent(typeof(GrabbableObject))]
//public class MultiplayerGrabInteractable : XRGrabInteractable
//{
//    private GrabbableObject grabbable;

//    protected override void Awake()
//    {
//        base.Awake();
//        grabbable = GetComponent<GrabbableObject>();
//    }

//    protected override void OnSelectEntered(SelectEnterEventArgs args)
//    {
//        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsConnectedClient)
//        {
//            ulong localId = NetworkManager.Singleton.LocalClientId;

//            // Empêche un deuxième joueur de prendre l'objet
//            if (grabbable.IsLocked && grabbable.NetworkObject.OwnerClientId != localId)
//            {
//                if (interactionManager != null)
//                {
//                    interactionManager.SelectExit(args.interactorObject, this);
//                }
//                return;
//            }

//            // Grab local normal
//            base.OnSelectEntered(args);

//            // Demande au serveur d'attribuer l'ownership
//            grabbable.ClientRequestGrab(localId);
//        }
//        else
//        {
//            // Mode hors réseau
//            base.OnSelectEntered(args);
//        }
//    }

//    protected override void OnSelectExited(SelectExitEventArgs args)
//    {
//        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsConnectedClient)
//        {
//            base.OnSelectExited(args);

//            // Relâche proprement côté serveur
//            grabbable.ClientRequestRelease();
//        }
//        else
//        {
//            base.OnSelectExited(args);
//        }
//    }
//}

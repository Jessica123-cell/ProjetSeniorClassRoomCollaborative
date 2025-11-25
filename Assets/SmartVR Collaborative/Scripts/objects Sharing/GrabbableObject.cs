using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(NetworkObject))]
[RequireComponent(typeof(MultiplayerGrabInteractable))]
public class GrabbableObject : NetworkBehaviour
{
    // Verrou réseau : true = quelqu'un tient l'objet
    private NetworkVariable<bool> isLocked = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private Rigidbody rb;
    private MultiplayerGrabInteractable xri;

    public bool IsLocked => isLocked.Value;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        xri = GetComponent<MultiplayerGrabInteractable>();
    }

    // ------------------------------------------------------
    //  AU SPAWN RESEAU (objets déjà dans la scène inclus)
    // ------------------------------------------------------
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // Le serveur devient propriétaire par défaut
        if (IsServer)
        {
            if (!NetworkObject.IsOwnedByServer)
            {
                NetworkObject.ChangeOwnership(NetworkManager.ServerClientId);
            }
        }

        ApplyRestState();
    }

    // ------------------------------------------------------
    //  ETATS PHYSIQUES
    // ------------------------------------------------------
    private void ApplyRestState()
    {
        if (rb == null) return;

        rb.isKinematic = false;     // physique active
        rb.useGravity = true;       // tombe normalement
    }

    private void ApplyGrabState()
    {
        if (rb == null) return;

        rb.isKinematic = false;     // toujours simulé
        rb.useGravity = false;      // ne tombe pas quand on le tient
    }

    // ------------------------------------------------------
    //  APPELS CÔTÉ CLIENT
    // ------------------------------------------------------
    public void ClientRequestGrab(ulong clientId)
    {
        TryGrabRpc(clientId);
    }

    public void ClientRequestRelease()
    {
        ReleaseRpc();
    }

    // ------------------------------------------------------
    //  RPC SERVEUR
    // ------------------------------------------------------
    [Rpc(SendTo.Server)]
    private void TryGrabRpc(ulong clientId)
    {
        // Si l'objet est déjà pris par quelqu'un d'autre → on ignore
        if (isLocked.Value && NetworkObject.OwnerClientId != clientId)
            return;

        // Lock pour ce client
        isLocked.Value = true;

        // Transfert de propriété si besoin
        if (NetworkObject.OwnerClientId != clientId)
        {
            NetworkObject.ChangeOwnership(clientId);
        }

        ApplyGrabState();

        // Force les AUTRES clients à relâcher localement
        ForceReleaseRpc(clientId);
    }

    [Rpc(SendTo.Server)]
    private void ReleaseRpc()
    {
        if (!isLocked.Value)
            return;

        // Déverrouillage
        isLocked.Value = false;

        // Le serveur récupère l'ownership quand personne ne tient l'objet
        NetworkObject.ChangeOwnership(NetworkManager.ServerClientId);

        // Tout le monde relâche la sélection
        ForceReleaseRpc(ulong.MaxValue);

        ApplyRestState();
    }

    // ------------------------------------------------------
    //  RPC CLIENTS
    // ------------------------------------------------------
    [Rpc(SendTo.Everyone)]
    private void ForceReleaseRpc(ulong newOwnerId)
    {
        // Si newOwnerId != MaxValue → on ne relâche PAS sur le client propriétaire
        if (NetworkManager.Singleton == null)
            return;

        if (newOwnerId != ulong.MaxValue &&
            NetworkManager.Singleton.LocalClientId == newOwnerId)
        {
            // ce client est le nouveau owner → il garde la sélection
            return;
        }

        if (xri != null && xri.isSelected && xri.interactionManager != null)
        {
            // IMPORTANT : on copie la liste pour éviter "Collection was modified"
            var listCopy = new List<IXRSelectInteractor>(xri.interactorsSelecting);

            foreach (var interactor in listCopy)
            {
                xri.interactionManager.SelectExit(interactor, xri);
            }
        }
    }
}
//using System.Collections.Generic;
//using Unity.Netcode;
//using UnityEngine;
//using UnityEngine.XR.Interaction.Toolkit;
//using UnityEngine.XR.Interaction.Toolkit.Interactors;

//public class GrabbableObject : NetworkBehaviour
//{
//    // verrou réseau
//    private NetworkVariable<bool> isLocked = new NetworkVariable<bool>(
//        false,
//        NetworkVariableReadPermission.Everyone,
//        NetworkVariableWritePermission.Server
//    );

//    private Rigidbody rb;
//    private MultiplayerGrabInteractable xri;

//    public bool IsLocked => isLocked.Value;

//    private void Awake()
//    {
//        rb = GetComponent<Rigidbody>();
//        xri = GetComponent<MultiplayerGrabInteractable>();
//    }

//    // ------------------------------------------------------
//    //  AU SPAWN RESEAU (OBJETS DEJA DANS LA SCÈNE INCLUS)
//    // ------------------------------------------------------
//    public override void OnNetworkSpawn()
//    {
//        base.OnNetworkSpawn();

//        // Le serveur prend l’ownership si personne ne l’a.
//        if (IsServer)
//        {
//            if (!NetworkObject.IsOwnedByServer)
//            {
//                NetworkObject.ChangeOwnership(NetworkManager.ServerClientId);
//            }
//        }

//        ApplyRestState();
//    }

//    // ------------------------------------------------------
//    //  ETATS PHYSIQUES
//    // ------------------------------------------------------
//    private void ApplyRestState()
//    {
//        if (rb == null) return;

//        rb.isKinematic = false;     // physique active
//        rb.useGravity = true;       // gravité active
//    }

//    private void ApplyGrabState()
//    {
//        if (rb == null) return;

//        rb.isKinematic = false;
//        rb.useGravity = false;
//    }

//    // ------------------------------------------------------
//    //  APPELS COTÉ CLIENT
//    // ------------------------------------------------------
//    public void ClientRequestGrab(ulong clientId)
//    {
//        TryGrabRpc(clientId);
//    }

//    public void ClientRequestRelease()
//    {
//        ReleaseRpc();
//    }

//    // ------------------------------------------------------
//    //  RPC SERVEUR
//    // ------------------------------------------------------
//    [Rpc(SendTo.Server)]
//    private void TryGrabRpc(ulong clientId)
//    {
//        // Si l'objet est déjà pris par quelqu'un d'autre → refus propre
//        if (isLocked.Value && NetworkObject.OwnerClientId != clientId)
//        {
//            // Renvoie juste au client B de relâcher localement
//            ForceReleaseRpc(NetworkObject.OwnerClientId);
//            return;
//        }

//        // Sinon, grab autorisé
//        isLocked.Value = true;
//        NetworkObject.ChangeOwnership(clientId);
//        ApplyGrabState();

//        // On force uniquement les autres à relâcher
//        ForceReleaseRpc(clientId);
//    }


//    [Rpc(SendTo.Server)]
//    private void ReleaseRpc()
//    {
//        if (!isLocked.Value)
//            return;

//        isLocked.Value = false;

//        NetworkObject.RemoveOwnership();

//        ForceReleaseRpc(ulong.MaxValue);

//        ApplyRestState();
//    }

//    // ------------------------------------------------------
//    //  RPC CLIENTS
//    // ------------------------------------------------------
//    [Rpc(SendTo.Everyone)]
//    private void ForceReleaseRpc(ulong newOwnerId)
//    {
//        if (NetworkManager.Singleton.LocalClientId != newOwnerId)
//        {
//            if (xri != null && xri.isSelected && xri.interactionManager != null)
//            {
//                // On copie AVANT de parcourir
//                var listCopy = new List<IXRSelectInteractor>(xri.interactorsSelecting);

//                foreach (var interactor in listCopy)
//                {
//                    xri.interactionManager.SelectExit(interactor, xri);
//                }
//            }
//        }
//    }

//}

using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class GrabbableObject : NetworkBehaviour
{
    // verrou réseau
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
    //  AU SPAWN RESEAU (OBJETS DEJA DANS LA SCÈNE INCLUS)
    // ------------------------------------------------------
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // Le serveur prend l’ownership si personne ne l’a.
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
        rb.useGravity = true;       // gravité active
    }

    private void ApplyGrabState()
    {
        if (rb == null) return;

        rb.isKinematic = false;
        rb.useGravity = false;
    }

    // ------------------------------------------------------
    //  APPELS COTÉ CLIENT
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
        if (isLocked.Value && NetworkObject.OwnerClientId != clientId)
        {
            // Refuse officiellement, sans modifier l'état de l'objet
            ForceReleaseRpc(NetworkObject.OwnerClientId);
            return;
        }


        isLocked.Value = true;

        NetworkObject.ChangeOwnership(clientId);

        ApplyGrabState();

        ForceReleaseRpc(clientId);
    }

    [Rpc(SendTo.Server)]
    private void ReleaseRpc()
    {
        if (!isLocked.Value)
            return;

        isLocked.Value = false;

        NetworkObject.RemoveOwnership();

        ForceReleaseRpc(ulong.MaxValue);

        ApplyRestState();
    }

    // ------------------------------------------------------
    //  RPC CLIENTS
    // ------------------------------------------------------
    [Rpc(SendTo.Everyone)]
    private void ForceReleaseRpc(ulong newOwnerId)
    {
        if (NetworkManager.Singleton.LocalClientId != newOwnerId)
        {
            if (xri != null && xri.isSelected && xri.interactionManager != null)
            {
                // On copie AVANT de parcourir
                var listCopy = new List<IXRSelectInteractor>(xri.interactorsSelecting);

                foreach (var interactor in listCopy)
                {
                    xri.interactionManager.SelectExit(interactor, xri);
                }
            }
        }
    }

}

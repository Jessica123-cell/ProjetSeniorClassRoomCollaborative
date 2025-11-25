using Unity.Netcode;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class GrabbableObject : NetworkBehaviour
{
    private Rigidbody rb;

    // verrouillage anti-double grab
    private NetworkVariable<bool> isLocked = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private MultiplayerGrabInteractable xri;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        xri = GetComponent<MultiplayerGrabInteractable>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        ApplyRestState();
    }

    public override void OnGainedOwnership()
    {
        base.OnGainedOwnership();
        ApplyGrabState();
    }

    public override void OnLostOwnership()
    {
        base.OnLostOwnership();
        ApplyRestState();
    }

    private void ApplyRestState()
    {
        rb.isKinematic = true;
        rb.useGravity = false;
    }

    private void ApplyGrabState()
    {
        rb.isKinematic = false;
        rb.useGravity = false;
    }

    // appelé par le client
    public void ClientRequestGrab(ulong clientId)
    {
        TryGrabRpc(clientId);
    }

    public void ClientRequestRelease()
    {
        ReleaseRpc();
    }

    // ----------------- SERVER RPC ---------------------

    [Rpc(SendTo.Server)]
    private void TryGrabRpc(ulong clientId)
    {
        // déjà pris ?
        if (isLocked.Value)
        {
            // refuse
            ForceReleaseRpc(ulong.MaxValue);
            return;
        }

        // verrouille
        isLocked.Value = true;

        // transfert de propriété
        NetworkObject.ChangeOwnership(clientId);

        // informe tous les autres : relachez tout !
        ForceReleaseRpc(clientId);
    }

    [Rpc(SendTo.Server)]
    private void ReleaseRpc()
    {
        // serveur reprend la propriété
        NetworkObject.RemoveOwnership();

        // déverrouille
        isLocked.Value = false;

        // forcer relâchement partout
        ForceReleaseRpc(ulong.MaxValue);
    }

    // ---------------- CLIENT RPC ----------------------

    [Rpc(SendTo.Everyone)]
    private void ForceReleaseRpc(ulong newOwner)
    {
        // si CE client n’est PAS le nouveau propriétaire
        if (NetworkManager.Singleton.LocalClientId != newOwner)
        {
            // force le XRI à relâcher immédiatement
            if (xri != null && xri.isSelected)
            {
                foreach (var interactor in xri.interactorsSelecting)
                {
                    xri.interactionManager.SelectExit(interactor, xri);
                }
            }
        }
    }
}

using Unity.Netcode;
using UnityEngine;

public class GrabbableObject : NetworkBehaviour
{
    private Rigidbody rb;

    // Verrou réseau (anti-double grab)
    private NetworkVariable<bool> isLocked = new NetworkVariable<bool>(
        false,                                     // initial value
        NetworkVariableReadPermission.Everyone,    // everyone can read
        NetworkVariableWritePermission.Server      // only server writes
    );

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
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

    // --- ETATS PHYSIQUES ---

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

    // --- EVENEMENTS LOCAUX ---

    public void OnGrab(ulong clientId)
    {
        TryGrabRpc(clientId);
    }

    public void OnRelease()
    {
        ReleaseRpc();
    }

    // --- RPC SERVEUR ---

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void TryGrabRpc(ulong clientId)
    {
        // déjà pris ?
        if (isLocked.Value)
            return;

        // verrouiller
        isLocked.Value = true;

        NetworkObject.ChangeOwnership(clientId);
        ApplyGrabState();
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void ReleaseRpc()
    {
        // serveur reprend
        NetworkObject.RemoveOwnership();

        // déverrouiller
        isLocked.Value = false;

        ApplyRestState();
    }
}

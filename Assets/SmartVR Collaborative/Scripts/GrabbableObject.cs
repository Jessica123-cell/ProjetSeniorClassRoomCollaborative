using Unity.Netcode;
using UnityEngine;

public class GrabbableObject : NetworkBehaviour
{
    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void OnGrab(ulong clientId)
    {
        RequestOwnershipRpc(clientId);
    }

    public void OnRelease()
    {
        ReleaseOwnershipRpc();
    }

    // Le client appelle → le serveur reçoit
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void RequestOwnershipRpc(ulong clientId)
    {
        if (NetworkObject.OwnerClientId != clientId)
        {
            NetworkObject.ChangeOwnership(clientId);
        }
    }

    // Le client appelle → le serveur reçoit
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void ReleaseOwnershipRpc()
    {
        NetworkObject.RemoveOwnership();
    }
}

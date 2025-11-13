using Unity.Netcode;
using UnityEngine;

public class GrabbableObject : NetworkBehaviour
{
    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        SetNoPhysics();
    }

    public override void OnGainedOwnership()
    {
        base.OnGainedOwnership();
        SetNoPhysics();
    }

    public override void OnLostOwnership()
    {
        base.OnLostOwnership();
        SetNoPhysics();
    }

    private void SetNoPhysics()
    {
        // L’objet ne tombe jamais, reste stable dans l’air,
        // et ne flotte pas au spawn
        rb.isKinematic = true;
        rb.useGravity = false;
    }

    // Appelé lorsque le joueur attrape l'objet
    public void OnGrab(ulong clientId)
    {
        RequestOwnershipRpc(clientId);
    }

    // Appelé lorsque le joueur relâche l'objet
    // (aucune action nécessaire — l’objet reste en place)
    public void OnRelease()
    {
        ReleaseOwnershipRpc();
    }

    // Le client demande au serveur de lui donner l’ownership
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void RequestOwnershipRpc(ulong clientId)
    {
        if (NetworkObject.OwnerClientId != clientId)
        {
            NetworkObject.ChangeOwnership(clientId);
        }
    }

    // Le client demande au serveur de retirer l’ownership
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void ReleaseOwnershipRpc()
    {
        NetworkObject.RemoveOwnership();
    }
}

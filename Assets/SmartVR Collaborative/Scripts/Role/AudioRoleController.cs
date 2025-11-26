using UnityEngine;
using Unity.Netcode;
using Unity.Services.Vivox;

public class AudioRoleController : NetworkBehaviour
{
    private PlayerRoleManager roleManager;
    public static AudioRoleController Local { get; private set; }

    private void Start()
    {
        if (IsOwner)
            Local = this;
        roleManager = GetComponent<PlayerRoleManager>();

        if (IsOwner)
            ApplyLocalAudioPermissions();

        roleManager.PlayerRole.OnValueChanged += (_, _) =>
        {
            if (IsOwner)
                ApplyLocalAudioPermissions();
        };
    }

    private void ApplyLocalAudioPermissions()
    {
        if (roleManager.IsObserver)
        {
            VivoxService.Instance.MuteInputDevice();
            Debug.Log("[AUDIO] Observer → micro OFF forcé.");
        }
        else
        {
            VivoxService.Instance.UnmuteInputDevice();
            Debug.Log("[AUDIO] Micro ON (Teacher/Student).");
        }
    }

    // ---- MUTER SON PROPRE MICRO ----
    public void ToggleSelfMute(bool mute)
    {
        if (roleManager.IsObserver)
        {
            Debug.Log("[AUDIO] Observer ne peut pas activer son micro.");
            return;
        }

        if (mute) VivoxService.Instance.MuteInputDevice();
        else VivoxService.Instance.UnmuteInputDevice();

        Debug.Log("[AUDIO] Self mute = " + mute);
    }

    // ---- TEACHER MUTER UN AUTRE ----
    public void TeacherMutePlayer(ulong targetClientId, bool mute)
    {
        if (!roleManager.IsTeacher)
        {
            Debug.LogWarning("[AUDIO] Seul le teacher peut muter les autres.");
            return;
        }

        RequestRemoteMuteServerRpc(targetClientId, mute);
    }

    // ---- SERVER RPC ----
    [ServerRpc(RequireOwnership = false)]
    private void RequestRemoteMuteServerRpc(ulong targetClientId, bool mute)
    {
        var sendParams = new ClientRpcSendParams
        {
            TargetClientIds = new[] { targetClientId }
        };

        var rpcParams = new ClientRpcParams
        {
            Send = sendParams
        };

        RemoteMuteClientRpc(mute, rpcParams);
    }

    // ---- CLIENT RPC ciblé ----
    [ClientRpc]
    private void RemoteMuteClientRpc(bool mute, ClientRpcParams rpcParams = default)
    {
        if (mute) VivoxService.Instance.MuteInputDevice();
        else VivoxService.Instance.UnmuteInputDevice();

        Debug.Log("[AUDIO] Teacher a " + (mute ? "MUTÉ" : "DÉMUTÉ") + " ce joueur.");
    }
}

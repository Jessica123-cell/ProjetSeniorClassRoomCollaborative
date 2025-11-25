using Unity.Netcode;
using UnityEngine;
using System.Collections;

public class PlayerRoleManager : NetworkBehaviour
{
    public NetworkVariable<Role> PlayerRole = new NetworkVariable<Role>(
        Role.Student,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public static PlayerRoleManager LocalPlayer { get; private set; }

    private void Start()
    {
        if (IsOwner)
        {
            LocalPlayer = this;
            Debug.Log($"[ROLE] Local Player role = {PlayerRole.Value}");
        }

        PlayerRole.OnValueChanged += OnRoleChanged;
    }

    private void OnRoleChanged(Role oldRole, Role newRole)
    {
        Debug.Log($"[ROLE] Role changed: {oldRole} → {newRole}");
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
            StartCoroutine(DelayedMenu());
    }

    private IEnumerator DelayedMenu()
    {
        yield return null;
        ShowRoleMenu();
    }

    private void ShowRoleMenu()
    {
        var menu = Object.FindAnyObjectByType<RoleSelectionUI>(FindObjectsInactive.Include);

        if (menu != null)
            menu.gameObject.SetActive(true);
        else
            Debug.LogWarning("[ROLE] Aucun RoleSelectionUI trouvé !");
    }

    public void SetRole(Role newRole)
    {
        if (IsServer)
            PlayerRole.Value = newRole;
        else
            SetRoleServerRpc(newRole);
    }

    [ServerRpc]
    private void SetRoleServerRpc(Role newRole)
    {
        PlayerRole.Value = newRole;
    }

    public bool IsTeacher => PlayerRole.Value == Role.Teacher;
    public bool IsStudent => PlayerRole.Value == Role.Student;
    public bool IsObserver => PlayerRole.Value == Role.Observer;
}

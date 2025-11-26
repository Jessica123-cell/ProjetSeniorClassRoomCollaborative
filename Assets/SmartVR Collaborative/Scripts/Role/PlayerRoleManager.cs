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

        if (!IsOwner)
            return;

        // Mise à jour de l’UI Teacher
        UpdateTeacherPanel();
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
            StartCoroutine(DelayedStartup());
    }

    private IEnumerator DelayedStartup()
    {
        // Petite attente : XR Rig + UI doivent être instanciés
        yield return new WaitForSeconds(0.3f);

        // Affiche le menu de sélection de rôle
        ShowRoleMenu();

        // Affiche le panel Teacher si nécessaire
        UpdateTeacherPanel();
    }

    private void UpdateTeacherPanel()
    {
        var teacherUI = FindAnyObjectByType<TeacherAudioUI>(FindObjectsInactive.Include);

        if (teacherUI != null)
        {
            teacherUI.ShowIfTeacher(IsTeacher);
            Debug.Log("[ROLE] TeacherPanel visible = " + IsTeacher);
        }
        else
        {
            Debug.LogWarning("[ROLE] TeacherAudioUI introuvable !");
        }
    }

    private void ShowRoleMenu()
    {
        var menu = FindAnyObjectByType<RoleSelectionUI>(FindObjectsInactive.Include);

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

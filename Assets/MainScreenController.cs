using UnityEngine;
using UnityEngine.UI;

public class MainScreenController : MonoBehaviour
{
    [Header("Diapo Buttons")]
    public Button nextPageBTN;
    public Button prevPageBTN;

    void Start()
    {
        StartCoroutine(WaitForRole());
    }

    private System.Collections.IEnumerator WaitForRole()
    {
        // Attendre que LocalPlayer soit prêt
        while (PlayerRoleManager.LocalPlayer == null)
            yield return null;

        // Attendre que son rôle soit initialisé
        while (!PlayerRoleManager.LocalPlayer.RoleIsInitialized)
            yield return null;

        // 1) Première mise à jour
        ApplyRoleToButtons(PlayerRoleManager.LocalPlayer.IsTeacher);

        // 2) IMPORTANT : écoute les changements de rôle en temps réel
        PlayerRoleManager.LocalPlayer.PlayerRole.OnValueChanged += OnRoleChanged;
    }

    private void OnRoleChanged(Role oldRole, Role newRole)
    {
        ApplyRoleToButtons(newRole == Role.Teacher);
    }

    private void ApplyRoleToButtons(bool isTeacher)
    {
        nextPageBTN.interactable = isTeacher;
        prevPageBTN.interactable = isTeacher;

        nextPageBTN.gameObject.SetActive(isTeacher);
        prevPageBTN.gameObject.SetActive(isTeacher);

        Debug.Log("[MainScreenController] Buttons updated = " + isTeacher);
    }

    public void NextPage()
    {
        if (!PlayerRoleManager.LocalPlayer.IsTeacher)
            return;

        SmartUnivManager.instance.OnNextPage();
    }

    public void PrevPage()
    {
        if (!PlayerRoleManager.LocalPlayer.IsTeacher)
            return;

        SmartUnivManager.instance.OnPrevPage();
    }
}



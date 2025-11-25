using UnityEngine;
using UnityEngine.UI;

public class RoleSelectionUI : MonoBehaviour
{
    public void ChooseTeacher()
    {
        Choose(Role.Teacher);
    }

    public void ChooseStudent()
    {
        Choose(Role.Student);
    }

    public void ChooseObserver()
    {
        Choose(Role.Observer);
    }

    private void Choose(Role role)
    {
        var player = PlayerRoleManager.LocalPlayer;

        if (player != null)
        {
            player.SetRole(role);
            Debug.Log("[ROLE UI] You are the " + role);
        }
        else
        {
            Debug.LogError("[ROLE UI] LocalPlayer is NULL !");
        }

        gameObject.SetActive(false);
    }
}

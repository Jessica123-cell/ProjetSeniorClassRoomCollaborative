using UnityEngine;
using UnityEngine.UI;

public class MainScreenController : MonoBehaviour
{
    [Header("Diapo Buttons")]
    public Button nextPageBTN;
    public Button prevPageBTN;

    void Start()
    {
        // Récupérer le joueur local
        var player = PlayerRoleManager.LocalPlayer;

        if (player == null)
        {
            Debug.LogWarning("[MainScreenController] LocalPlayer is NULL at Start!");
            return;
        }

        // Vérifier si le joueur est enseignant
        bool isTeacher = player.CurrentRole == Role.Teacher;

        // Seul le professeur peut cliquer les boutons
        nextPageBTN.interactable = isTeacher;
        prevPageBTN.interactable = isTeacher;
    }

    public void NextPage()
    {
        SmartUnivManager.instance.OnNextPage();
    }

    public void PrevPage()
    {
        SmartUnivManager.instance.OnPrevPage();
    }
}


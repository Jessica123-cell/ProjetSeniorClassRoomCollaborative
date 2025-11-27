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
        //  Attendre que LocalPlayer soit prêt
        while (PlayerRoleManager.LocalPlayer == null)
            yield return null;

        //  Attendre que le rôle soit choisi
        while (!PlayerRoleManager.LocalPlayer.RoleIsInitialized)
            yield return null;

        // Appliquer les permissions
        bool isTeacher = PlayerRoleManager.LocalPlayer.IsTeacher;
        nextPageBTN.interactable = isTeacher;
        prevPageBTN.interactable = isTeacher;

        // (optionnel mais propre : cacher complètement)
        nextPageBTN.gameObject.SetActive(isTeacher);
        prevPageBTN.gameObject.SetActive(isTeacher);

        Debug.Log("[MainScreenController] Buttons enabled = " + isTeacher);
    }


    public void NextPage()
    {
        if (!PlayerRoleManager.LocalPlayer.IsTeacher)
            return; // Bloque les students
        SmartUnivManager.instance.OnNextPage();
    }

    public void PrevPage()
    {
        if (!PlayerRoleManager.LocalPlayer.IsTeacher)
            return; //  Bloque les students
        SmartUnivManager.instance.OnPrevPage();
    }
}



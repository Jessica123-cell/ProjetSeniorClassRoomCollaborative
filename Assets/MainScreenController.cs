using UnityEngine;

public class MainScreenController : MonoBehaviour
{
    public void NextPage()
    {
        SmartUnivManager.instance.OnNextPage();
    }

    public void PrevPage()
    {
        SmartUnivManager.instance.OnPrevPage();
    }
}

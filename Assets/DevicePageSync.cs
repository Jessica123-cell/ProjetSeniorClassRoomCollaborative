using Unity.Netcode;
using UnityEngine;

public class DevicePageSync : NetworkBehaviour
{
    [Header("Reference to the device logic (SmartUnivManager OR InteractiveTablet)")]
    public MonoBehaviour pageController;  // will hold SmartUnivManager OR InteractiveTablet

    private System.Action<int> forceSetPage;
    private System.Func<int> getPageIndex;

    private NetworkVariable<int> syncedPage = new NetworkVariable<int>(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private void Awake()
    {
        // link to the correct functions depending on controller type
        if (pageController is SmartUnivManager m)
        {
            forceSetPage = m.ForceSetPage;
            getPageIndex = () => m.PageIndex;
        }
        else if (pageController is InteractiveTablet t)
        {
            forceSetPage = t.ForceSetPage;
            getPageIndex = () => t.GetCurrentPageIndex();
        }
    }

    private void Start()
    {
        syncedPage.OnValueChanged += (oldVal, newVal) =>
        {
            forceSetPage?.Invoke(newVal);
        };
    }

    // Called by UI local buttons via unique wrapper button scripts
    [ServerRpc(RequireOwnership = false)]
    public void NextPageServerRpc()
    {
        int newIndex = syncedPage.Value + 1;

        if (pageController is InteractiveTablet t)
        {
            if (newIndex >= t.document.documentPages.Length)
                return; // STOP si on dépasse
        }

        syncedPage.Value = newIndex;
    }

    [ServerRpc(RequireOwnership = false)]
    public void PrevPageServerRpc()
    {
        int newIndex = syncedPage.Value - 1;

        if (newIndex < 0)
            return; // STOP si on dépasse

        syncedPage.Value = newIndex;
    }
}

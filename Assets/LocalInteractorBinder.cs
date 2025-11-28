using Unity.Netcode;
using UnityEngine;

public class LocalInteractorBinder : NetworkBehaviour
{
    [Header("Rig Reference")]
    public GameObject xrRig; // Assign� via VRPlayerConnector

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsOwner)
        {
            SetRigInteractors(false);
        }
        else
        {
            SetRigInteractors(true);
        }
    }

    private void SetRigInteractors(bool state)
    {
        if (xrRig == null) return;

        foreach (var interactor in xrRig.GetComponentsInChildren<MonoBehaviour>(true))
        {
            if (interactor is UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor ||
                interactor is UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInputInteractor ||
                interactor is UnityEngine.XR.Interaction.Toolkit.Interactors.XRGazeInteractor ||
                interactor is UnityEngine.XR.Interaction.Toolkit.Interactors.XRPokeInteractor)
            {
                interactor.enabled = state;
            }
        }
    }
}

using Unity.Netcode;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class DisableRemoteXRInteractors : NetworkBehaviour
{
    public XRRayInteractor leftRay;
    public XRRayInteractor rightRay;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            // Désactive les rayons des avatars distants
            if (leftRay) leftRay.enabled = false;
            if (rightRay) rightRay.enabled = false;
        }
    }
}
  
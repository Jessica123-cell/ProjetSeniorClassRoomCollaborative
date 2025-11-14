using UnityEngine;
using UnityEngine.XR;
using System.Collections.Generic;

public class VRPlayerConnector : MonoBehaviour
{
    public Transform globalForwardReference;

    private void Start()
    {
        // délai pour que le casque initialise le tracking
        Invoke(nameof(CalibratePlayer), 0.3f);
    }

    private void CalibratePlayer()
    {
        FixHeight();
        RecenterXR();
        AlignYaw();
    }

    private void FixHeight()
    {
        Transform cameraOffset = transform.Find("Camera Offset");
        if (cameraOffset != null)
        {
            cameraOffset.localPosition = new Vector3(0, 1.6f, 0);
            Debug.Log("Camera height normalized to 1.6m");
        }
    }

    private void RecenterXR()
    {
        var subsystems = new List<XRInputSubsystem>();
        SubsystemManager.GetSubsystems(subsystems);

        if (subsystems.Count > 0)
            subsystems[0].TryRecenter();
    }

    private void AlignYaw()
    {
        if (globalForwardReference == null) return;

        Vector3 target = globalForwardReference.forward;
        target.y = 0;
        target.Normalize();

        Vector3 current = Camera.main.transform.forward;
        current.y = 0;
        current.Normalize();

        float angle = Vector3.SignedAngle(current, target, Vector3.up);
        transform.Rotate(0, angle, 0);
    }
}

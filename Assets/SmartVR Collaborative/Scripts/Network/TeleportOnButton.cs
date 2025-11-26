using UnityEngine;
using UnityEngine.InputSystem;
using Unity.XR.CoreUtils;

public class TeleportOnButtonX : MonoBehaviour
{
    [Header("Teleport Points (Teacher)")]
    public Transform teacherPoint1;
    public Transform teacherPoint2;
    public Transform teacherPoint3;

    [Header("Teleport Points (Student)")]
    public Transform studentPoint1;
    public Transform studentPoint2;

    [Header("XR Origin")]
    public XROrigin xrOrigin;

    [Header("Input Action (Button X)")]
    public InputActionProperty xButtonAction;

    private int currentIndex = 0;

    private void OnEnable()
    {
        if (xButtonAction != null)
            xButtonAction.action.Enable();
    }

    private void OnDisable()
    {
        if (xButtonAction != null)
            xButtonAction.action.Disable();
    }

    private void Update()
    {
        if (xButtonAction != null && xButtonAction.action.WasPressedThisFrame())
        {
            TeleportNext();
        }
    }

    private void TeleportNext()
    {
        if (xrOrigin == null)
        {
            Debug.LogWarning("XR Origin non assigné !");
            return;
        }

        // --- RÉCUPÉRER LE RÔLE ---
        var roleMgr = PlayerRoleManager.LocalPlayer;
        if (roleMgr == null)
        {
            Debug.LogWarning("PlayerRoleManager.LocalPlayer introuvable !");
            return;
        }

        bool isTeacher = roleMgr.IsTeacher;

        // --- LISTE DES POINTS SELON LE RÔLE ---
        Transform[] points;

        if (isTeacher)
        {
            points = new Transform[] { teacherPoint1, teacherPoint2, teacherPoint3 };
        }
        else
        {
            points = new Transform[] { studentPoint1, studentPoint2 };
        }

        // Sécurité : vérifier que les points existent
        if (points.Length == 0 || points[currentIndex] == null)
        {
            Debug.LogWarning("Points de téléportation non assignés !");
            return;
        }

        Transform target = points[currentIndex];

        TeleportTo(target);

        // Passe au point suivant
        currentIndex = (currentIndex + 1) % points.Length;
    }

    private void TeleportTo(Transform target)
    {
        xrOrigin.MoveCameraToWorldLocation(target.position);

        // Orientation horizontale seulement
        Vector3 forward = target.forward;
        forward.y = 0f;
        forward.Normalize();

        xrOrigin.MatchOriginUpCameraForward(Vector3.up, forward);
    }
}

using UnityEngine;

public class FollowUI : MonoBehaviour
{
    public enum FollowMode
    {
        None,
        FollowHeadAlways,
        FollowHeadOnEnable,
        FixedScreenCorner
    }

    [Header("Mode de suivi")]
    public FollowMode mode = FollowMode.None;

    [Header("Références")]
    public Transform head;

    [Header("Offsets (pour FollowHead modes)")]
    public float distance = 1.2f;
    public float heightOffset = 0.0f;

    [Header("Offsets (pour Fixed Corner)")]
    public Vector3 fixedLocalPosition = new Vector3(0.8f, 0.7f, 1.2f);

    private void OnEnable()
    {
        if (mode == FollowMode.FollowHeadOnEnable)
            PlaceInFrontOfHead();
        else if (mode == FollowMode.FixedScreenCorner)
            PlaceInFixedCorner();
    }

    private void LateUpdate()
    {
        if (head == null) return;

        switch (mode)
        {
            case FollowMode.FollowHeadAlways:
                PlaceInFrontOfHead();
                break;

            case FollowMode.FixedScreenCorner:
                // Le canvas reste fixe par rapport à la tête (un HUD)
                PlaceInFixedCorner();
                break;

            case FollowMode.None:
            case FollowMode.FollowHeadOnEnable:
                // Ne rien faire
                break;
        }
    }

    private void PlaceInFrontOfHead()
    {
        transform.position = head.position + head.forward * distance + Vector3.up * heightOffset;

        transform.LookAt(head);
        transform.rotation = Quaternion.Euler(0,
            transform.rotation.eulerAngles.y + 180,
            0);
    }

    private void PlaceInFixedCorner()
    {
        // Position relative à la tête (en haut à droite)
        Vector3 targetPos = head.TransformPoint(fixedLocalPosition);
        transform.position = targetPos;

        // Le canvas regarde vers l'utilisateur
        transform.LookAt(head);
        transform.rotation = Quaternion.Euler(0,
            transform.rotation.eulerAngles.y + 180,
            0);
    }
}

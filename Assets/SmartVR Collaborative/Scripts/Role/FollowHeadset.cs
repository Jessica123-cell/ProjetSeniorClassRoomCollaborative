using UnityEngine;

public class FollowHeadset : MonoBehaviour
{
    public Transform head;

    private void LateUpdate()
    {
        if (head == null) return;

        // Positionne devant le joueur
        transform.position = head.position + head.forward * 1.2f;

        // Toujours face au joueur
        transform.LookAt(head);
        transform.rotation = Quaternion.Euler(0,
            transform.rotation.eulerAngles.y + 180,
            0);
    }
}
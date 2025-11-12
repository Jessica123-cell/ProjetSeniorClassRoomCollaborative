using Unity.Netcode;
using UnityEngine;

public class PlayerAvatar : NetworkBehaviour
{
    public Transform leftHand;
    public Transform rightHand;

    private Transform vrLeft;
    private Transform vrRight;

    private void Awake()
    {
        if (leftHand == null) leftHand = transform.Find("LeftHand");
        if (rightHand == null) rightHand = transform.Find("RightHand");
    }

    private void Start()
    {
        if (IsOwner)
        {
            // Recherche plus souple des contrôleurs (compat Quest 2 / Quest 3 / OpenXR)
            vrLeft = FindXRController("Left Controller", "LeftHand Controller", "LeftHand", "LeftHand Device");
            vrRight = FindXRController("Right Controller", "RightHand Controller", "RightHand", "RightHand Device");

            if (vrLeft == null || vrRight == null)
                Debug.LogWarning("Impossible de trouver les contrôleurs VR dans la scène !");
            else
                Debug.Log($"Contrôleurs trouvés : Gauche = {vrLeft.name}, Droite = {vrRight.name}");
        }
        else
        {
            Debug.Log($"Avatar distant détecté : Client {OwnerClientId}");
        }
    }

    private void Update()
    {
        if (IsOwner && vrLeft && vrRight)
        {
            leftHand.position = vrLeft.position;
            leftHand.rotation = vrLeft.rotation;

            rightHand.position = vrRight.position;
            rightHand.rotation = vrRight.rotation;
        }
    }

    // 🧩 Fonction utilitaire pour détecter les contrôleurs selon plusieurs noms possibles
    private Transform FindXRController(params string[] possibleNames)
    {
        foreach (string name in possibleNames)
        {
            var obj = GameObject.Find(name);
            if (obj != null)
            {
                Debug.Log($"✅ Contrôleur trouvé : {name}");
                return obj.transform;
            }
        }
        return null;
    }
}

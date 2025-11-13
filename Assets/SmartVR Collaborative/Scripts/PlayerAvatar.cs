using Unity.Netcode;
using UnityEngine;

public class PlayerAvatar : NetworkBehaviour
{
    [Header("Références des mains")]
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
            // Masque toutes les géométries des manettes réseau locales
            HideLocalControllerModels();

            // Recherche souple des contrôleurs (Quest 2 / Quest 3 / OpenXR)
            vrLeft = FindXRController("Left Controller", "LeftHand Controller", "LeftHand", "LeftHand Device");
            vrRight = FindXRController("Right Controller", "RightHand Controller", "RightHand", "RightHand Device");

            if (vrLeft == null || vrRight == null)
                Debug.LogWarning("Impossible de trouver les contrôleurs VR dans la scène !");
            else
                Debug.Log($" Contrôleurs trouvés : Gauche = {vrLeft.name}, Droite = {vrRight.name}");
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

    // Masque TOUTES les meshes du modèle réseau local
    private void HideLocalControllerModels()
    {
        foreach (var renderer in GetComponentsInChildren<MeshRenderer>(true))
        {
            string n = renderer.name.ToLower();

            // On masque tout ce qui appartient aux manettes ou aux mains locales
            if (n.Contains("Controller_Base") ||
                n.Contains("thumb") ||
                n.Contains("Trigger") ||
                n.Contains("Button_Home") ||
                n.Contains("Button_A") ||
                n.Contains("Button_B") ||
                n.Contains("TouchPad") ||
                n.Contains("ThumbStick") ||
                n.Contains("ThumbStick_Base") ||
                n.Contains("Bumper") ||
                n.Contains("hand") ||
                renderer.transform.IsChildOf(leftHand) ||
                renderer.transform.IsChildOf(rightHand))
            {
                renderer.enabled = false;
            }
        }

        Debug.Log("Modèles de manettes réseau masqués pour le joueur local !");
    }

    // Recherche souple des contrôleurs XR dans la scène
    private Transform FindXRController(params string[] possibleNames)
    {
        foreach (string name in possibleNames)
        {
            var obj = GameObject.Find(name);
            if (obj != null)
            {
                Debug.Log($"Contrôleur trouvé : {name}");
                return obj.transform;
            }
        }
        return null;
    }
}

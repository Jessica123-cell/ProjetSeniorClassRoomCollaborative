using Unity.Netcode;
using UnityEngine;

public class PlayerAvatar : NetworkBehaviour
{
    [Header("Vivox")]
    public string VivoxDisplayName { get; set; }
    public Transform VoiceAttachPoint; // point d'attache de la voix (main droite)

    [Header("Références des mains réseau")]
    public Transform leftHand;
    public Transform rightHand;

    // Références XR locales (controllers réels)
    private Transform vrLeft;
    private Transform vrRight;

    private void Awake()
    {
        // Sécurise les références
        if (leftHand == null) leftHand = transform.Find("LeftHand");
        if (rightHand == null) rightHand = transform.Find("RightHand");

        // Le point d'attache du son est par défaut la main droite
        if (VoiceAttachPoint == null) VoiceAttachPoint = rightHand;
    }

    private void Start()
    {
        if (IsOwner)
        {
            // Récupère ton DisplayName Vivox
            VivoxDisplayName = VivoxVoiceManager.Instance.LocalDisplayName;

            // Cache les modèles réseau pour le joueur local
            HideLocalControllerModels();

            // Recherche souple des contrôleurs VR (Quest / OpenXR)
            vrLeft = FindXRController("Left Controller", "LeftHand Controller", "LeftHand", "LeftHand Device");
            vrRight = FindXRController("Right Controller", "RightHand Controller", "RightHand", "RightHand Device");

            if (!vrLeft || !vrRight)
                Debug.LogWarning("[Avatar] Impossible de trouver les contrôleurs XR dans la scène !");
            else
                Debug.Log($"[Avatar] Contrôleurs trouvés : G = {vrLeft.name} | D = {vrRight.name}");
        }
        else
        {
            Debug.Log($"[Avatar] Avatar distant détecté pour client {OwnerClientId}");
        }
    }

    private void Update()
    {
        if (!IsOwner) return;

        if (vrLeft && vrRight)
        {
            // Mise à jour réseau des mains
            leftHand.SetPositionAndRotation(vrLeft.position, vrLeft.rotation);
            rightHand.SetPositionAndRotation(vrRight.position, vrRight.rotation);
        }

        // 🔊 Mise à jour de la position 3D de la voix Vivox
        if (VivoxVoiceManager.Instance != null && VoiceAttachPoint != null)
        {
            VivoxVoiceManager.Instance.UpdateLocal3DPosition(VoiceAttachPoint.gameObject);
        }
    }

    // ------------------------------------------------------------------
    //  Masquage des modèles réseau pour le joueur local
    // ------------------------------------------------------------------
    private void HideLocalControllerModels()
    {
        foreach (var renderer in GetComponentsInChildren<MeshRenderer>(true))
        {
            string n = renderer.name.ToLower();

            // Masque toutes les géométries de mains/manettes locales
            if (n.Contains("controller") ||
                n.Contains("thumb") ||
                n.Contains("trigger") ||
                n.Contains("button") ||
                n.Contains("stick") ||
                n.Contains("hand") ||
                renderer.transform.IsChildOf(leftHand) ||
                renderer.transform.IsChildOf(rightHand))
            {
                renderer.enabled = false;
            }
        }

        Debug.Log("[Avatar] Modèles des manettes réseau masqués pour le joueur local.");
    }

    // ------------------------------------------------------------------
    // Recherche souple des contrôleurs VR dans la scène
    // ------------------------------------------------------------------
    private Transform FindXRController(params string[] possibleNames)
    {
        foreach (string name in possibleNames)
        {
            GameObject obj = GameObject.Find(name);
            if (obj != null)
            {
                Debug.Log($"[Avatar] Contrôleur trouvé : {name}");
                return obj.transform;
            }
        }
        return null;
    }
}

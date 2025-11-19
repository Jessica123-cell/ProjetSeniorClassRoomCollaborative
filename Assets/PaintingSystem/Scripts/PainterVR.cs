using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class PainterVR : MonoBehaviour
{
    [Header("References")]
    public Transform controllerTip; // Le point au bout de ta manette (crée un petit Empty child)
    public RenderTexture PaintTexture;
    public RenderTexture PreviewTexture;
    public Material PaintMat;
    public MeshFilter Quad;
    public Color color;

    [Header("VR Input")]
    public InputActionProperty paintAction; // Trigger action (Select ou Activate)

    private bool isPainting;
    private CommandBuffer cmd;

    void Start()
    {
        cmd = new CommandBuffer { name = "PainterVR Buffer" };
    }

    void Update()
    {
        if (controllerTip == null || PaintMat == null || Quad == null)
            return;

        // --- Détection du point de contact avec le Quad ---
        Ray ray = new Ray(controllerTip.position, controllerTip.forward);

        if (Physics.Raycast(ray, out RaycastHit hitInfo))
        {
            Debug.DrawRay(ray.origin, ray.direction * hitInfo.distance, Color.red);

            Vector3 localPoint = Quad.transform.InverseTransformPoint(hitInfo.point);
            Vector2 uv = new Vector2(localPoint.x + 0.5f, localPoint.y + 0.5f);
            PaintMat.SetVector("_MousePos", uv);
        }

        // --- Prévisualisation du pinceau ---
        Color.RGBToHSV(color, out float H, out float s, out float v);
        PaintMat.SetColor("_color", Color.HSVToRGB(H, s, v + 0.1f));

        cmd.Clear();
        cmd.SetRenderTarget(PreviewTexture);
        cmd.ClearRenderTarget(true, true, Color.black);
        cmd.SetViewProjectionMatrices(Matrix4x4.identity, Matrix4x4.identity);
        cmd.Blit(null, PreviewTexture, PaintMat, 0);
        Graphics.ExecuteCommandBuffer(cmd);

        // --- Lecture du trigger VR ---
        float triggerValue = paintAction.action.ReadValue<float>();
        isPainting = triggerValue > 0.5f;

        if (isPainting)
        {
            PaintMat.SetColor("_Color", color);

            cmd.Clear();
            cmd.SetRenderTarget(PaintTexture);
            cmd.SetViewProjectionMatrices(Matrix4x4.identity, Matrix4x4.identity);
            cmd.Blit(null, PaintTexture, PaintMat, 0);
            Graphics.ExecuteCommandBuffer(cmd);
        }

        // --- Effacer la peinture avec le bouton secondaire gauche (optionnel) ---
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            cmd.Clear();
            cmd.SetRenderTarget(PaintTexture);
            cmd.ClearRenderTarget(true, true, Color.black);
            Graphics.ExecuteCommandBuffer(cmd);
        }
    }
}

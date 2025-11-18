using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.InputSystem;
using UnityEngine.XR;

public class Painter : MonoBehaviour
{

    public Transform RightHand;
    public LineRenderer lineRenderer;
    [Range(0.001f, 0.1f)]
    public float brushSize = 0.02f;
    [Header("References")]
    public Camera mainCam;               // Assigne Camera.main dans l'Inspector
    public RenderTexture PaintTexture;
    public RenderTexture PreviewTexture;
    public Material PaintMat;
    public MeshFilter Quad;
    public Color color = Color.red;
    public Texture2D defaultCircle;

    [Header("Input Actions")]
    public InputActionProperty pointerPosition;   // Valeur Vector2 (souris ou joystick)
    public InputActionProperty paintAction;       // Bouton ou trigger
    public InputActionProperty clearAction;       // Bouton pour effacer (optionnel, ex. espace)

    private CommandBuffer cmd;

    private Vector2 lastMouseUV;

    void OnEnable()
    {
        pointerPosition.action.Enable();
        paintAction.action.Enable();
        clearAction.action.Enable();


        Graphics.Blit(defaultCircle, PaintTexture);
    }

    void OnDisable()
    {
        pointerPosition.action.Disable();
        paintAction.action.Disable();
        clearAction.action.Disable();
    }

    void Start()
    {
        if (mainCam == null)
            mainCam = Camera.main;

        cmd = new CommandBuffer { name = "Painter Buffer" };
    }
    
    void LateUpdate()
    {
        Vector3 rayOrigin;
        Vector3 rayDir;
        PaintMat.SetFloat("_BrushSize", brushSize);
        if (XRSettings.isDeviceActive)
        {
            
            if (RightHand != null)
            {
                rayOrigin = RightHand.transform.position;
                rayDir = RightHand.transform.forward;
            }
            else
            {
                rayOrigin = mainCam.transform.position;
                rayDir = mainCam.transform.forward;
            }
        }
        else
        {
            Vector2 pointerPosScreen = pointerPosition.action.ReadValue<Vector2>();
            Ray ray = mainCam.ScreenPointToRay(pointerPosScreen);
            rayOrigin = ray.origin;
            rayDir = ray.direction;
        }
        Ray raycast = new Ray(rayOrigin, rayDir);
        if (Physics.Raycast(raycast, out RaycastHit hitInfo))
        {
            Debug.DrawRay(raycast.origin, raycast.direction * hitInfo.distance, Color.red);
            Vector3 localPoints = Quad.transform.InverseTransformPoint(hitInfo.point);
            Vector2 uv = new Vector2(localPoints.x + 0.5f, localPoints.y + 0.5F);
            
            if (hitInfo.collider != null && hitInfo.collider.tag.Equals("Canvas"))
            {
                lineRenderer.SetPosition(0, RightHand.transform.position);
                lineRenderer.SetPosition(1, hitInfo.point);
                lineRenderer.enabled = true;

                for (float i = 10.0f; i >= 0f; i -= 1f)
                {
                    DrawCanvas(Vector2.Lerp(lastMouseUV, uv, 1f / i));
                }

                lastMouseUV = uv;
            } else
            {
                lineRenderer.enabled = false;
            }
        }
    }

    private void DrawCanvas(Vector2 uv)
    {
        PaintMat.SetVector("_MousePos", uv);

        // Preview
        Color.RGBToHSV(color, out float H, out float s, out float v);
        PaintMat.SetColor("_color", Color.HSVToRGB(H, s, v + 0.1f));

        cmd.Clear();
        cmd.SetRenderTarget(PreviewTexture);
        cmd.ClearRenderTarget(true, true, Color.black);
        cmd.SetViewProjectionMatrices(Matrix4x4.identity, Matrix4x4.identity);
        cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, PaintMat, 0, 0);
        Graphics.ExecuteCommandBuffer(cmd);

        // Peinture si le bouton est pressé
        if (paintAction.action.ReadValue<float>() > 0.5f)
        {
            PaintMat.SetColor("_Color", color);

            cmd.Clear();
            cmd.SetRenderTarget(PaintTexture);
            cmd.SetViewProjectionMatrices(Matrix4x4.identity, Matrix4x4.identity);
            cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, PaintMat, 0, 0);
            Graphics.ExecuteCommandBuffer(cmd);
        }

        // Effacer la peinture si bouton clear pressé
        if (clearAction.action.ReadValue<float>() > 0.5f)
        {
            cmd.Clear();
            cmd.SetRenderTarget(PaintTexture);
            cmd.ClearRenderTarget(true, true, Color.black);
            Graphics.ExecuteCommandBuffer(cmd);

            Graphics.Blit(defaultCircle, PaintTexture);
        }
    }
}


using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;


public class Painters : MonoBehaviour
{
    Camera mainCam;
    public RenderTexture PaintTexture;
    public RenderTexture PreviewTexture;
    public Material PaintMat;
    public MeshFilter Quad;
    public Color color;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mainCam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hitInfo))
        {

            Debug.DrawRay(hitInfo.point, hitInfo.normal, Color.red);
            Vector3 localPoints = Quad.transform.InverseTransformPoint(hitInfo.point);
            Vector2 pointerPos = new Vector2(localPoints.x + 0.5F, localPoints.y + 0.5f);
            PaintMat.SetVector("_MousePos", pointerPos);
        }
        //Preview
        Color.RGBToHSV(color, out float H, out float s, out float v);   
        PaintMat.SetColor("_color", Color.HSVToRGB(H,s,v+0.1f ));
        CommandBuffer cmd =  new CommandBuffer();
        cmd.SetRenderTarget(PreviewTexture);
        cmd.ClearRenderTarget(true, true, Color.black);
        cmd.SetViewProjectionMatrices(Matrix4x4.identity, Matrix4x4.identity);
        cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, PaintMat, 0, 0);
    
        Graphics.ExecuteCommandBuffer(cmd);

        //Preview
        if (Input.GetMouseButton(0))
        {
            PaintMat.SetColor("_Color", color);
            cmd.Clear();

            cmd.SetRenderTarget(PaintTexture);
            cmd.SetViewProjectionMatrices(Matrix4x4.identity, Matrix4x4.identity);
            cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, PaintMat, 0, 0);
            
            Graphics.ExecuteCommandBuffer(cmd);
        }
        //Clear paint
        if (Input.GetKeyDown(KeyCode.Space))
        {
            cmd.Clear();
            cmd.SetRenderTarget(PaintTexture);
            cmd.ClearRenderTarget(true, true, Color.black);
            Graphics.ExecuteCommandBuffer(cmd);    
        }
    }
}

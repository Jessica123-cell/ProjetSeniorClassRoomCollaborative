using Unity.Netcode;
using UnityEngine;

public class NetworkPainter : NetworkBehaviour
{
    public Painter painter;

    [Rpc(SendTo.Everyone)]
    public void DrawRpc(Vector2 uv)
    {
        painter.DrawCanvas(uv);
    }

    [Rpc(SendTo.Everyone)]
    public void ClearRpc()
    {
        painter.ClearCanvas();
    }
}


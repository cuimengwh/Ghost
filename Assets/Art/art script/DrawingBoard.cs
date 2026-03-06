using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class DrawingBoard : MonoBehaviour
{
    private RenderTexture cacheTex;
    private RenderTexture currentTex;
    private Material effectMat;
    private RawImage rawImage;

    public int width = 1200;
    public int height = 800;
    void Awake()
    {
        Initialized();
    }
    void Start()
    {
    }
    public void SetRectTransformPivot(RectTransform rectTransform, Vector2 pivot)
    {
        rectTransform.pivot = pivot;
    }
    private void Initialized()
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        SetRectTransformPivot(rectTransform, Vector2.zero);
        rawImage = GetComponent<RawImage>();//
        effectMat = new Material(Shader.Find("Brush/BrushEffect"));
        Shader brushShader = Shader.Find("Brush/BrushEffect");
        if (brushShader == null)
        {
            Debug.LogError("Shader 'Brush/BrushEffect' not found. Please ensure the shader exists in your project.");
            return;
        }
        cacheTex = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32);
        Graphics.Blit(null, cacheTex, effectMat, 1);
        currentTex = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32);
        Graphics.Blit(cacheTex, currentTex);
        rawImage.texture = currentTex;
    }
    public void RenderBrushToBoard(DrawingBrush brush, Vector2 uv)
    {
        Vector2 dir = uv - brush.lastuv;
        float brushSize = brush.brushSize / 2;
        int length = Mathf.CeilToInt(dir.magnitude / brushSize);
        if (Vector3.SqrMagnitude(dir) > brushSize * brushSize)
        {
            for (int i = 0; i < length; i++)
            {
                RenderToMatTex(brush, brush.lastuv + dir.normalized * i * brushSize);
            }
        }
        else
        {
            RenderToMatTex(brush, uv);
        }
    }
    private void RenderToMatTex(DrawingBrush pen, Vector2 uv)
    {
        effectMat.SetVector("_BrushPos", new Vector4(uv.x, uv.y, pen.lastuv.x, pen.lastuv.y));
        effectMat.SetColor("_BrushColor", pen.brushCol);
        effectMat.SetFloat("_BrushSize", pen.brushSize);
        effectMat.SetFloat("_ScaleX", width * 1.0f / height);
        Graphics.Blit(cacheTex, currentTex, effectMat, 0);
        Graphics.Blit(currentTex, cacheTex);
    }
    public Texture2D GetTexture2D()
    {
        Texture2D texture2D = new Texture2D(width, height, TextureFormat.ARGB32, false);
        RenderTexture.active = cacheTex;
        texture2D.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        texture2D.Apply();
        RenderTexture.active = null;
        return texture2D;
    }
    public void Clear()
    {
        Graphics.Blit(null, cacheTex, effectMat, 1);
        if (effectMat == null)
        {
            Debug.Log("effectMat is null");
        }
        Graphics.Blit(cacheTex, currentTex);
        rawImage.texture = currentTex;
        Debug.Log("have been claer ");
    }
    public void Eraser()
    {
        DrawingBrush pen = GetComponent<DrawingBrush>();
        pen.brushCol = Color.white;
    }
}
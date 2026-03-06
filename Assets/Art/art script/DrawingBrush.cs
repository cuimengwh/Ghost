using UnityEngine;
using UnityEngine.EventSystems;

public class DrawingBrush : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerDownHandler
{
    public float brushSize = 0.1f;
    public Color brushCol = Color.green;

    internal Vector2 lastuv;
    protected bool isBrush;
    protected bool isDown;

    public Transform handler;

    private bool isDrag;

    private Vector2 value;
    private float width;
    private float height;
    private DrawingBoard DrawingBoard;

    public FlexibleColorPicker flexibleColorPicker;

    public Canvas canvas; 
    public Vector2 targetPos;
    private void Awake()
    {
        width = GetComponent<RectTransform>().rect.size.x;
        height = GetComponent<RectTransform>().rect.size.y;
        DrawingBoard = GetComponent<DrawingBoard>();
        canvas = GetComponentInParent<Canvas>();
        
    }
    void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
    {
        isDown = true;
        isDrag = true;
    }

    void IDragHandler.OnDrag(PointerEventData eventData)
    {
        if (isDown)
        {
            Vector2 uv = CalculateUV();
            BrushColor(DrawingBoard, uv);
        }
    }

    void IEndDragHandler.OnEndDrag(PointerEventData eventData)
    {
        isDown = false;
        isDrag = false;
    }

    void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
    {
        lastuv = CalculateUV();
        BrushColor(DrawingBoard, value);
    }
    
    void Update()
    {
        CalculateUV();
        if (isDrag)
        {
            BrushColor(DrawingBoard, value);
        }
        brushCol = flexibleColorPicker.GetColor();
    }
    public Vector2 CalculateUV()
    {
        //Vector2 targetPos;
        if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(GetComponent<RectTransform>(), Input.mousePosition, null, out targetPos);
        }
        else
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(GetComponent<RectTransform>(), Input.mousePosition, canvas.worldCamera, out targetPos);
        }
        targetPos.y = Mathf.Clamp(targetPos.y, 0, height);
        targetPos.x = Mathf.Clamp(targetPos.x, 0, width);
        handler.localPosition = targetPos;

        value.x = targetPos.x / width;
        value.y = targetPos.y / height;
        return value;
    }
    public void BrushColor(DrawingBoard drawingBoard, Vector2 uv)
    {
        drawingBoard.RenderBrushToBoard(this, uv);
        lastuv = uv;
    }
    public void SetBrushSize(float brushSize)
    {
        handler.GetComponent<RectTransform>().sizeDelta = Vector2.one * brushSize * 2 * height;
        this.brushSize = brushSize;
    }
    public void Eraser()
    {
        flexibleColorPicker.SetColor(Color.white);
        brushCol = flexibleColorPicker.GetColor();
    }
}

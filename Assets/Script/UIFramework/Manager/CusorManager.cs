using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CusorManager : MonoBehaviour
{
    [Header("鼠标基本信息")]
    public Sprite normal, press;
    public Image cusorImage;
    private Sprite currentSprite;
    private RectTransform cusorCanvas;

    private void OnEnable()
    {
        EventHandler.SetCusorVisibleEvent += OnSetCusorVisibleEvent;
    }

    private void OnDisable()
    {
        EventHandler.SetCusorVisibleEvent -= OnSetCusorVisibleEvent;
    }

    private void Start()
    {
        OnSetCusorVisibleEvent(false);
    }

    private void OnSetCusorVisibleEvent(bool isVisible)
    {
        if(isVisible)
        {
            Cursor.visible = false;
            cusorImage.gameObject.SetActive(true);
        }
        else
        {
            Cursor.visible = true;
            cusorImage.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        CusorFollow();
        CusorChangeSprite();
    }

    private void CusorChangeSprite()
    {
        if (Input.GetMouseButton(0))
            currentSprite = press;
        else
            currentSprite = normal;

        if (cusorImage.sprite != currentSprite)
            cusorImage.sprite = currentSprite;
    }

    private void CusorFollow()
    {
        cusorImage.transform.position = Input.mousePosition;
    }

}

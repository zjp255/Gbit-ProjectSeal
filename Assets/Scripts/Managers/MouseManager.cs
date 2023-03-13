using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.EventSystems;

public class MouseManager : Singleton<MouseManager>
{
    RaycastHit hitInfo;
    
    //public EventVector3 OnMouseClicked;
    public event Action<Vector3> OnMouseClicked;
    public event Action<GameObject> OnEnemyClicked;

    public Texture2D point, doorway, attack, target, arrow;

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(this);
    }

    private void Update()
    {
        SetCursorTexture();
        MouseControl();
    }

    void SetCursorTexture()
    {
        if (InteractWithUI())
        {
            Cursor.SetCursor(arrow, new Vector2(16, 16), CursorMode.Auto);
            return;
        }

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if(Physics.Raycast(ray,out hitInfo))
        {
            //«–ªª Û±ÍÃ˘Õº
            switch (hitInfo.collider.gameObject.tag)
            {
                case "Ground":
                    Cursor.SetCursor(target, new Vector2(16, 16), CursorMode.Auto);
                    break;
                case "Enemy":
                    Cursor.SetCursor(attack, new Vector2(16, 16), CursorMode.Auto);
                    break;
                case "Protal":
                    Cursor.SetCursor(doorway, new Vector2(16, 16), CursorMode.Auto);
                    break;
                case "Item":
                    Cursor.SetCursor(point, new Vector2(16, 16), CursorMode.Auto);
                    break;
                default:
                    Cursor.SetCursor(arrow, new Vector2(16, 16), CursorMode.Auto);
                    break;
            }
        }
    }
    void MouseControl()
    {
        if (InteractWithUI()) return;
        if (Input.GetMouseButtonDown(0) && hitInfo.collider != null)
        {
            if (hitInfo.collider.gameObject.CompareTag("Ground"))
                OnMouseClicked?.Invoke(hitInfo.point);
            if (hitInfo.collider.gameObject.CompareTag("Enemy"))
                OnEnemyClicked?.Invoke(hitInfo.collider.gameObject);
            if (hitInfo.collider.gameObject.CompareTag("Attackable"))
                OnEnemyClicked?.Invoke(hitInfo.collider.gameObject);
            if (hitInfo.collider.gameObject.CompareTag("Protal"))
                OnMouseClicked?.Invoke(hitInfo.point);
            if (hitInfo.collider.gameObject.CompareTag("Item"))
                OnMouseClicked?.Invoke(hitInfo.point);
        }
    }

    bool InteractWithUI()
    {
        return (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject());
    }
}

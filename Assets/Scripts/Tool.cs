using System;
using UnityEngine;
using UnityEngine.UI;

public class Tool : MonoBehaviour {
    public Button _button;

    public RectTransform _toolTransform;

    public bool IsActive { get; private set; }

    public static Tool ActiveTool { get; private set; }

    protected virtual void Awake() {
        _toolTransform.gameObject.SetActive(false);
        enabled = false;
    }

    protected virtual void Update() {
        if (Input.GetMouseButtonDown(1)) {
            Deactivate();
        }

        _toolTransform.position = Input.mousePosition;
    }

    public virtual void Activate() {
        if (ActiveTool == this) return;

        if (ActiveTool) {
            ActiveTool.Deactivate();
        }

        ActiveTool = this;
        WindowManager.Instance.SetClickThrough(false);
        _button.targetGraphic.enabled = false;
        _toolTransform.gameObject.SetActive(true);
        enabled = true;
        Cursor.visible = false;
    }

    public virtual void Deactivate() {
        if (ActiveTool != this) return;

        ActiveTool = null;
        _button.targetGraphic.enabled = true;
        _toolTransform.gameObject.SetActive(false);
        enabled = false;
        Cursor.visible = true;
    }
}

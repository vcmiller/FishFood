using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class HoverExpandButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
    public CanvasGroup _content;

    private void Start() {
        _content.alpha = 0;
        _content.blocksRaycasts = false;
    }

    public void OnPointerEnter(PointerEventData eventData) {
        _content.alpha = 1;
        _content.blocksRaycasts = true;
    }

    public void OnPointerExit(PointerEventData eventData) {
        _content.alpha = 0;
        _content.blocksRaycasts = false;
    }
}
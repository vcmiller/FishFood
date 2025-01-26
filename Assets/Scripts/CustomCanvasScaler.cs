using UnityEngine;
using UnityEngine.UI;

public class CustomCanvasScaler : CanvasScaler {
    [SerializeField] private float _scaleFactorMultiplier = 1;

    protected override void HandleConstantPhysicalSize() {
        float currentDpi = Screen.dpi;
        float dpi = (currentDpi == 0 ? m_FallbackScreenDPI : currentDpi);
        float targetDPI = 1;
        switch (m_PhysicalUnit)
        {
            case Unit.Centimeters: targetDPI = 2.54f; break;
            case Unit.Millimeters: targetDPI = 25.4f; break;
            case Unit.Inches:      targetDPI =     1; break;
            case Unit.Points:      targetDPI =    72; break;
            case Unit.Picas:       targetDPI =     6; break;
        }

        float scaleFactorMultiplier = _scaleFactorMultiplier;
        SetScaleFactor(scaleFactorMultiplier * dpi / targetDPI);
        SetReferencePixelsPerUnit(m_ReferencePixelsPerUnit * targetDPI / m_DefaultSpriteDPI);
    }
}
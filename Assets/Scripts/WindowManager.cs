using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Infohazard.Core;
using UnityEngine;
using UnityEngine.EventSystems;

public class WindowManager : Singleton<WindowManager> {
    [DllImport("user32.dll")]
    private static extern IntPtr GetActiveWindow();

    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy,
                                            uint uFlags);

    [DllImport("Dwmapi.dll")]
    private static extern int DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS pMarInset);

    [DllImport("user32.dll")]
    private static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr CreateWindowEx(
        uint dwExStyle,
        string lpClassName,
        string lpWindowName,
        uint dwStyle,
        int x,
        int y,
        int nWidth,
        int nHeight,
        IntPtr hWndParent,
        IntPtr hMenu,
        IntPtr hInstance,
        IntPtr lpParam
    );

    [StructLayout(LayoutKind.Sequential)]
    private struct MARGINS {
        public int cxLeftWidth;
        public int cxRightWidth;
        public int cyTopHeight;
        public int cyBottomHeight;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct WindowCompositionAttributeData {
        public WindowCompositionAttribute Attribute;
        public IntPtr Data;
        public int SizeOfData;
    }

    private enum WindowCompositionAttribute {
        WCA_ACCENT_POLICY = 19,
    }

    private enum AccentState {
        ACCENT_DISABLED = 0,
        ACCENT_ENABLE_GRADIENT = 1,
        ACCENT_ENABLE_TRANSPARENTGRADIENT = 2,
        ACCENT_ENABLE_BLURBEHIND = 3,
        ACCENT_INVALID_STATE = 4
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct AccentPolicy {
        public AccentState AccentState;
        public uint AccentFlags;
        public uint GradientColor;
        public uint AnimationId;
    }

    private const int GWL_EXSTYLE = -20;

    private const uint WS_VISIBLE = 0x10000000;
    private const uint WS_POPUP = 0x80000000;
    private const uint WS_EX_LAYERED = 0x80000;
    private const uint WS_EX_TRANSPARENT = 0x20;
    private const uint WS_EX_TOOLWINDOW = 0x80;
    private const uint SWP_NOACTIVATE = 0x0010;
    private const uint SWP_NOMOVE = 0x0002;
    private const uint SWP_NOSIZE = 0x0001;
    private const uint SWP_SHOWWINDOW = 0x0040;

    private static readonly IntPtr HWND_NOTOPMOST = new(-2); // Place below other top-level windows
    private static readonly IntPtr HWND_TOPMOST = new(-1); // Place above all non-topmost windows
    private static readonly IntPtr HWND_TOP = new(0); // Position on top in Z-order
    private static readonly IntPtr HWND_BOTTOM = new(1); // Position at the bottom of the Z-order

    private IntPtr _hwnd;

    private readonly List<BlurWindow> _blurWindows = new();

    [SerializeField]
    private EventSystem _eventSystem;

    // Start is called before the first frame update
    private void Awake() {
        if (Application.platform != RuntimePlatform.WindowsPlayer) return;

        _hwnd = GetActiveWindow();

        MARGINS margins = new() { cxLeftWidth = -1 };

        DwmExtendFrameIntoClientArea(_hwnd, ref margins);
        SetWindowPos(_hwnd, HWND_TOPMOST, 0, 0, 0, 0, 0);
    }

    public BlurWindow CreateBlurRegion(RectInt rect) {
        if (Application.platform != RuntimePlatform.WindowsPlayer) {
            return new BlurWindow(IntPtr.Zero) { Rect = rect };
        }

        // Optionally create a child window here for the specific region.
        IntPtr childWindow = CreateWindowEx(
            WS_EX_LAYERED | WS_EX_TRANSPARENT | WS_EX_TOOLWINDOW, // Extended styles
            "STATIC", // Class name (arbitrary; "STATIC" works here)
            "", // Window name
            WS_POPUP | WS_VISIBLE, // Window styles
            rect.xMin, rect.yMin, rect.width, rect.height, // Position and size
            IntPtr.Zero, // Parent window
            IntPtr.Zero,
            IntPtr.Zero,
            IntPtr.Zero);

        // Enable Acrylic or Blur for the child window
        EnableAcrylicEffect(childWindow);

        // Place the blurred window behind the main window
        SetWindowPos(
            childWindow,
            HWND_TOP,
            rect.xMin, rect.yMin,
            rect.width, rect.height,
            SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE | SWP_SHOWWINDOW);

        BlurWindow newWindow = new(childWindow) { Rect = rect };
        _blurWindows.Add(newWindow);
        return newWindow;
    }

    private void EnableAcrylicEffect(IntPtr hwnd) {
        AccentPolicy policy = new() {
            AccentState = AccentState.ACCENT_ENABLE_BLURBEHIND,
            AccentFlags = 2, // Transparent gradient flag
            GradientColor = 0x99000000, // Semi-transparent black with alpha
        };

        WindowCompositionAttributeData data = new() {
            Attribute = WindowCompositionAttribute.WCA_ACCENT_POLICY,
            SizeOfData = Marshal.SizeOf(policy),
            Data = Marshal.AllocHGlobal(Marshal.SizeOf(policy))
        };

        Marshal.StructureToPtr(policy, data.Data, false);

        SetWindowCompositionAttribute(hwnd, ref data);

        Marshal.FreeHGlobal(data.Data);
    }

    private void Update() {
        if (Application.platform != RuntimePlatform.WindowsPlayer) return;

        foreach (BlurWindow blurWindow in _blurWindows) {
            SetWindowPos(
                blurWindow.Handle,
                _hwnd,
                blurWindow.Rect.xMin, blurWindow.Rect.yMin,
                blurWindow.Rect.width, blurWindow.Rect.height,
                SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE | SWP_SHOWWINDOW);
        }

        if (Tool.ActiveTool == null) {
            SetClickThrough(!CheckMouse());
        }
    }

    private bool CheckMouse() {
        List<RaycastResult> results = new();
        PointerEventData pointerData = new(EventSystem.current) {
            position = Input.mousePosition,
        };

        _eventSystem.RaycastAll(pointerData, results);

        return results.Count > 0;
    }

    public void SetClickThrough(bool clickthrough) {
        if (Application.platform != RuntimePlatform.WindowsPlayer) return;
        if (clickthrough) {
            SetWindowLong(_hwnd, GWL_EXSTYLE, WS_EX_LAYERED | WS_EX_TRANSPARENT);
        } else {
            SetWindowLong(_hwnd, GWL_EXSTYLE, WS_EX_LAYERED);
        }
    }
}

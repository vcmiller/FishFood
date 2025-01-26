using System;
using UnityEngine;

public class BlurWindow {
    public RectInt Rect { get; set; }
    public IntPtr Handle { get; }

    public BlurWindow(IntPtr handle) {
        Handle = handle;
    }
}

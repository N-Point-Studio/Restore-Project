using System;

[Flags]
public enum ToolType
{
    Brush = 1 << 0,
    Chisel = 1 << 1,
    ToothBrush = 1 << 2,
    WetCloth = 1 << 3,
}
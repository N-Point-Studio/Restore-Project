using System;

[Flags]
public enum ToolType
{
    Brush = 1 << 0,
    Chisel = 1 << 1,
}
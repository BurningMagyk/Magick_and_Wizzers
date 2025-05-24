using Godot;
using Match;
using System;

public partial class CommandView : CanvasLayer {
    public delegate void SelectCommandDelegate(Command command);
    public SelectCommandDelegate SelectCommand;
    
    public void SetViewPortRect(Rect2 viewPortRect)
    {

    }
}

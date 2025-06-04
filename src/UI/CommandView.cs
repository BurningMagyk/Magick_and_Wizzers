using Godot;
using Match;
using System;

public partial class CommandView : CanvasLayer {
    public delegate void SelectCommandDelegate(Command command);
    public SelectCommandDelegate SelectCommand;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {
        Hide();
    }
    
    public void SetViewPortRect(Rect2 viewPortRect)
    {

    }
}

using Godot;
using Match;
using System;

public partial class CommandView : CanvasLayer, IView {
    public delegate bool SelectCommandDelegate(Command command);
    public SelectCommandDelegate SelectCommand;

    public bool Showing { get; private set; }
    public bool InputEnabled { get; set; } = true;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {
        Hide();
    }
    
    public void SetViewPortRect(Rect2 viewPortRect)
    {

    }
}

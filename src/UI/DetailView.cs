using Godot;
using System;

public partial class DetailView : CanvasLayer {
    public delegate void SelectItemDelegate();
    public SelectItemDelegate SelectItem;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {
        Hide();
    }

    public void SetViewPortRect(Rect2 viewPortRect)
    {

    }
}

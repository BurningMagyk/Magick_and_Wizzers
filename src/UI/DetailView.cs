using Godot;
using System;

public partial class DetailView : CanvasLayer {
    public delegate void SelectItemDelegate();
    public SelectItemDelegate SelectItem;

    public void SetViewPortRect(Rect2 viewPortRect)
    {

    }
}

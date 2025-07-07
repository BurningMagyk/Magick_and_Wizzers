using Godot;
using System;

namespace UI {
public partial class DetailView : CanvasLayer, IView {
  public delegate bool SelectItemDelegate();
  public SelectItemDelegate SelectItem;
  public bool Showing { get; private set; }
  public bool InputEnabled { get; set; } = true;

  public delegate bool GoBackDelegate();
  public GoBackDelegate GoBack;

  // Called when the node enters the scene tree for the first time.
  public override void _Ready() {
    Hide();
  }

  public override void _Input(InputEvent @event) {
    if (!Showing) { return; }

    if (Input.IsActionJustPressed("detail") || Input.IsActionJustPressed("back")) {
      GoBack?.Invoke();
    }
  }

  public new void Show() {
    base.Show();

    Showing = true;
  }

  public new void Hide() {
    base.Hide();

    Showing = false;
  }

  public void SetViewPortRect(Rect2 viewPortRect) {

  }
}
}


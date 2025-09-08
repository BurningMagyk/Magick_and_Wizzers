using Godot;
using Match;
using System;

namespace UI {
public partial class CommandView : CanvasLayer, IView {
  public delegate bool SelectCommandDelegate(Command command, SelectTypeEnum selectTypeEnum);
  public SelectCommandDelegate SelectCommand;
  public delegate bool GoBackDelegate();
  public GoBackDelegate GoBack;

  public bool Showing { get; private set; }
  public bool InputEnabled { get; set; } = true;

  // Called when the node enters the scene tree for the first time.
  public override void _Ready() {
    Hide();
  }

  public override void _Input(InputEvent @event) {
    if (!Showing) { return; }

    if (Input.IsActionJustPressed("detail")) {
      SelectCommand?.Invoke(null, SelectTypeEnum.DETAIL);
    } else if (Input.IsActionJustPressed("back")) {
      GoBack?.Invoke();
    }
  }

  public new void Show() {
	  base.Show();
	  Showing = true;
	}
    
  public void SetViewPortRect(Rect2 viewPortRect) {

  }
}
}

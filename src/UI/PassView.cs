using Godot;
using System;

namespace UI {
public partial class PassView : CanvasLayer, IView {
  public delegate void ConfirmPassDelegate();
  public ConfirmPassDelegate ConfirmPass;
  public delegate bool GoBackDelegate();
  public GoBackDelegate GoBack;

  public bool Showing { get; private set; }
  public bool InputEnabled { get; set; } = true;

  private Rect2 viewPortRect;

  public override void _Ready() {
    // Make the base stuff invisible.
	  Hide();
  }

  public override void _Input(InputEvent @event) {
    if (!Showing) { return; }

    if (Input.IsActionJustPressed("pass")) {
      ConfirmPass?.Invoke();
    }
    if (Input.IsActionJustPressed("back")) {
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
    this.viewPortRect = viewPortRect;
  }
}
}

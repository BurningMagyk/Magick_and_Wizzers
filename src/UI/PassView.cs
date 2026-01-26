using Godot;
using System;

namespace UI {
public partial class PassView : CanvasLayer, IView {

  public bool Showing { get; private set; }
  public bool InputEnabled { get; set; } = true;

  private Rect2 viewPortRect;

  public override void _Ready() {
    // Make the base stuff invisible.
	  Hide();
  }

  public void Input(UI.InputType inputType, bool press) {
    if (!Showing) { return; }

    if (inputType == UI.InputType.PASS && press) {
      Select?.Invoke(null, WizardStep.SelectType.STANDARD);
    }
  }

  public delegate void SelectDelegate(object target, WizardStep.SelectType selectType);
	public SelectDelegate Select;

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

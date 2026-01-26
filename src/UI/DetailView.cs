using Godot;
using System;

namespace UI {
public partial class DetailView : CanvasLayer, IView {
  public bool Showing { get; private set; }
  public bool InputEnabled { get; set; } = true;

  // Called when the node enters the scene tree for the first time.
  public override void _Ready() {
	  Hide();
  }

  public void Input(UI.InputType inputType, bool press) {
	if (!Showing) { return; }

	  if (inputType == UI.InputType.DETAIL && press) {
		  Select?.Invoke(null, WizardStep.SelectType.BACK);
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

  }
}
}

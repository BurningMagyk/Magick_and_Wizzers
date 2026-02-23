using Godot;
using Main;
using Match;
using System;

namespace UI {
public partial class Item : Control {
  private TextureRect[] backgrounds;
  private Sprite2D sleeve;

  // This is typically only set in the CommandView.
  public Command CommandObject { get; set; }

  // Called when the node enters the scene tree for the first time.
  public override void _Ready() {
	  TextureRect normalBackground = GetNodeOrNull<TextureRect>("Background");

	  if (normalBackground == null) {
		  // This is going to use several backgrounds.
		  backgrounds = new TextureRect[Enum.GetValues<Command.StatusEnum>().Length];
		foreach (Command.StatusEnum status in Enum.GetValues<Command.StatusEnum>()) {
			TextureRect background = GetNode<TextureRect>("Background " + Util.ToTitleCase(status.ToString()));
			background.Visible = status == Command.StatusEnum.PENDING;
			backgrounds[(int) status] = background;
		}
	  } else {
		// This is only going to use one background.
		normalBackground.Visible = true;
		backgrounds = [normalBackground];
	  }

	  sleeve = GetNode<Sprite2D>("Sleeve");
	  
	  sleeve.Position = Size / 2;
	  sleeve.Visible = false;
  }

  public void Hover() {
	  sleeve.Visible = true;
  }
  public void Unhover() {
	  sleeve.Visible = false;
  }

  public void SetText(string text) {
	  GetNode<Label>("Label").Text = text;
  }

  public void SetStatus(Command.StatusEnum status) {
	  foreach (TextureRect background in backgrounds) {
		background.Visible = false;
	  }
	  backgrounds[(int) status].Visible = true;
  }

  public override string ToString() {
	  return $"Item: {CommandObject.Type}, Status: {CommandObject.Status}";
  }
}
}

using Godot;
using Match;
using System;

namespace UI {
public partial class CommandItem : Control {
  private Sprite2D sleeve;

  public Command.CommandType CommandType { get; set; }
  public bool Available { get; set; }

  // Called when the node enters the scene tree for the first time.
  public override void _Ready() {
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
}
}

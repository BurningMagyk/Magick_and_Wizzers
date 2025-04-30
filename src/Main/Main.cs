using Godot;
using System;
using UI;

namespace Main {
public partial class Main : Node {

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		new Match.Match(GetNode<Display.Board>("Board"), GetNode<UI.UI>("UI"));
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta) {

	}
}
}

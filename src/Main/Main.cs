using Godot;
using System;
using UI;

namespace Main {
public partial class Main : Node {
  Match.Match mMatch;

  // Called when the node enters the scene tree for the first time.
  public override void _Ready() {
    Match.Player[] players = [
      new Match.Player("Keltoí", Card.CreateRandomCard()),
      new Match.Player("Tristram", Card.CreateRandomCard())
    ];
	  mMatch = new Match.Match(GetNode<Display.Board>("Board"), GetNode<UI.UI>("UI"), players);
  }

  // Called every frame. 'delta' is the elapsed time since the previous frame.
  public override void _Process(double delta) {
	  mMatch.FrameUpdate(delta);
  }
}
}

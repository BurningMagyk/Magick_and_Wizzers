using Godot;
using System;

namespace UI {
public class ViewState {
    // Is empty or null if not casting anything.
    public Match.Piece[] casters;
    // Is empty or null if not commanding anything.
    public Match.Piece[] commandee;
    // Is null if no spell has been selected to be cast.
    public Match.Spell spell = null;
    // Is empty or null if not targeting anything.
    public Match.Target[] targets;
    // Both are null unless this ViewState is in transition.
    public ViewState prev = null, next = null;
    // 
}
}

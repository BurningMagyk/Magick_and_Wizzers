using Godot;
using Match;
using System;

namespace Display {
public class TileHighlight(bool isStart) {
  private readonly bool[] directions = new bool[8];

    public bool IsStart { get; private set; } = isStart;

    public bool IsEnd() {
    if (IsStart) {
      return false;
    }
    foreach (bool direction in directions) {
      if (direction) {
        return true;
      }
    }
    return false;
  }

  public void SetDirection(int direction) {
    directions[direction] = true;
  }
}
}
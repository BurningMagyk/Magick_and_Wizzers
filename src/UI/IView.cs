using Godot;
using System;

namespace UI {
public interface IView {
  bool Showing { get; }
  bool InputEnabled { get; set; }

  void Show();
  void Hide();
  void SetViewPortRect(Rect2 viewPortRect);

  public enum State {
    PASS,
    SURRENDER,
    DETAIL,
    MEANDER_BOARD,
    MEANDER_HAND,
    COMMAND_LIST,
    DESIGNATE_BOARD,
    DESIGNATE_HAND,
    DESIGNATE_LIST,
    THEATER,
    CAST
  }
}
}

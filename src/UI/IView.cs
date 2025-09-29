using Godot;
using System;

namespace UI {
public interface IView {
  bool Showing { get; }
  bool InputEnabled { get; set; }

  void Show();
  void Hide();
  void SetViewPortRect(Rect2 viewPortRect);
}
}

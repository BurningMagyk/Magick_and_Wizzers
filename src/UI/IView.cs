using Godot;
using System;

public interface IView {
  bool Showing { get; }
  bool InputEnabled { get; set; }
}

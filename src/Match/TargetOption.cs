using System;

namespace Match {
public class TargetOption {
  public Type[] Types { get; set; }

  public TargetOption(Type[] types) {
    Types = types;
  }
}
}
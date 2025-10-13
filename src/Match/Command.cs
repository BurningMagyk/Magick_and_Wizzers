using Godot;
using System;
using System.Collections.Generic;
using UI;

namespace Match {
public class Command {
  public enum CommandType {
    // Active commands
    APPROACH, // Enter range of target
    AVOID, // Leave range of target
    INTERACT, // Interact with the target

    // Reactive commands
    INTERCEPT, // Execute command when actor enters range of recipient
    LINGER, // Execute command when actor leaves range of recipient
    BRIDLE, // Execute command when actor interacts with recipient
    
    // Passive commands
    AMBLE, // Move slowly
    SKULK, // Move stealthily
    LAYER // Move through particular layer
  } 

  public enum RangeType {
    DISTANCE,
    DETECTION,
    COMBAT,
    INFLUENCE,
  }

  private readonly int duration; // in hours

  public CommandType Type { get; private set; }
  public Piece Actor { get; private set; }
  public List<Target> targets = [];
  public RangeType Range { get; private set; }
  public int RangeDistance { get; private set; } // in acres, only relevant for RangeType.DISTANCE
  public int Duration { get; private set; } // -1 for indefinite duration
  // For commands that need UI input from multiple views.
  // Will typically just be [ViewStateEnum.DESIGNATE_BOARD] unless it's CommandType.INTERACT.
  public ViewStateEnum[] ViewSteps { get; private set; }
    
  private Command(
    CommandType type,
    RangeType rangeType,
    int rangeDistance,
    int duration
  ) {
    Type = type;
    Range = rangeType;
    RangeDistance = rangeDistance;
    Duration = duration;
    ViewSteps = [ViewStateEnum.DESIGNATE_BOARD]; // should be more variable when when CommandType.INTERACT
  }

  public ViewStateEnum StepView() {
    if (ViewSteps.Length == 0) {
      return ViewStateEnum.NONE;
    }

    ViewStateEnum viewStep = ViewSteps[0];

    ViewStateEnum[] temp = new ViewStateEnum[ViewSteps.Length - 1];
    Array.Copy(ViewSteps, 1, temp, 0, temp.Length);
    ViewSteps = temp;

    return viewStep;
  }

  public bool Feed(Piece piece) {
    targets.Add(new Target(piece));
    // For now, just accept one piece and then consider the command fully fed.
    return true;
  }

  public bool Feed(Tile tile) {
    targets.Add(new Target(tile));
    // For now, just accept one tile and then consider the command fully fed.
    return true;
  }

  public bool Feed(Main.Card card) {
    targets.Add(new Target(card));
    // For now, just accept one card and then consider the command fully fed.
    return true;
  }

  public bool Feed(Activity activity) {
    targets.Add(new Target(activity));
    // For now, just accept one activity and then consider the command fully fed.
    return true;
  }

  public bool Feed(string item) {
    targets.Add(new Target(item));
    // For now, just accept one item and then consider the command fully fed.
    return true;
  }

  public void SetActor(Piece actor) {
    Actor = actor;
  }

  public void Complete() {
    Actor.Command = this;
  }

  public static Command Approach(int rangeDistance, int duration) {
    return new Command(CommandType.APPROACH, RangeType.DISTANCE, rangeDistance, duration);
  }

  public static Command Approach(RangeType rangeType, int duration) {
    return new Command(CommandType.APPROACH, rangeType, 0, duration);
  }

  public static Command Avoid(int rangeDistance, int duration) {
    return new Command(CommandType.AVOID, RangeType.DISTANCE, rangeDistance, duration);
  }

  public static Command Avoid(RangeType rangeType, int duration) {
    return new Command(CommandType.AVOID, rangeType, 0, duration);
  }

  public static Command Interact(int duration = -1) {
    return new Command(CommandType.INTERACT, RangeType.COMBAT, 0, duration);
  }

  public static Command Act(Activity activity) {
    Target target;
    if (activity == null) {
      target = new Target(Target.TargetType.ACTIVITY);
    } else {
      target = new Target(activity);
    }
      return new Command(CommandType.INTERACT, RangeType.INFLUENCE, 0, 0);
  }

  public static Command Intercept(
    Piece secondActor,
    RangeType rangeType,
    int rangeDistance
  ) {
    return new Command(CommandType.INTERCEPT, rangeType, rangeDistance, 0);
  }

  public static Command Linger(
    Piece secondActor,
    RangeType rangeType,
    int rangeDistance
  ) {
    return new Command(CommandType.LINGER, rangeType, rangeDistance, 0);
  }

  public static Command Defend(Piece secondActor) {
    return new Command(CommandType.BRIDLE, RangeType.COMBAT, 0, 0);
  }

  public static Command Bridle(Piece secondActor, Activity activity) {
    Target target;
    if (activity == null) {
      target = new Target(Target.TargetType.ACTIVITY);
    } else {
      target = new Target(activity);
    }
    return new Command(CommandType.BRIDLE, RangeType.INFLUENCE, 0, 0);
  }

  public static Command Amble() {
    return new Command(CommandType.AMBLE, RangeType.DISTANCE, 0, -1);
  }

  public static Command Skulk() {
    return new Command(CommandType.SKULK, RangeType.DISTANCE, 0, -1);
  }

  public static Command Layer() {
    return new Command(CommandType.LAYER, RangeType.DISTANCE, 0, -1);
  }
}
}

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

  public enum RangeEnum { NOT_APPLICABLE, ADJACENT, NEAR, MODERATE, FAR }

  public CommandType Type { get; private set; }
  public Piece Actor { get; private set; }
  public RangeEnum Range { get; private set; }

  public bool IsPrimary { get; set; }

	// For commands that need UI input from multiple views.
	// Will typically just be [ViewStateEnum.DESIGNATE_BOARD] unless it's CommandType.INTERACT.
  public ViewStateEnum[] ViewSteps { get; private set; }

  private readonly List<Target> targets = [];
	
  private Command(CommandType type, Piece actor) : this(type, RangeEnum.NOT_APPLICABLE, actor) { }
  private Command(
	  CommandType type,
	  RangeEnum range,
	  Piece actor
  ) {
	  Type = type;
	  Range = range;
	  Actor = actor;
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

  public void Complete() {
	  Actor.AddCommand(this);
  }

  public Target[] GetTargets() {
	  return [.. targets];
  }

  public bool IsActive() {
    return Type == CommandType.APPROACH ||
           Type == CommandType.AVOID ||
           Type == CommandType.INTERACT;
  }

  public string Describe() {
    String mainDescription = $"{Type} command for {Actor.Name}";
    if (targets.Count == 0) {
      return mainDescription + " with no targets.";
    }
    return mainDescription + " targeting the following: " +
      string.Join(", ", targets);
  }

  public static Command Approach(Piece actor, RangeEnum range) {
	  return new Command(CommandType.APPROACH, range, actor);
  }

  public static Command Avoid(Piece actor, RangeEnum range) {
	  return new Command(CommandType.AVOID, range, actor);
  }

  public static Command Interact(Piece actor) {
	  return new Command(CommandType.INTERACT, actor);
  }

  // public static Command Act(Activity activity) {
  //   Target target;
  //   if (activity == null) {
  //     target = new Target(Target.TargetType.ACTIVITY);
  //   } else {
  //     target = new Target(activity);
  //   }
  //     return new Command(CommandType.INTERACT, RangeType.INFLUENCE, 0, 0);
  // }

  public static Command Intercept(Piece actor, RangeEnum rangeType) {
    return new Command(CommandType.INTERCEPT, rangeType, actor);
  }

  public static Command Linger(Piece actor, RangeEnum range) {
    return new Command(CommandType.LINGER, range, actor);
  }

  // public static Command Defend(Piece secondActor) {
  //   return new Command(CommandType.BRIDLE, RangeType.COMBAT, 0, 0);
  // }

  // public static Command Bridle(Piece secondActor, Activity activity) {
  //   Target target;
  //   if (activity == null) {
  //     target = new Target(Target.TargetType.ACTIVITY);
  //   } else {
  //     target = new Target(activity);
  //   }
  //   return new Command(CommandType.BRIDLE, RangeType.INFLUENCE, 0, 0);
  // }

  public static Command Amble(Piece actor) {
	  return new Command(CommandType.AMBLE, actor);
  }

  public static Command Skulk(Piece actor) {
	  return new Command(CommandType.SKULK, actor);
  }

  public static Command Layer(Piece actor) {
	  return new Command(CommandType.LAYER, actor);
  }
}
}

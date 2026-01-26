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
	  OBSTRUCT, // Get between two targets
	  INTERACT, // Interact with any target

	  // Reactive commands
	  INTERCEPT, // Execute command when target enters range of recipient
	  LINGER, // Execute command when target leaves range of recipient
	  BRIDLE, // Execute command when target interacts with recipient
	
	  // Passive commands
	  AMBLE, // Move slowly
	  SKULK, // Move stealthily
	  LAYER // Move through particular layer
  }

  public enum RangeEnum { NOT_APPLICABLE, ADJACENT, NEAR, MODERATE, FAR }

  public CommandType Type { get; private set; }
  public int MaxTargetCount { get; set; }
  public Piece Actor { get; private set; }
  public RangeEnum Range { get; private set; }
  public Ability SourceAbility { get; set; }
  public bool IsComplete { get; private set; }
  public bool IsRelaxed { get; private set; }

	// For commands that need UI input from multiple views.
	// Will typically just be [ViewStateEnum.DESIGNATE_BOARD] unless it's CommandType.INTERACT.
  public IView.State[] ViewSteps { get; private set; }

  private readonly List<ITarget> targets = [];
  private readonly List<Command> triggers = [];

  private string spec = "";
	
  public Command(
	  CommandType type,
		int maxTargetCount,
		Ability sourceAbility = null
  ) {
	  Type = type;
		MaxTargetCount = maxTargetCount;
		Reset();
  }

  public void Relax() {
		IsRelaxed = true;
		IsComplete = true;
  }

  public bool FeedTarget(ITarget target) {
		targets.Add(target);
		// For now, just accept one target and then consider the command fully fed.
		IsComplete = true;
		return true;
  }

  public bool FeedSpec(string spec) {
		this.spec = spec;
		IsComplete = true;
		return true;
  }

  public bool FeedRange(RangeEnum range) {
		Range = range;
		IsComplete = true;
		return true;
  }

  public bool FeedActor(Piece actor) {
		Actor = actor;
		IsComplete = true;
		return true;
  }

  public void FeedTrigger(Command command) {
		triggers.Add(command);
  }

  public void Reset() {
		Actor = null;
		Range = RangeEnum.NOT_APPLICABLE;
		targets.Clear();
		triggers.Clear();
		// ViewSteps = DetermineViewSteps(out TargetOption[] targetOptions);
		// TargetOptions = targetOptions;
		IsComplete = false;
  }

  public ITarget[] GetTargets() {
	  return [.. targets];
  }

  public bool IsActive() {
	if (IsRelaxed) {
	  return false;
	}
	return Type == CommandType.APPROACH ||
		   Type == CommandType.OBSTRUCT ||
		   Type == CommandType.AVOID ||
		   Type == CommandType.INTERACT;
  }

  // public IView.State[] DetermineViewSteps(out TargetOption[] targetOptions) {
	// 	if (Type == CommandType.APPROACH || Type == CommandType.AVOID) {
	// 		List<IView.State> steps = [];
	// 		List<TargetOption> targetOptionsList = [];
	// 		for (int i = 0; i < MaxTargetCount * 2; i++) {
	// 		steps.Add(IView.State.DESIGNATE_BOARD);
	// 		targetOptionsList.Add(new TargetOption([typeof(Tile), typeof(Piece), typeof(Activity)]));
	// 		}
	// 		targetOptions = [.. targetOptionsList];
	// 		return [.. steps];
	// 	} else if (
	// 		Type == CommandType.OBSTRUCT ||
	// 		Type == CommandType.LINGER ||
	// 		Type == CommandType.INTERCEPT ||
	// 		Type == CommandType.BRIDLE
	// 	) {
	// 		List<IView.State> steps = [];
	// 		List<TargetOption> targetOptionsList = [];
	// 		for (int i = 0; i < MaxTargetCount; i++) {
	// 			steps.Add(IView.State.DESIGNATE_BOARD);
	// 			// The actors.
	// 			targetOptionsList.Add(new TargetOption([typeof(Piece), typeof(Activity)]));
	// 		}
	// 		for (int i = 0; i < MaxTargetCount; i++) {
	// 			steps.Add(IView.State.DESIGNATE_BOARD);
	// 			// The recipients.
	// 			targetOptionsList.Add(new TargetOption([typeof(Tile), typeof(Piece), typeof(Activity)]));
	// 		}
	// 		if (Type != CommandType.OBSTRUCT) {
	// 			for (int i = 0; i < MaxTargetCount; i++) {
	// 				// Choose which other commands will be triggered by this one.
	// 				steps.Add(ViewStateEnum.COMMAND_LIST);
	// 				targetOptionsList.Add(new TargetOption([typeof(Command)]));
	// 			}
	// 		}
	// 		targetOptions = [.. targetOptionsList];
	// 		return [.. steps];
	// 	} else if (Type == CommandType.INTERACT) {
	// 		List<ViewStateEnum> steps = [];
	// 		List<TargetOption> targetOptionsList = [];
	// 		if (SourceAbility == null) {
	// 			// Basic attack.
	// 			for (int i = 0; i < MaxTargetCount; i++) {
	// 				steps.Add(ViewStateEnum.DESIGNATE_BOARD);
	// 				targetOptionsList.Add(new TargetOption([typeof(Tile), typeof(Piece), typeof(Activity)]));
	// 			}
	// 		} else {
	// 			// Ability.
	// 			for (int i = 0; i < SourceAbility.TargetOptions.Length; i++) {
	// 				steps.Add(UI.UI.DetermineViewStateForTargetTypes(SourceAbility.TargetOptions[i].Types));
	// 			}
	// 		}
	// 		if (targetOptionsList.Count == 0 && SourceAbility.TargetOptions.Length != steps.Count) {
	// 			throw new Exception(
	// 						"Mismatch between number of target options and number of view steps for INTERACT command."
	// 			);
	// 		}
	// 		targetOptions = SourceAbility.TargetOptions;
	// 		return [.. steps];
	// 	}
	// 	targetOptions = [];
	// 	return [];
  // }

  public string Describe() {
		if (Type == CommandType.LAYER) {
			return null;
		}

		if (Actor == null) {
			return null;
		}

		string mainDescription = $"{Type} command for {Actor.Name}";
		if (targets.Count == 0) {
			return mainDescription + " with no targets.";
		}
		return mainDescription + " targeting the following: " +
			string.Join(", ", targets);
  }
}
}

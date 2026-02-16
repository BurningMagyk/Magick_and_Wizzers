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
	public enum StatusEnum { PENDING, COMPLETE, READY, RELAXED, DISABLED }

  public CommandType Type { get; private set; }
	public StatusEnum Status { get; private set; } = StatusEnum.PENDING;
  public Ability Sauce { get; private set; } = null;

  public Piece Actor { get; private set; }

  public Vector2I TargetCountRange { get; private set; }

	private readonly List<WizardStep> wizardSteps = [];
  private readonly List<RangedTarget> targets = [];
	
	// For basic stuff. Can have variable target count, but each matches the same spec.
  private Command(
	  CommandType type,
		Piece actor,
		object[] specsArgs,
		int minTargetCount,
		int maxTargetCount = -1
  ) {
	  Type = type;
		Actor = actor;
		// If there are no specsArgs specified here, then we expect zero targets or for wizard steps to be added later.
		if (specsArgs != null && specsArgs.Length > 0) {
			string name = type.ToString() + " command for " + actor.Name + " with " + minTargetCount + " to "
				+ (maxTargetCount == -1 ? "unlimited" : maxTargetCount.ToString()) + " targets";
			wizardSteps.Add(WizardStep.CreateForCommand(name, specsArgs));
		}
		if (maxTargetCount == -1) {
			maxTargetCount = minTargetCount;
		}
		TargetCountRange = new Vector2I(minTargetCount, maxTargetCount);
  }

  // For special stuff. Has specific target count, but each can match different specs.
  private Command(
		CommandType type,
		Piece actor,
		object[][] specsArgsArray
	) : this(type, actor, null, specsArgsArray.Length, specsArgsArray.Length) {
		foreach (object[] specsArgs in specsArgsArray) {
			string name = type.ToString() + " command for " + actor.Name + " with target spec " + string.Join(", ", specsArgs);
			wizardSteps.Add(WizardStep.CreateForCommand(name, specsArgs));
		}
	}

  public void Reset() {
		targets.Clear();
		Status = StatusEnum.PENDING;
	}

  public void Relax() {
		Status = StatusEnum.RELAXED;
  }

	// This method doesn't need to check whether the target matches specs because we expected the wizard step to have
	// done that already.
  public void Feed(object target, RangeEnum range = RangeEnum.NOT_APPLICABLE) {
		// Accept the target.
		if (target is RangedTarget rangedTarget) {
			targets.Add(rangedTarget);
		} else {
			targets.Add(new RangedTarget(target, range));
		}

		// If we've reached the minimum target count, mark as complete.
		if (targets.Count >= TargetCountRange.X) {
			Status = StatusEnum.COMPLETE;
		}
  }

	public bool IsReactive() {
		return Type == CommandType.INTERCEPT || Type == CommandType.LINGER || Type == CommandType.BRIDLE;
	}

	public Command GetTriggeredCommand(Board board) {
		Spacial instigator = (Spacial) targets[0].Target;
		Spacial recipient = (Spacial) targets[1].Target;
		Command triggeredCommand = (Command) targets[2].Target;
		RangeEnum range = targets[1].Range;

		// Check whether the first two targets are in range.
		if (!CanActorSee(instigator, board) || !CanActorSee(recipient, board)) {
			return null;
		}

		if (Type == CommandType.INTERCEPT) {
			if (board.AreInRange(instigator, recipient, range)) {
				// Return the command to be executed.
				return triggeredCommand;
			}
			return null;
		} else if (Type == CommandType.LINGER) {
			if (!board.AreInRange(instigator, recipient, range)) {
				// Return the command to be executed.
				return triggeredCommand;
			}
			return null;
		} else if (Type == CommandType.BRIDLE) {
			// TODO - implement interaction checking.
			return null;
		}

		throw new Exception("Should not be calling this with type \"" + Type.ToString() + "\".");
	}

  // public string Describe() {
	// 	return "";
  // }

	public record RangedTarget(object Target, RangeEnum Range);

	private bool CanActorSee(Spacial target, Board board) {
		// TODO - implement line of sight and stuff.
		// Automatically return true if target is not a Piece.
		return true;
	}

	public static Command CreateApproach(Piece actor) {
		return new Command(CommandType.APPROACH, actor, [typeof(Piece), typeof(Tile)], 1);
	}
	public static Command CreateAvoid(Piece actor, int targetCount = 1) {
		return new Command(CommandType.AVOID, actor, [typeof(Piece), typeof(Tile)], targetCount);
	}
	public static Command CreateObstruct(Piece actor) {
		return new Command(CommandType.OBSTRUCT, actor, [[typeof(Piece)], [typeof(Piece), typeof(Tile)]]);
	}
	public static Command CreateInteract(Piece actor) {
		return new Command(CommandType.INTERACT, actor, [typeof(Piece), typeof(Tile), typeof(Activity)], 1);
	}
	public static Command CreateIntercept(Piece actor) {
		return new Command(CommandType.INTERCEPT, actor, [[typeof(Piece)], [typeof(Piece), typeof(Tile)], [typeof(Command)]]);
	}
	public static Command CreateLinger(Piece actor) {
		return new Command(CommandType.LINGER, actor, [[typeof(Piece)], [typeof(Piece), typeof(Tile)], [typeof(Command)]]);
	}
	public static Command CreateBridle(Piece actor) {
		return new Command(CommandType.BRIDLE, actor, [[typeof(Piece)], [typeof(Piece), typeof(Tile)], [typeof(Command)]]);
	}
	public static Command CreateAmble(Piece actor) {
		return new Command(CommandType.AMBLE, actor, null, 0, 0);
	}
	public static Command CreateSkulk(Piece actor) {
		return new Command(CommandType.SKULK, actor, null, 0, 0);
	}
	public static Command CreateLayer(Piece actor) {
		return new Command(CommandType.LAYER, actor, null, 0, 0);
	}

	public static Command CreateFromType(CommandType type, Piece actor) {
		return type switch {
			CommandType.APPROACH => CreateApproach(actor),
			CommandType.AVOID => CreateAvoid(actor),
			CommandType.OBSTRUCT => CreateObstruct(actor),
			CommandType.INTERACT => CreateInteract(actor),
			CommandType.INTERCEPT => CreateIntercept(actor),
			CommandType.LINGER => CreateLinger(actor),
			CommandType.BRIDLE => CreateBridle(actor),
			CommandType.AMBLE => CreateAmble(actor),
			CommandType.SKULK => CreateSkulk(actor),
			CommandType.LAYER => CreateLayer(actor),
			_ => throw new Exception("Cannot create Command from unknown CommandType: \"" + type.ToString() + "\"."),
		};
	}

	public static int RangeEnumToUnits(RangeEnum range) {
		return range switch {
			RangeEnum.ADJACENT => Board.DIAGONAL_UNITS,
			RangeEnum.NEAR => Board.DIAGONAL_UNITS * 2,
			RangeEnum.MODERATE => Board.DIAGONAL_UNITS * 4,
			RangeEnum.FAR => Board.DIAGONAL_UNITS * 7,
			_ => throw new Exception("Cannot convert RangeEnum to units for unknown RangeEnum: \"" + range.ToString() + "\"."),
		};
	}
}
}

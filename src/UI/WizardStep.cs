using Godot;
using Match;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UI {
public class WizardStep {
  private static readonly IView.State[] VIEW_STATES_WITH_DETAIL = [
	  IView.State.MEANDER_BOARD,
	  IView.State.MEANDER_HAND,
	  IView.State.DESIGNATE_BOARD,
	  IView.State.DESIGNATE_HAND,
	  IView.State.CAST
  ];

  public static readonly WizardStep ROOT = new(null, null, IView.State.MEANDER_BOARD, true);
  public static readonly WizardStep HAND = new(ROOT, SelectionSpecs.ForHand(null), IView.State.MEANDER_HAND);
  public static readonly WizardStep PASS = new(ROOT, SelectionSpecs.ForPass(), IView.State.PASS);
  public static readonly WizardStep SURRENDER = new(
	  ROOT,
	  SelectionSpecs.ForSurrender(),
	  IView.State.SURRENDER
  );
  public static readonly WizardStep THEATER = new(
	  PASS,
	  SelectionSpecs.ForPass(),
	  IView.State.THEATER,
    true
  );

  private readonly WizardStep prevStep;
  private readonly Dictionary<SelectionSpecs, WizardStep> nextSteps = [];
  private readonly bool irreversible;

  public IView.State ViewState { get; private set; }

  public WizardStep Progress(object target, SelectType selectType) {
	  foreach (SelectionSpecs specs in nextSteps.Keys.ToArray()) {
	    if (specs.Matches(target, selectType)) {
		    return nextSteps[specs];
	    }
	  }
	  return null;
  }

  public WizardStep Regress() {
	  return prevStep;
  }

  private WizardStep(
    WizardStep prevStep,
    SelectionSpecs selectionSpecs,
    IView.State viewState,
    bool irreversible = false
  ) {
    // TODO - When we add the method that connects a new step, use this flag to disable the way back.
    this.irreversible = irreversible;

    if (irreversible) {
      this.prevStep = null;
    } else {
      this.prevStep = prevStep;
    }

	  ViewState = viewState;

	  if (prevStep != null) {
		  prevStep.nextSteps[selectionSpecs] = this;
	  }

	  // Automatically make a detail step for certain view states.
	  if (VIEW_STATES_WITH_DETAIL.Contains(viewState)) {
		WizardStep detailStep = new WizardStep(
		  this,
		  SelectionSpecs.ForDetail(),
		  IView.State.DETAIL
		);
	  // Selecting DETAIL from the detail step returns to this step.
	  detailStep.nextSteps[SelectionSpecs.ForDetail()] = this;
	  }
  }

  public enum SelectType {
	  STANDARD, // X button, space key
	  STANDARD_SKIP, // X button + left trigger, space key + shift key
	  HAND, // square button, E key
	  BACK, // circle button, Q key
	  BACK_SKIP, // circle button + left trigger, Q key + shift key
	  DETAIL, // triangle button, F key
	  PASS, // start button, enter key
	  SURRENDER // select button, backspace key
  }

  private class SelectionSpecs {
	private readonly SelectType selectType;
	private readonly TargetOption[] targetOptions;
	private readonly TargetSpecs targetSpecs;

  private static TargetOption GetTargetOptionFor(object target) {
	if (target == null) {
	  return TargetOption.NONE;
	  } else if (target is Tile || target is Display.ITile) {
		  return TargetOption.TILE;
	  } else if (target is Piece || target is Display.Piece) {
		  return TargetOption.PIECE;
	  } else if (target is Activity) {
		  return TargetOption.ACTIVITY;
	  } else if (target is Card || target is Main.Card) {
		  return TargetOption.CARD;
	  } else if (target is Item) {
		  return TargetOption.ITEM;
	  } else {
		  throw new ArgumentException("Target of type \"" + target.GetType().ToString() + "\" is not valid for selection.");
	  }
  }

	private SelectionSpecs(SelectType selectType, TargetOption[] targetOptions, TargetSpecs targetSpecs = null) {
	  this.selectType = selectType;
	  if (targetOptions != null && targetOptions.Length == 0) {
		  throw new ArgumentException("Target options cannot be empty. Make it null if we don't want to check them.");
	  }
	  this.targetOptions = targetOptions;
	  this.targetSpecs = targetSpecs;
	}

	public bool Matches(object target, SelectType selectType) {
	  // The target has an invalid type.
	  TargetOption targetOption = GetTargetOptionFor(target);

	  // The user's select type must be valid for this SelectionSpecs.
	  if (this.selectType != selectType) {
		  return false;
	  }

	  // If there are no options to match with, matching the user's select type is enough.
	  if (targetOptions == null) {
		  return true;
	  }

	  // The target must be a valid option for this SelectionSpecs.
	  if (!targetOptions.Contains(targetOption)) {
		  return false;
	  }

	  // If there are no specs to match with, the previous checks are enough.
	  if (targetSpecs == null) {
		  return true;
	  }

	  // The target must match the specs for this SelectionSpecs.
	  return targetSpecs.Matches(target);
	}

	public static SelectionSpecs ForStandard(TargetOption[] targetOptions, TargetSpecs targetSpecs = null) {
	  return new SelectionSpecs(SelectType.STANDARD, targetOptions, targetSpecs);
	}

	public static SelectionSpecs ForHand(TargetOption[] targetOptions, TargetSpecs targetSpecs = null) {
	  return new SelectionSpecs(SelectType.HAND, targetOptions, targetSpecs);
	}

	public static SelectionSpecs ForSkip(TargetOption[] targetOptions, TargetSpecs targetSpecs = null) {
	  return new SelectionSpecs(SelectType.STANDARD_SKIP, targetOptions, targetSpecs);
	}

	public static SelectionSpecs ForSkip() {
	  return new SelectionSpecs(SelectType.BACK_SKIP, null, null);
	}

	public static SelectionSpecs ForBack() {
	  return new SelectionSpecs(SelectType.BACK, null, null);
	}

	public static SelectionSpecs ForDetail() {
	  TargetOption[] options = [
		  TargetOption.TILE,
		  TargetOption.PIECE,
		  TargetOption.CARD
	  ];
	  TargetSpecs specs = new();
	  return new SelectionSpecs(SelectType.DETAIL, options, specs);
	}

	public static SelectionSpecs ForPass() {
	  return new SelectionSpecs(SelectType.PASS, null, null);
	}

	public static SelectionSpecs ForSurrender() {
	  return new SelectionSpecs(SelectType.SURRENDER, null, null);
	}
  }

  // public static WizardStep Create(WizardStep prevStep, )
}
}

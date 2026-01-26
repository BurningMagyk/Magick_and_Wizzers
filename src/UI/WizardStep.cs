using Godot;
using Match;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
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
	private static readonly IView.State[] VIEW_STATES_IRREVERSIBLE = [
	  IView.State.MEANDER_BOARD,
	  IView.State.THEATER
	];

  public static readonly WizardStep ROOT = new(null, IView.State.MEANDER_BOARD);
  public static readonly WizardStep HAND = new(
	  ROOT, // Get to HAND from ROOT.
	  IView.State.MEANDER_HAND,
		[SelectType.HAND, typeof(Piece)] // User must used HAND key/button on a Piece to get to HAND step.
  );
  public static readonly WizardStep PASS = new(
		ROOT, // Get to PASS from ROOT.
		IView.State.PASS,
		[SelectType.PASS] // User must use PASS key/button to get to PASS step.
	);
  public static readonly WizardStep SURRENDER = new(
	  ROOT, // Get to SURRENDER from ROOT.
	  IView.State.SURRENDER,
		[SelectType.SURRENDER] // User must use SURRENDER key/button to get to SURRENDER step.
  );
  public static readonly WizardStep THEATER = new(
	  PASS, // Get to THEATER from PASS.
	  IView.State.THEATER
  ); // Does not rely on user input to get to THEATER step.

  private readonly Dictionary<Specs, WizardStep> nextSteps = [];

  public IView.State ViewState { get; private set; }

  public WizardStep Progress(object target, SelectType selectType) {
		foreach (KeyValuePair<Specs, WizardStep> entry in nextSteps) {
			if (entry.Key.Matches(target, selectType)) {
				return entry.Value;
			}
		}
	  return null;
  }

  private WizardStep(
	  WizardStep prevStep, // To add this step as a next step of prevStep.
	  IView.State viewState,
	  object[] specsArgs = null // What are the specifications for coming to this step.
  ) {
	  ViewState = viewState;

	  // Automatically make a detail step for certain view states.
	  if (VIEW_STATES_WITH_DETAIL.Contains(viewState)) {
		  WizardStep detailStep = new(
			  this,
			  IView.State.DETAIL,
			  [SelectType.DETAIL, typeof(Card), typeof(Piece), typeof(Tile)]
		  );
		  // Selecting DETAIL from the detail step returns to this step.
		  detailStep.nextSteps[new Specs([SelectType.DETAIL])] = this;
	  }

		if (prevStep == null) {
			return;
		}

		// Automatically set a regression step except for certain view states.
		if (!VIEW_STATES_IRREVERSIBLE.Contains(viewState)) {
		  nextSteps[new Specs([SelectType.BACK])] = prevStep;
		}

		// Set an additional regression step for the detail step.
		if (viewState == IView.State.DETAIL) {
			nextSteps[new Specs([SelectType.DETAIL])] = prevStep;
		}

		prevStep.nextSteps[new Specs(specsArgs)] = this;
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

  private class Specs {
	  private readonly SelectType selectType = SelectType.STANDARD; // The type of selection made by the user.
	  private readonly HashSet<Type> classArgs; // Piece, Tile, Card, Activity, etc.
	  private readonly HashSet<object> statArgs; // Race, Element, Cost, etc.

		public Specs(object[] args) {
			if (args == null || args.Length == 0) {
				classArgs = [];
				statArgs = [];
				return;
			}

			List<Type> classArgsList = [];
			List<object> statArgsList = [];

			foreach (object arg in args) {
				if (arg == null) {
					throw new ArgumentException("Specs arguments cannot be null.");
				} else if (arg is SelectType selectTypeArg) {
					selectType = selectTypeArg;
				} else if (arg is Type typeArg) {
					classArgsList.Add(ToOrthodoxType(typeArg));
				} else {
					Type argType = arg.GetType();
					if (argType.IsEnum && argType.IsNested && argType.DeclaringType == typeof(Main.Stats)) {
						statArgsList.Add(arg);
					} else {
						throw new ArgumentException("Argument of type \"" + argType.ToString() + "\" is not valid for selection.");
					}
				}
			}

			classArgs = [.. classArgsList.Distinct()];
			statArgs = [.. statArgsList.Distinct()];
		}

		public bool Matches(object target, SelectType selectType) {
			if (this.selectType != selectType) {
				return false;
			}

			if (classArgs.Count == 0 && statArgs.Count == 0) {
				return true;
			}

			if (target == null) {
				return false;
			}
			Type targetType = target.GetType();

			if (classArgs.Count > 0) {
				bool classMatch = false;
				foreach (Type classArg in classArgs) {
					//GD.Print("Checking if target type " + ToOrthodoxType(targetType).ToString() + " equals class arg " + classArg.ToString() + ".");
					if (ToOrthodoxType(targetType) == classArg) {
						classMatch = true;
						break;
					}
				}
				if (!classMatch) {
					return false;
				}
			}

			if (statArgs.Count > 0) {
				// TODO - check if target has a Stats member variable, return false if not.
				foreach (object statArg in statArgs) {
					// TODO - check that the Stats object includes every statArg, return false if even one is missing.
				}
			}

			return true;
		}

		private static Type ToOrthodoxType(Type type) {
			if (typeof(Display.Piece).IsAssignableFrom(type) || typeof(Piece).IsAssignableFrom(type)) {
				return typeof(Piece);
			} else if (typeof(Display.ITile).IsAssignableFrom(type) || typeof(Tile).IsAssignableFrom(type)) {
				return typeof(Tile);
			} else if (typeof(Main.Card).IsAssignableFrom(type) || typeof(Card).IsAssignableFrom(type)) {
				return typeof(Main.Card);
			} else if (typeof(Activity).IsAssignableFrom(type)) {
				return typeof(Activity);
			} else if (typeof(Item).IsAssignableFrom(type)) {
				return typeof(Item);
			} else {
				throw new Exception("Unknown type \"" + type.ToString() + "\" for orthodox conversion.");
			}
		}
  }

  // public static WizardStep Create(WizardStep prevStep, )
}
}

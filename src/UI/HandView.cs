using Godot;
using System;
using Match;

namespace UI {
public partial class HandView : CanvasLayer, IView {
  private const int STARTING_CARD_COUNT = 5, MAX_CARD_COUNT = 10;
  private readonly Card[] cardsInHand = new Card[MAX_CARD_COUNT];
  
  public enum HandViewMode { BROWSE, SELECTED }

  public bool Showing { get; private set; }
	public bool InputEnabled { get; set; } = true;
  
  public Tile.PartitionTypeEnum HoverPartition { get; private set; }
  public int CardCountSupposed { get; private set; }

  private PackedScene cardBase;
  private Rect2 viewPortRect;
  private HandViewMode mode = HandViewMode.BROWSE;
  private Card cardInSelection;
  private Vector2 selectionPosition;
  private int hoveredCardIndex;

  // Called when the node enters the scene tree for the first time.
  public override void _Ready() {
	cardBase = ResourceLoader.Load<PackedScene>("res://scenes/card.tscn");

	// Make the base stuff invisible.
	GetNode<Card>("Card").Visible = false;

	hoveredCardIndex = -1;
	HoverPartition = Tile.MAX_PARTITION;
	Hide();
	CardCountSupposed = STARTING_CARD_COUNT;
  }

  // Called every frame. 'delta' is the elapsed time since the previous frame.
  public override void _Process(double delta) { }

  public override void _Input(InputEvent @event) {
		if (!Showing || !InputEnabled) { return; }

		if (Input.IsActionJustPressed("d_left")) {
			HoverCard(Main.DirectionEnum.LEFT);
		}
		if (Input.IsActionJustPressed("d_right")) {
			HoverCard(Main.DirectionEnum.RIGHT);
		}
		if (Input.IsActionJustPressed("select")) {
			SelectHoveredCard(false);
		}
		if (Input.IsActionJustPressed("detail")) {
			SelectHoveredCard(true);
		}
		if (Input.IsActionJustPressed("hand") || Input.IsActionJustPressed("back")) {
			GoBack?.Invoke();
		}
  }

  public delegate bool SelectCardDelegate(Card card, SelectTypeEnum selectTypeEnum);
  public SelectCardDelegate SelectCard;
  public delegate bool GoBackDelegate();
  public GoBackDelegate GoBack;

  public new void Show() {
	  base.Show();

	  for (int i = 0; i < cardsInHand.Length; i++) {
			if (cardsInHand[i] != null) {
				cardsInHand[i].SendToBrowsePosition();
				cardsInHand[i].Visible = true;
			}
	  }

	  // Only draw cards if we're supposed to have a higher amount than current.
	  int cardCountNeedToDraw = CardCountSupposed - GetCardCount();
	  if (cardCountNeedToDraw > 0) {
			for (int i = 0; i < cardCountNeedToDraw; i++) { DrawCard(); }
	  }

	  Showing = true;
	}

  public new void Hide() {
		base.Hide();

		for (int i = 0; i < cardsInHand.Length; i++) {
			if (cardsInHand[i] != null) {
			cardsInHand[i].Visible = false;
			}
		}

		Showing = false;
  }

  public void SetViewPortRect(Rect2 viewPortRect) {
		this.viewPortRect = viewPortRect;

		// Setup where selected card will be positioned.
		selectionPosition = new Vector2(viewPortRect.Size.X * 4 / 5, viewPortRect.Size.Y / 2);
	}

  public void Unhover(bool forgetIndex = true) {
		if (hoveredCardIndex > -1 && cardsInHand[hoveredCardIndex] != null) {
			cardsInHand[hoveredCardIndex].Unhover();
		}
		if (forgetIndex) {
			hoveredCardIndex = -1;
		}
  }

	private void DrawCard() {
		bool drewCard = false;
		for (int i = 0; i < cardsInHand.Length; i++) {
			if (cardsInHand[i] == null && !drewCard) {
			Control drawnCard = cardBase.Instantiate() as Control;
			drawnCard.Visible = true;
			drawnCard.Name = "Card " + i;
			AddChild(drawnCard);
			cardsInHand[i] = GetNode<Card>("Card " + i);
			drewCard = true;
			}
			if (cardsInHand[i] != null) {
			cardsInHand[i].Position = new Vector2(
			viewPortRect.Size.X / 2 + cardsInHand[i].Size.X * (i - CardCountSupposed / 2F),
			viewPortRect.Size.Y / 2
			);
			cardsInHand[i].BrowsePosition = new Vector2(cardsInHand[i].Position.X, cardsInHand[i].Position.Y);
			}
		}
  }

  private void HoverCard(Main.DirectionEnum direction) {
		int hoveredCardIndexPrevious = hoveredCardIndex;

		if (direction == Main.DirectionEnum.NONE || GetCardCount() == 0) {
			// Intend to unhover or no cards in hand.
			Unhover();
			return;
		} else if (direction == Main.DirectionEnum.LEFT) {
			if (hoveredCardIndex == -1 || hoveredCardIndex == GetLeftmostCardIndex()) {
			// Start sleeve at right end.
			hoveredCardIndex = GetRightmostCardIndex();
			} else {
			// Move sleeve to the left.
			hoveredCardIndex = GetRightmostCardIndex(hoveredCardIndex - 1);
			}
			if (hoveredCardIndex == -1) {
			Unhover();
			return;
			}
		} else if (direction == Main.DirectionEnum.RIGHT) {
			if (hoveredCardIndex == -1 || hoveredCardIndex == GetRightmostCardIndex()) {
			// Start sleeve at left end.
			hoveredCardIndex = GetLeftmostCardIndex();
			} else {
			// Move sleeve to the right.
			hoveredCardIndex = GetLeftmostCardIndex(hoveredCardIndex + 1);
			}
			if (hoveredCardIndex == -1) {
			Unhover();
			return;
			}
		}
		
		Card hoveredCard = cardsInHand[hoveredCardIndex];
		if (hoveredCardIndexPrevious > -1) {
			cardsInHand[hoveredCardIndexPrevious].Unhover();
		}

		// Place the sleeve over the hovered card.
		hoveredCard.Hover();
  }

  private void SelectHoveredCard(bool forDetail) {
		if (hoveredCardIndex == -1
			|| hoveredCardIndex >= cardsInHand.Length
			|| cardsInHand[hoveredCardIndex] == null) {
			return;
		}

		// Emits signal to call HandView.Selected, defined in UI class.
		SelectCard?.Invoke(cardsInHand[hoveredCardIndex], forDetail ? SelectTypeEnum.DETAIL : SelectTypeEnum.FINAL);
  }

  public void RemoveHoveredCard() {
		if (hoveredCardIndex == -1
			|| hoveredCardIndex >= cardsInHand.Length
			|| cardsInHand[hoveredCardIndex] == null) {
			return;
		}

		// Remove card from hand.
		cardsInHand[hoveredCardIndex].Visible = false;
		cardsInHand[hoveredCardIndex] = null;
		CardCountSupposed--;
  }

  private int GetLeftmostCardIndex(int startIndex = 0) {
	for (int i = startIndex; i < cardsInHand.Length; i++) {
	  if (cardsInHand[i] != null) {
	  return i;
	  }
	}
	return -1;
	}
	private int GetRightmostCardIndex(int startIndex) {
	for (int i = startIndex; i >= 0; i--) {
	  if (cardsInHand[i] != null) {
	  return i;
	  }
	}
	return -1;
  }
  private int GetRightmostCardIndex() {
	  return GetRightmostCardIndex(cardsInHand.Length - 1);
  }
  
  private int GetCardCount() {
	int count = 0;
	for (int i = 0; i < cardsInHand.Length; i++) {
	  if (cardsInHand[i] != null) {
	  count++;
	  }
	}
	return count;
	}
}
}

using Godot;
using System;

namespace UI {
	public partial class HandView : CanvasLayer {
		private const float CARD_WIDTH = 170, CARD_HEIGHT = 224;
		private const int STARTING_CARD_COUNT = 5, MAX_CARD_COUNT = 10;
		private readonly Card[] cardsInHand = new Card[MAX_CARD_COUNT];

		public bool Showing { get; private set; }
		public Main.Tile.PartitionType HoverPartition { get; private set; }
		public int CardCountSupposed { get; private set; }

		private PackedScene cardBase;
		private Sprite2D cardSleeve;
		private Rect2 viewPortRect;
		private int hoveredCardIndex;

		// Called when the node enters the scene tree for the first time.
		public override void _Ready() {
			cardBase = ResourceLoader.Load<PackedScene>("res://scenes/card.tscn");
			cardSleeve = GetNode<Sprite2D>("Card Sleeve");

			// Make the base stuff invisible.
			GetNode<Card>("Card").Visible = false;

			hoveredCardIndex = -1;
			HoverPartition = Main.Tile.MAX_PARTITION;
			Hide();
			CardCountSupposed = STARTING_CARD_COUNT;
		}

		// Called every frame. 'delta' is the elapsed time since the previous frame.
		public override void _Process(double delta) { }

		public override void _Input(InputEvent @event) {
			if (Showing == false) { return; }

			if (Input.IsActionJustPressed("ui_left")) {
				HoverCard(DirectionEnum.LEFT);
			}
			if (Input.IsActionJustPressed("ui_right")) {
				HoverCard(DirectionEnum.RIGHT);
			}
			if (Input.IsActionJustPressed("ui_select")) {
				PlayCard();
			}
		}

		[Signal]
		public delegate void PlayedEventHandler(Card card);

		public new void Show() {
			base.Show();

			for (int i = 0; i < cardsInHand.Length; i++) {
				if (cardsInHand[i] != null) {
					cardsInHand[i].Visible = true;
				}
			}

			// Only draw cards if we're supposed to have a higher amount than current.
			int cardCountNeedToDraw = CardCountSupposed - GetCardCount();
			if (cardCountNeedToDraw > 0) {
				for (int i = 0; i < cardCountNeedToDraw; i++) { DrawCard(); }
			}

			Unhover();
			Showing = true;
		}

		public new void Hide() {
			base.Hide();

			cardSleeve.Visible = false;
			for (int i = 0; i < cardsInHand.Length; i++) {
				if (cardsInHand[i] != null) {
					cardsInHand[i].Visible = false;
				}
			}

			Showing = false;
		}

		public void SetViewPortRect(Rect2 viewPortRect) {
			this.viewPortRect = viewPortRect;
		}

		private void DrawCard() {
			for (int i = 0; i < cardsInHand.Length; i++) {
				if (cardsInHand[i] == null) {
					Control drawnCard = cardBase.Instantiate() as Control;
					drawnCard.Position = new Vector2(
						CARD_WIDTH * (i + 0.5F),
						(viewPortRect.Size.Y + Main.Tile2D.TILE_SIZE) / 2
					);
					drawnCard.Visible = true;
					drawnCard.Name = "Card " + i;
					AddChild(drawnCard);
					cardsInHand[i] = GetNode<Card>("Card " + i);
					break;
				}
			}
		}

		private void HoverCard(DirectionEnum direction) {
			if (direction == DirectionEnum.NONE || GetCardCount() == 0) {
				// Intend to unhover or no cards in hand.
				Unhover();
				return;
			} else if (direction == DirectionEnum.LEFT) {
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
			} else if (direction == DirectionEnum.RIGHT) {
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

			// Get difference in size between card and sleeve.
			Vector2 sizeDifference = new Vector2(
				cardSleeve.Texture.GetWidth() - CARD_WIDTH,
				cardSleeve.Texture.GetHeight() - CARD_HEIGHT
			);

			// Place the sleeve over the hovered card.
			cardSleeve.Position = hoveredCard.Position - sizeDifference / 2;
			cardSleeve.Visible = true;
		}

		private void Unhover() {
			hoveredCardIndex = -1;
			cardSleeve.Visible = false;
		}

		private void PlayCard() {
			if (hoveredCardIndex == -1
				|| hoveredCardIndex >= cardsInHand.Length
				|| cardsInHand[hoveredCardIndex] == null) {
				return;
			}
			Card card = cardsInHand[hoveredCardIndex] as Card;

			// Remove card from hand.
			cardsInHand[hoveredCardIndex].Visible = false;
			cardsInHand[hoveredCardIndex] = null;

			// Play the card.
			EmitSignal(SignalName.Played, card);

			// Make no card hovered.
			Unhover();
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

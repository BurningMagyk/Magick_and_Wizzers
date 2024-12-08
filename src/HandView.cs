using Godot;
using System;

namespace UI {
	public partial class HandView : Node {
		private const float CARD_WIDTH = 170, CARD_HEIGHT = 224;
		private const int STARTING_CARD_COUNT = 5, MAX_CARD_COUNT = 10;
		private readonly Control[] cardsInHand = new Control[MAX_CARD_COUNT];

		public bool Showing { get; private set; }
		public Main.Tile.PartitionType HoverPartition { get; private set; }

		private PackedScene cardBase;
		private Sprite2D cardSleeve;
		private Rect2 viewPortRect;
		private int cardCountSupposed, hoveredCardIndex;

		// Called when the node enters the scene tree for the first time.
		public override void _Ready() {
			cardBase = ResourceLoader.Load<PackedScene>("res://scenes/card.tscn");
			cardSleeve = GetNode<Sprite2D>("Card Sleeve");

			// Make the base stuff invisible.
			GetNode<Card>("Card").Visible = false;

			hoveredCardIndex = -1;
			HoverPartition = Main.Tile.MAX_PARTITION;
			Hide();
			cardCountSupposed = STARTING_CARD_COUNT;
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
		}

		public void Show() {
			// Count number of cards in hand currently.
			int cardCountCurrent = 0;
			for (int i = 0; i < cardsInHand.Length; i++) {
				if (cardsInHand[i] != null) {
					cardCountCurrent++;
					cardsInHand[i].Visible = true;
				}
			}

			// Only draw cards if we're supposed to have a higher amount than current.
			int cardCountNeedToDraw = cardCountSupposed - cardCountCurrent;
			if (cardCountNeedToDraw > 0) {
				for (int i = 0; i < cardCountNeedToDraw; i++) { DrawCard(); }
			}

			cardSleeve.Visible = true;
			HoverCard(DirectionEnum.NONE);

			Showing = true;
		}

		public void Hide() {
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
						(viewPortRect.Size.Y + Main.Tile.TILE_SIZE) / 2
					);
					drawnCard.Visible = true;
					AddChild(drawnCard);
					cardsInHand[i] = drawnCard;
					break;
				}
			}
		}

		private void HoverCard(DirectionEnum direction) {
			if (direction == DirectionEnum.NONE || cardsInHand[0] == null) {
				// Unhover cards in hand or no cards in hand.
				hoveredCardIndex = -1;
				cardSleeve.Visible = false;
				return;
			} else if (direction == DirectionEnum.LEFT) {
				if (hoveredCardIndex == -1 || hoveredCardIndex == 0) {
					// Start sleeve at right end.
					for (int i = cardsInHand.Length - 1; i >= 0; i--) {
						if (cardsInHand[i] != null) {
							hoveredCardIndex = i;
							break;
						}
					}
				} else {
					// Move sleeve to the left.
					hoveredCardIndex--;
				}
			} else if (direction == DirectionEnum.RIGHT) {
				if (hoveredCardIndex == -1 || hoveredCardIndex == cardsInHand.Length - 1 || cardsInHand[hoveredCardIndex + 1] == null) {
					// Start sleeve at left end.
					hoveredCardIndex = 0;
				} else {
					// Move sleeve to the right.
					hoveredCardIndex++;
				}
			}
		
			Control hoveredCard = cardsInHand[hoveredCardIndex];

			// Place the sleeve over the hovered card.
			cardSleeve.Position = new Vector2(hoveredCard.Position.X - 10, hoveredCard.Position.Y - 10);
			cardSleeve.Visible = true;
		}
	}
}

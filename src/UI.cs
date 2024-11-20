using Godot;
using System;

namespace Main {
	public partial class UI : Node
	{
		private const float CAMERA_SPEED = 5F;
		private const float WIDTH = 170, HEIGHT = 224;
		private const int STARTING_CARD_COUNT = 5, MAX_CARD_COUNT = 10;

		private readonly Control[] cardsInHand = new Control[MAX_CARD_COUNT];
		private PackedScene cardScene;
		private int hoveredCardIndex;

		private Camera2D camera;
		private Sprite2D cardSleeve;
		private Vector2[] joystick = new Vector2[] {
			new Vector2(0, 0),
			new Vector2(0, 0),
		};

		// Called when the node enters the scene tree for the first time.
		public override void _Ready() {
			cardScene = ResourceLoader.Load<PackedScene>("res://scenes/card.tscn");
			camera = GetNode<Camera2D>("Camera");
			cardSleeve = GetNode<Sprite2D>("Hand View/Card Sleeve");

			for (int i = 0; i < STARTING_CARD_COUNT; i++) { DrawCard(); }

			// Make the base stuff invisible.
			GetNode<CanvasLayer>("Hand View").GetNode<Control>("Card").Visible = false;

			HoverCard(DirectionEnum.NONE);
		}

		// Called every frame. 'delta' is the elapsed time since the previous frame.
		public override void _Process(double delta) {
			
		}

		public override void _PhysicsProcess(double delta) {
			camera.Position += joystick[0] * CAMERA_SPEED;
			camera.Align();
		}
		public override void _Input(InputEvent @event) {
			if (Input.IsActionJustPressed("ui_left")) {
				HoverCard(DirectionEnum.LEFT);
			}
			if (Input.IsActionJustPressed("ui_right")) {
				HoverCard(DirectionEnum.RIGHT);
			}

			float horizontalPan = 0, verticalPan = 0;
		// 	if (Input.IsKeyPressed(Key.A)) {
		// 		horizontalPan -= 1;
		// 	}
		// 	if (Input.IsKeyPressed(Key.D)) {
		// 		horizontalPan += 1;
		// 	}
		// 	if (Input.IsKeyPressed(Key.W)) {
		// 		verticalPan -= 1;
		// 	}
		// 	if (Input.IsKeyPressed(Key.S)) {
		// 		verticalPan += 1;
		// 	}
		// 	joystick[0] = new Vector2(horizontalPan, verticalPan);

		// 	if (Input.IsKeyPressed(Key.F)) {
		// 		GD.Print("test");
		// 	}
		}

		private void DrawCard() {
			for (int i = 0; i < cardsInHand.Length; i++) {
				if (cardsInHand[i] == null) {
					Control drawnCard = cardScene.Instantiate() as Control;
					drawnCard.Position = new Vector2(
						WIDTH * (i + 0.5F),
						(camera.GetViewportRect().Size.Y - HEIGHT) / 2
					);
					drawnCard.Visible = true;
					GetNode<CanvasLayer>("Hand View").AddChild(drawnCard);
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

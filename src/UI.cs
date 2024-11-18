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
		private int hoveredCardIndex = -1;

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
				GD.Print("Move card selection to left");
			}
			if (Input.IsActionJustPressed("ui_right")) {
				GD.Print("Move card selection to right");
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

		private void HoverCard(int index) {
			hoveredCardIndex = index;

			// Place the sleeve over the hovered card.
			Control hoveredCard = cardsInHand[hoveredCardIndex];
		}
	}
}

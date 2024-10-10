using Godot;
using System;

namespace Main {
	public partial class UI : Node
	{
		private const float CAMERA_SPEED = 5F;

		private PackedScene cardScene;
		private Control[] cardsInHand = new Control[10];

		private Camera2D camera;
		private Vector2[] joystick = new Vector2[] {
			new Vector2(0, 0),
			new Vector2(0, 0),
		};

		// Called when the node enters the scene tree for the first time.
		public override void _Ready() {
			cardScene = ResourceLoader.Load<PackedScene>("res://scenes/card.tscn");
			camera = GetNode<Camera2D>("Camera");

			DrawCard();
			DrawCard();
			DrawCard();
		}

		// Called every frame. 'delta' is the elapsed time since the previous frame.
		public override void _Process(double delta) {
			
		}

        public override void _PhysicsProcess(double delta) {
            camera.Position += joystick[0] * CAMERA_SPEED;
			camera.Align();
        }

        // public override void _UnhandledInput(InputEvent @event) {
		// 	float horizontalPan = 0, verticalPan = 0;
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
		// }

		private void DrawCard() {
			for (int i = 0; i < cardsInHand.Length; i++) {
				if (cardsInHand[i] == null) {
					Control drawnCard = cardScene.Instantiate() as Control;
					drawnCard.Position = new Vector2(
						drawnCard.Size.X * (i + 0.5F),
						camera.GetViewportRect().Size.Y - drawnCard.Size.Y / 2);
					drawnCard.Visible = true;
					GetNode<CanvasLayer>("Hand View").AddChild(drawnCard);
					cardsInHand[i] = drawnCard;
					GD.Print(GetNode<CanvasLayer>("Hand View"));
					break;
				}
			}
		}
	}
}

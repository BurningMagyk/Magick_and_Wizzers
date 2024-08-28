using Godot;
using System;

namespace Main {
	public partial class UI : Node
	{
		private const float CAMERA_SPEED = 5F;

		private Camera2D camera;
		private Vector2[] joystick = new Vector2[] {
			new Vector2(0, 0),
			new Vector2(0, 0),
		};

		// Called when the node enters the scene tree for the first time.
		public override void _Ready() {
			camera = GetNode<Camera2D>("Camera");
		}

		// Called every frame. 'delta' is the elapsed time since the previous frame.
		public override void _Process(double delta) {
			
		}

        public override void _PhysicsProcess(double delta) {
            camera.Position += joystick[0] * CAMERA_SPEED;
			camera.Align();
        }

        public override void _UnhandledInput(InputEvent @event) {
			float horizontalPan = 0, verticalPan = 0;
			if (Input.IsKeyPressed(Key.A)) {
				horizontalPan -= 1;
			}
			if (Input.IsKeyPressed(Key.D)) {
				horizontalPan += 1;
			}
			if (Input.IsKeyPressed(Key.W)) {
				verticalPan -= 1;
			}
			if (Input.IsKeyPressed(Key.S)) {
				verticalPan += 1;
			}
			joystick[0] = new Vector2(horizontalPan, verticalPan);

			if (Input.IsKeyPressed(Key.F)) {
				GD.Print("test");
			}
		}
	}
}

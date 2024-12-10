using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

using ElementEnum = Main.Stats.ElementEnum;
using ElementGroupEnum = Main.Stats.ElementGroupEnum;
using ClassEnum = Main.Stats.ClassEnum;
using ClassGroupEnum = Main.Stats.ClassGroupEnum;
using RaceEnum = Main.Stats.RaceEnum;
using RaceGroupEnum = Main.Stats.RaceGroupEnum;

namespace Main {
    public partial class Piece : Node2D {
        public Tile Tile { get; set; }
        public Stats Stats { get; set; }

        // Called when the node enters the scene tree for the first time.
        public override void _Ready() {
        }

        // Called every frame. 'delta' is the elapsed time since the previous frame.
        public override void _Process(double delta) {
        }
    }
}


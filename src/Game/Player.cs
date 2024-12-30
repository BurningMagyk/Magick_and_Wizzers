using Godot;
using System;

public class Player {
    public string Name { get; private set; }
    public bool Alive { get; set; }
    public Player(string name) {
        Name = name;
        Alive = true;
    }
}

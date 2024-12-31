using Godot;
using System;

namespace Game {
public class Command {
    public enum CommandType {
        MOVE
    }
    public CommandType Type { get; private set; }
    public Main.DirectionEnum Direction { get; private set; }
    
    private Command(CommandType type, Main.DirectionEnum direction) {
        Type = type;
        Direction = direction;
    }

    public static Command Move(Main.DirectionEnum direction) {
        return new Command(CommandType.MOVE, direction);
    }
}
}

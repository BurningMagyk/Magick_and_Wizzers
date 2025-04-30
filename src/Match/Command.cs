using Godot;
using System;

namespace Match {
public class Command {
    public enum CommandType {
        // Active commands
        TARRY, // Stay put
        APPROACH, // Enter range of target
        AVOID, // Leave range of target
        ATTACK, // Attack the target
        CAST, // Cast a spell

        // Reactive commands
        INTERCEPT, // Execute command when actor enters range of recipient
        LINGER, // Execute command when actor leaves range of recipient
        DEFEND, // Execute command when actor attacks recipient
        BRIDLE, // Execute command when actor casts a spell
        
        // Passive commands
        AMBLE, // Move slowly
        SKULK, // Move stealthily
        SURFACE, // Move to the surface
        FLY, // Move into the air
        SWIM, // Move into the water
        BORE, // Move into the ground
    }

    public enum RangeType {
        DISTANCE,
        DETECTION,
        COMBAT,
        INFLUENCE,
    }

    private readonly int duration; // in hours

    public CommandType Type { get; private set; }
    public Target Target { get; private set; }
    public RangeType Range { get; private set; }
    public int RangeDistance { get; private set; } // in acres, only relevant for DISTANCE
    public int Duration { get; private set; } // -1 for indefinite duration
    
    private Command(
        CommandType type,
        Piece actor,
        Target target,
        RangeType rangeType,
        int rangeDistance,
        int duration
    ) {
        Type = type;
        Target = target;
        Range = rangeType;
        RangeDistance = rangeDistance;
        Duration = duration;
    }

    public static Command Tarry(Piece self, int duration) {
        Target target = new Target(self.Tile);
        return new Command(CommandType.TARRY, null, target, RangeType.DISTANCE, 0, duration);
    }

    public static Command Approach(Target target, int rangeDistance, int duration) {
        return new Command(CommandType.APPROACH, null, target, RangeType.DISTANCE, rangeDistance, duration);
    }

    public static Command Approach(Target target, RangeType rangeType, int duration) {
        return new Command(CommandType.APPROACH, null, target, rangeType, 0, duration);
    }

    public static Command Avoid(Target target, int rangeDistance, int duration) {
        return new Command(CommandType.AVOID, null, target, RangeType.DISTANCE, rangeDistance, duration);
    }

    public static Command Avoid(Target target, RangeType rangeType, int duration) {
        return new Command(CommandType.AVOID, null, target, rangeType, 0, duration);
    }

    public static Command Attack(Target target, int duration = -1) {
        return new Command(CommandType.ATTACK, null, target, RangeType.COMBAT, 0, duration);
    }

    public static Command Cast(Spell spell) {
        Target target;
        if (spell == null) {
            target = new Target(TargetType.SPELL);
        } else {
            target = new Target(spell);
        }
        return new Command(CommandType.CAST, null, target, RangeType.INFLUENCE, 0, 0);
    }

    public static Command Intercept(
        Piece actor,
        Target recipient,
        RangeType rangeType,
        int rangeDistance
    ) {
        return new Command(CommandType.INTERCEPT, actor, recipient, rangeType, rangeDistance, 0);
    }

    public static Command Linger(
        Piece actor,
        Target recipient,
        RangeType rangeType,
        int rangeDistance
    ) {
        return new Command(CommandType.LINGER, actor, recipient, rangeType, rangeDistance, 0);
    }

    public static Command Defend(Piece actor, Target recipient) {
        return new Command(CommandType.DEFEND, actor, recipient, RangeType.COMBAT, 0, 0);
    }

    public static Command Bridle(Piece actor, Spell spell) {
        Target target;
        if (spell == null) {
            target = new Target(TargetType.SPELL);
        } else {
            target = new Target(spell);
        }
        return new Command(CommandType.BRIDLE, actor, target, RangeType.INFLUENCE, 0, 0);
    }

    public static Command Amble() {
        return new Command(CommandType.AMBLE, null, null, RangeType.DISTANCE, 0, -1);
    }

    public static Command Skulk() {
        return new Command(CommandType.SKULK, null, null, RangeType.DISTANCE, 0, -1);
    }

    public static Command Surface() {
        return new Command(CommandType.SURFACE, null, null, RangeType.DISTANCE, 0, -1);
    }

    public static Command Fly() {
        return new Command(CommandType.FLY, null, null, RangeType.DISTANCE, 0, -1);
    }

    public static Command Swim() {
        return new Command(CommandType.SWIM, null, null, RangeType.DISTANCE, 0, -1);
    }

    public static Command Bore() {
        return new Command(CommandType.BORE, null, null, RangeType.DISTANCE, 0, -1);
    }
}
}

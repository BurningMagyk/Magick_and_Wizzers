using Godot;
using Main;
using System;

public partial class Card : Control {
	public enum CardTypeEnum {
		SUMMON, CHARM, MASTERY, TERRAFORM, NORMAL, TRAP
	}

	Label levelLabel, actionPointsLabel, hitPointsLabel;
	ColorRect elementIcon;
	CardTypeEnum cardType;
	Stats stats;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		VBoxContainer foreground = GetNode<VBoxContainer>("Foreground");
		HBoxContainer topContainer = foreground.GetNode<HBoxContainer>("Stats Top");
		levelLabel = topContainer.GetNode<Label>("Level");
		elementIcon = topContainer.GetNode<ColorRect>("Element");
		HBoxContainer bottomContainer = foreground.GetNode<HBoxContainer>("Stats Bottom");
		actionPointsLabel = bottomContainer.GetNode<Label>("Action Points");
		hitPointsLabel = bottomContainer.GetNode<Label>("Hit Points");

		cardType = CardTypeEnum.SUMMON;
		stats = Stats.CreateRandom();

		levelLabel.Text = "Level " + stats.Level.ToString();
		elementIcon.Color = ToColor(stats.ElementGroup);
		actionPointsLabel.Text = stats.MaxActionPoints.ToString();
		hitPointsLabel.Text = stats.MaxHitPoints.ToString();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta) {
	}

	private static Color ToColor(Stats.ElementGroupEnum elementGroup) {
		switch (elementGroup) {
			case Stats.ElementGroupEnum.PURPLE:
				return new Color(0.8f, 0.4f, 0.8f);
			case Stats.ElementGroupEnum.YELLOW:
				return new Color(0.8f, 0.8f, 0.4f);
			case Stats.ElementGroupEnum.CYAN:
				return new Color(0.4f, 0.8f, 0.8f);
			case Stats.ElementGroupEnum.RED:
				return new Color(0.8f, 0.4f, 0.4f);
			case Stats.ElementGroupEnum.BLACK:
				return new Color(0.4f, 0.4f, 0.4f);
			case Stats.ElementGroupEnum.GREEN:
				return new Color(0.4f, 0.8f, 0.4f);
			default:
				return new Color(1.0f, 1.0f, 1.0f);
		}
	}
}

using Godot;
using Main;
using System;

public partial class Card : Control {
  public enum CardTypeEnum {
	SUMMON, CHARM, MASTERY, TERRAFORM, NORMAL, TRAP
  }

  public Stats Stats { get; private set; }
  public Texture2D Illustration { get; private set; }

  private Label levelLabel, actionPointsLabel, hitPointsLabel;
  private ColorRect elementIcon;
  private CardTypeEnum cardType;

  // Called when the node enters the scene tree for the first time.
  public override void _Ready() {
	VBoxContainer foreground = GetNode<VBoxContainer>("Foreground");
	Illustration = foreground.GetNode<TextureRect>("Illustration").Texture;
	HBoxContainer topContainer = foreground.GetNode<HBoxContainer>("Stats Top");
	levelLabel = topContainer.GetNode<Label>("Level");
	elementIcon = topContainer.GetNode<ColorRect>("Element");
	HBoxContainer bottomContainer = foreground.GetNode<HBoxContainer>("Stats Bottom");
	actionPointsLabel = bottomContainer.GetNode<Label>("Action Points");
	hitPointsLabel = bottomContainer.GetNode<Label>("Hit Points");

	cardType = CardTypeEnum.SUMMON;
	Stats = Stats.CreateRandom();

	levelLabel.Text = "Level " + Stats.Level.ToString();
	elementIcon.Color = ToColor(Stats.ElementGroup);
	actionPointsLabel.Text = Stats.MaxActionPoints.ToString();
	hitPointsLabel.Text = Stats.MaxHitPoints.ToString();
  }

  // Called every frame. 'delta' is the elapsed time since the previous frame.
  public override void _Process(double delta) { }

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

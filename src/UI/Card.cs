using Godot;
using Main;
using System;

namespace UI {
public partial class Card : Control {
  public enum CardTypeEnum {
	  SUMMON, CHARM, MASTERY, TERRAFORM, NORMAL, TRAP
  }

  private enum CardColorEnum {
	  GREY, YELLOW, AMBER, ORANGE, SUNSET, RED, PURPLE, INDIGO, BLUE, TEAL, MAGENTA
  }
  private static readonly Color[] CARD_COLORS = [
	  new Color(0.8f, 0.8f, 0.8f),        // Grey
	  new Color(0.851f, 0.827f, 0.118f),  // Yellow
	  new Color(),                        // Amber
	  new Color(),                        // Orange
	  new Color(),                        // Sunset
	  new Color(0.945f, 0.075f, 0.063f),  // Red
	  new Color(0.784f, 0.0f, 0.867f),    // Purple
	  new Color(),                        // Indigo
	  new Color(0.071f, 0.082f, 0.867f),  // Blue
	  new Color(0.075f, 0.71f, 0.62f),    // Teal
	  new Color(0.81f, 0.117f, 0.473f)    // Magenta
  ];

  public Stats Stats { get; private set; }
  public Texture2D Illustration { get; private set; }

  private Label levelLabel, actionPointsLabel, lifePointsLabel;
  private ColorRect elementIcon;
  private CardTypeEnum cardType;
  private Sprite2D sleeve;

  public Vector2 BrowsePosition {
	  private get; set;
  }

  // Called when the node enters the scene tree for the first time.
  public override void _Ready() {
	if (CARD_COLORS[(int) CardColorEnum.ORANGE] == new Color()) {
	  // Setup color stuff.
	  CARD_COLORS[(int) CardColorEnum.ORANGE] = EvenMix(
		CARD_COLORS[(int) CardColorEnum.RED],
		CARD_COLORS[(int) CardColorEnum.YELLOW]
	  );
	  CARD_COLORS[(int) CardColorEnum.AMBER] = EvenMix(
		CARD_COLORS[(int) CardColorEnum.YELLOW],
		CARD_COLORS[(int) CardColorEnum.ORANGE]
	  );
	  CARD_COLORS[(int) CardColorEnum.SUNSET] = EvenMix(
		CARD_COLORS[(int) CardColorEnum.ORANGE],
		CARD_COLORS[(int) CardColorEnum.RED]
	  );
	  CARD_COLORS[(int) CardColorEnum.INDIGO] = EvenMix(
		CARD_COLORS[(int) CardColorEnum.PURPLE],
		CARD_COLORS[(int) CardColorEnum.BLUE]
	  );
	}

	  VBoxContainer foreground = GetNode<VBoxContainer>("Foreground");
	  Illustration = foreground.GetNode<TextureRect>("Illustration").Texture;
	  HBoxContainer topContainer = foreground.GetNode<HBoxContainer>("Stats Top");
	  levelLabel = topContainer.GetNode<Label>("Level");
	  elementIcon = topContainer.GetNode<ColorRect>("Element");
	  HBoxContainer bottomContainer = foreground.GetNode<HBoxContainer>("Stats Bottom");
	  actionPointsLabel = bottomContainer.GetNode<Label>("Action Points");
	  lifePointsLabel = bottomContainer.GetNode<Label>("Life Points");
 
	sleeve = GetNode<Sprite2D>("Sleeve");
	sleeve.Position = Size / 2;
	sleeve.Visible = false;

	  Stats = Stats.CreateRandom();

	if (Stats.IsCreature()) {
	  cardType = CardTypeEnum.SUMMON;
	} else {
	  Random random = new();
	  int cardTypeIndex = random.Next(1, 7);
	  if (cardTypeIndex >= Enum.GetNames(typeof(CardTypeEnum)).Length) {
		cardTypeIndex = Enum.GetNames(typeof(CardTypeEnum)).Length - 1;
	  }
	  cardType = (CardTypeEnum) cardTypeIndex;

	  actionPointsLabel.Visible = false;
	  lifePointsLabel.Visible = false;
	}
	GetNode<ColorRect>("Background").Color = ToColor(cardType, Stats.IsCreature(), Stats.GetAbilityCount());

	  levelLabel.Text = "Level " + Stats.Level.ToString();
	  elementIcon.Color = ToColor(Stats.ElementGroup);
	  actionPointsLabel.Text = Stats.MaxActionPoints.ToString();
	  lifePointsLabel.Text = Stats.MaxLifePoints.ToString();
  }

  // Called every frame. 'delta' is the elapsed time since the previous frame.
  public override void _Process(double delta) { }

  public void Hover() {
	  sleeve.Visible = true;
  }
  public void Unhover() {
	  sleeve.Visible = false;
  }

  public void SendToBrowsePosition() {
	  Position = BrowsePosition;
  }

  private static Color ToColor(Stats.ElementGroupEnum elementGroup) {
	  return elementGroup switch {
		Stats.ElementGroupEnum.PURPLE => new Color(0.8f, 0.4f, 0.8f),
		Stats.ElementGroupEnum.YELLOW => new Color(0.8f, 0.8f, 0.4f),
		Stats.ElementGroupEnum.CYAN => new Color(0.4f, 0.8f, 0.8f),
		Stats.ElementGroupEnum.RED => new Color(0.8f, 0.4f, 0.4f),
		Stats.ElementGroupEnum.BLACK => new Color(0.2f, 0.2f, 0.2f),
		Stats.ElementGroupEnum.GREEN => new Color(0.4f, 0.8f, 0.4f),
		_ => CARD_COLORS[(int) CardColorEnum.GREY] // Should never get this color if working properly.
	  };
  }

  private static Color ToColor(CardTypeEnum cardType, bool isCreature, int abilityCount) {
	  if (cardType == CardTypeEnum.SUMMON) {
		if (isCreature) {
		  return CARD_COLORS[abilityCount];
	  } else {
		return CARD_COLORS[(int) CardColorEnum.TEAL];
	  }
	} else if (
	  cardType == CardTypeEnum.CHARM
	  || cardType == CardTypeEnum.MASTERY
	  || cardType == CardTypeEnum.TERRAFORM
	  || cardType == CardTypeEnum.NORMAL
	) {
	  return CARD_COLORS[(int) CardColorEnum.TEAL];
	} else if (cardType == CardTypeEnum.TRAP) {
	  return CARD_COLORS[(int) CardColorEnum.MAGENTA];
	}

	// Cards should never be grey if working properly.
	return CARD_COLORS[(int) CardColorEnum.GREY];
  }

  private static Color EvenMix(Color a, Color b) {
	  return new Color(
		  (a.R + b.R) / 2,
		  (a.G + b.G) / 2,
		  (a.B + b.B) / 2
	  );
  }
}
}

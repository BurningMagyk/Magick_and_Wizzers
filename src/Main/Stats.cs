using Godot;
using Match;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;

namespace Main {
public class Stats {

  public enum ElementGroupEnum {
	  PURPLE, YELLOW, CYAN, RED, BLACK, GREEN, TEAL, FUCHSIA
  }
  public enum ElementEnum {
	  BLACK, DREAM, UNHOLY,
	  RAY, LIGHTNING, HOLY,
	  WATER, MIST, FROST, POISON,
	  FIRE, LASER, LAVA,
	  DIRT, METAL, WOOD, FLESH,
	  WIND, THUNDER
  }

  public enum ClassEnum {
	  WARRIOR, KNIGHT, ARCHER, RANGER, ROGUE,
	  WIZARD, CLERIC, CLOWN,
	  SMITH, TAILOR, CHEF, FARMER, MASON, MINER, STRUMPET, GOVERNOR
  }
  public enum ClassGroupEnum {
	  FIGHTER, CASTER, TRADER
  }

	public enum StructureEnum {
		NONE, WALL, EQUIPMENT, VEHICLE
	}

  public enum RaceEnum {
	  DRAGON, DINOSAUR, CHIMERA, SPHINX,
	  APE, RODENT, FELINE, CANINE, BEAR, OVID, HORSE, BAT, CETACEAN,
	  SERPENT, LIZARD, TORTOISE, AMPHIBIAN, FISH, CEPHALOPOD,
	  INSECT, SPIDER, CRUSTACEAN, WORM, SLUG, JELLYFISH,
	  SLIME, PLANT, FUNGUS, BACTERIA,
	  MAN, ELF, GNOME, GIANT, HOMUNCULUS, ORC,
	  GHOST, GROTESQUE, FAIRY, ANGEL, AVATAR,
	  HORROR, DEVIL, SKELETON, ZOMBIE, VAMPIRE,
	  ILLUSION, DOLL, GOLEM, MACHINE
  }
  public enum RaceGroupEnum {
	  HUMAN, MAMMAL, FEATHERED, REPTILIAN, AQUATIC, ARTHROPOD, FLORA, SPIRIT, HOLY, UNHOLY, UNDEAD, CONSTRUCT
  }

	private const int MAX_ABILITY_COUNT = 7;

  private ElementEnum[] elements;
  private ElementGroupEnum[] elementGroups;
  private ClassEnum[] classes;
  private ClassGroupEnum[] classGroups;
	private StructureEnum structure;
  private RaceEnum[] races;
  private RaceGroupEnum[] raceGroupsStrong, raceGroupsWeak;
	private Ability[] abilities;

  public string Name { get; private set;}
  public int Level { get; private set; }
	public Command.CommandType[] AvailableCommandTypes { get; private set; }
  public int MaxActionPoints { get; private set; }
  public int MaxLifePoints { get; private set; }
  public ElementGroupEnum ElementGroup { get; private set; }

	// For creatures.
  public Stats(
	  string name,
	  ElementEnum[] elements, ClassEnum[] classes, RaceEnum[] races,
		Command.CommandType[] availableCommandTypes, Ability[] abilities,
	  int maxActionPoints, int maxLifePoints,
		bool isTrap = false
  ) {
	  Name = name;
		if (elements.Distinct().Count() != elements.Length) {
			throw new Exception("Duplicate elements provided.");
		}
	  this.elements = elements;
	  elementGroups = isTrap ?
			[.. DetermineElementGroups(elements), ElementGroupEnum.FUCHSIA] :
			DetermineElementGroups(elements);

		if (classes.Distinct().Count() != classes.Length) {
			throw new Exception("Duplicate classes provided.");
		}
	  this.classes = classes;
	  classGroups = DetermineClassGroups(classes);

		if (races.Distinct().Count() != races.Length) {
			throw new Exception("Duplicate races provided.");
		}
	  this.races = races;
		RaceGroupEnum[][] raceGroups = DetermineRaceGroups(races);
		raceGroupsStrong = raceGroups[0];
		raceGroupsWeak = raceGroups[1];

		if (availableCommandTypes.Length == 0) {
			throw new Exception("Creature must have at least one available command type.");
		}
		AvailableCommandTypes = availableCommandTypes;
		this.abilities = abilities;

	  MaxActionPoints = maxActionPoints;
	  MaxLifePoints = maxLifePoints;

	  Level = DetermineLevel(maxActionPoints, maxLifePoints);
  }

	// For non-creatures.
	public Stats(
		string name,
		ElementEnum[] supernaturalElements, ElementEnum[] naturalElements,
		StructureEnum structure,
		Ability[] abilities,
		bool isTrap = false
	) {
		Name = name;
		if (supernaturalElements.Distinct().Count() != supernaturalElements.Length) {
			throw new Exception("Duplicate supernatural elements provided.");
		}
		if (naturalElements.Distinct().Count() != naturalElements.Length) {
			throw new Exception("Duplicate natural elements provided.");
		}
		for (int i = 0; i < supernaturalElements.Length; i++) {
			if (IsElementNatural((ElementEnum) i)) {
				throw new Exception("Element " + (ElementEnum) i + " is natural but was provided as supernatural.");
			}
		}
		for (int i = 0; i < naturalElements.Length; i++) {
			if (!IsElementNatural((ElementEnum) i)) {
				throw new Exception("Element " + (ElementEnum) i + " is supernatural but was provided as natural.");
			}
		}
		if (supernaturalElements.Contains(ElementEnum.HOLY) && 
				supernaturalElements.Contains(ElementEnum.UNHOLY)
		) {
			throw new Exception("Item cannot have both HOLY and UNHOLY elements.");
		}
		if (!isTrap && supernaturalElements.Length == 0) {
			throw new Exception("Non-trap non-creature items must have at least one supernatural element.");
		}
		elements = [.. supernaturalElements, .. naturalElements];
		elementGroups = isTrap ?
			[.. DetermineElementGroups(elements), ElementGroupEnum.TEAL, ElementGroupEnum.FUCHSIA] :
			[.. DetermineElementGroups(elements), ElementGroupEnum.TEAL];
		this.structure = structure;
		this.abilities = abilities;
	}

	// For non-creatures without a structure.
	public Stats(
		string name,
		ElementEnum[] supernaturalElements, ElementEnum[] naturalElements,
		Ability[] abilities,
		bool isTrap = false
	) : this(
		name,
		supernaturalElements,
		naturalElements,
		StructureEnum.NONE,
		abilities,
		isTrap
	) { }

  public bool IsCreature() {
	  return races != null && races.Length > 0;
  }
	public int GetAbilityCount() {
		return abilities.Length;
  }

  public static Stats CreateRandom() {
	  Random random = new();

		if (random.Next(0, 2) == 0) {
			// 50% (total) chance to be a non-item creature.
			return CreateRandomForCreature(false);
		} else {
			int varChance = random.Next(0, 10);
			if (varChance < 4) {
				// 40% chance to be an item.
				if (varChance == 0) {
					// 10% chance to be an item creature.
					if (random.Next(0, 4) == 0) {
						// 5% chance to be an item creature trap.
						return CreateRandomForCreature(true, true);
					}
					return CreateRandomForCreature(true, false);
				} else if (varChance < 2) {
					// 20% chance to be an equipment.
					if (random.Next(0, 8) == 0) {
						// 2.5% chance to be an equipment trap.
						return CreateRandomForNonCreature(true, true);
					}
					return CreateRandomForNonCreature(true, false);
				} else {
					// 10% chance to be a non-creature, non-equipment item.
					if (random.Next(0, 2) == 0) {
						// 5% chance to be a non-equipment item trap.
						return CreateRandomForNonCreature(false, true);
					}
					return CreateRandomForNonCreature(false, false);
				}
			} else if (varChance < 6) {
				// 20% chance to be a charm.
				if (random.Next(0, 4) == 0) {
					// 5% chance to be a charm trap.
					return CreateRandomForNonCreature(false, true);
				}
				return CreateRandomForNonCreature(false, false);
			} else if (varChance < 9) {
				// 30% chance to be normal.
				if (random.Next(0, 3) == 0) {
					// 10% chance to be a normal trap.
					return CreateRandomForCreature(false, true);
				}
				return CreateRandomForCreature(false, false);
			} else {
				if (random.Next(0, 4) == 0) {
					// 7.5% chance to be a terraform.
					return CreateRandomForNonCreature(false, false);
				} else {
					// 2.5% chance to be a mastery.
					return CreateRandomForNonCreature(false, false);
				}
			}
		}
  }

	public static Stats CreateRandomForCreature(bool isConstruct = false, bool isTrap = false) {
		Random random = new();

		List<ElementEnum> randomElements = [];
		randomElements.Add(GetRandomElement([.. randomElements], !isConstruct));
		if (random.Next(0, 3) == 0) {
			ElementEnum secondRandomElement = GetRandomElement([.. randomElements], !isConstruct);
			if ((randomElements[0] != ElementEnum.HOLY || secondRandomElement != ElementEnum.UNHOLY) &&
					(randomElements[0] != ElementEnum.UNHOLY || secondRandomElement != ElementEnum.HOLY)
			) {
				randomElements.Add(secondRandomElement);
			}
		}

		List<RaceEnum> randomRaces = [];
		randomRaces.Add(
			isConstruct ? GetRandomConstructRace([.. randomRaces]) : GetRandomNonConstructRace([.. randomRaces])
		);
		if (random.Next(0, 3) == 0) {
			randomRaces.Add(
				isConstruct ? GetRandomRace([.. randomRaces]) : GetRandomNonConstructRace([.. randomRaces])
			);
		}

		bool isHuman = false;
		List<ClassEnum> randomClasses = [];
		RaceGroupEnum[][] raceGroupsStrongAndWeak = DetermineRaceGroups([.. randomRaces]);
		foreach (RaceGroupEnum[] raceGroupsArray in raceGroupsStrongAndWeak) {
			foreach (RaceGroupEnum raceGroup in raceGroupsArray) {
				if (raceGroup == RaceGroupEnum.HUMAN) {
					isHuman = true;
					break;
				}
			}
		}
		if (isHuman) {
			int randomVarForClasses = random.Next(0, 5);
			if (randomVarForClasses > 0) {
				randomClasses.Add(GetRandomClass([.. randomClasses]));
				if (randomVarForClasses == 4) {
					randomClasses.Add(GetRandomClass([.. randomClasses]));
				}
			}
		}

		int abilityCount;
		int varChance = random.Next(0, 5 + 4 + 3 + 2 + 1);
		if (varChance < 5) {
			// No abilities.
			abilityCount = 0;
		} else if (varChance < 5 + 4) {
			// 1 ability.
			abilityCount = 1;
		} else if (varChance < 5 + 4 + 3) {
			// 2 abilities.
			abilityCount = 2;
		} else if (varChance < 5 + 4 + 3 + 2) {
			// 3 abilities.
			abilityCount = 3;
		} else {
			// 4 abilities.
			abilityCount = 4;
		}

		int randomActionPoints = random.Next(0, 2500) / 100 * 100;
		int randomHitPoints = random.Next(0, 2500) / 100 * 100;
		int totalPoints = randomActionPoints + randomHitPoints;
		int tooManyPoints = totalPoints - 4000;
		if (tooManyPoints > 0) {
			int subtractFromActionPoints = random.Next(0, tooManyPoints + 1) / 100 * 100;
			randomActionPoints -= subtractFromActionPoints;
			randomHitPoints -= tooManyPoints - subtractFromActionPoints;	
		}

		return new Stats(
			GetRandomName(),
			[.. randomElements],
			[.. randomClasses],
		  [.. randomRaces],
			[
				Command.CommandType.APPROACH,
				Command.CommandType.AVOID//,
				//Command.CommandType.INTERCEPT
			],
			new Ability[abilityCount],
			randomActionPoints,
			randomHitPoints,
			isTrap
		);
	}

	private static Stats CreateRandomForNonCreature(bool isEquipment, bool isTrap = false) {
		Random random = new();

		List<ElementEnum> randomSupernaturalElements = [];
		randomSupernaturalElements.Add(GetRandomElement([.. randomSupernaturalElements], false));
		List<ElementEnum> randomNaturalElements = [];
		if (random.Next(0, 3) == 0) {
			ElementEnum secondRandomElement = GetRandomElement([.. randomSupernaturalElements], false);
			if ((randomSupernaturalElements[0] != ElementEnum.HOLY || secondRandomElement != ElementEnum.UNHOLY) &&
					(randomSupernaturalElements[0] != ElementEnum.UNHOLY || secondRandomElement != ElementEnum.HOLY)
			) {
				if (IsElementNatural(secondRandomElement)) {
					randomNaturalElements.Add(secondRandomElement);
				} else {
					randomSupernaturalElements.Add(secondRandomElement);
				}
			}
		}

		int abilityCount;
		int varChance = random.Next(0, 5 + 4 + 3 + 2 + 1);
		if (varChance < 5) {
			// No abilities.
			abilityCount = 0;
		} else if (varChance < 5 + 4) {
			// 1 ability.
			abilityCount = 1;
		} else if (varChance < 5 + 4 + 3) {
			// 2 abilities.
			abilityCount = 2;
		} else if (varChance < 5 + 4 + 3 + 2) {
			// 3 abilities.
			abilityCount = 3;
		} else {
			// 4 abilities.
			abilityCount = 4;
		}

		return new Stats(
			GetRandomName(),
			[.. randomSupernaturalElements],
			[.. randomNaturalElements],
			isEquipment ? StructureEnum.EQUIPMENT : StructureEnum.NONE,
			new Ability[abilityCount],
			isTrap
		);
	}

	private static ElementEnum GetRandomElement(ElementEnum[] excludingElements, bool allowNatural = true) {
		List<ElementEnum> availableToChoose = [];
		for (int i = 0; i < Enum.GetNames(typeof(ElementEnum)).Length; i++) {
			ElementEnum element = (ElementEnum) i;
			if (excludingElements.Contains(element)) {
				continue;
			}
			if (allowNatural || !IsElementNatural(element)) {
				availableToChoose.Add(element);
			}
		}

		Random random = new();
		return availableToChoose[random.Next(0, availableToChoose.Count)];
	}

	private static ClassEnum GetRandomClass(ClassEnum[] excludingClasses) {
		List<ClassEnum> availableToChoose = [];
		for (int i = 0; i < Enum.GetNames(typeof(ClassEnum)).Length; i++) {
			ClassEnum classEnum = (ClassEnum) i;
			if (excludingClasses.Contains(classEnum)) {
				continue;
			}
			availableToChoose.Add(classEnum);
		}

		Random random = new();
		return availableToChoose[random.Next(0, availableToChoose.Count)];
	}

	private static RaceEnum GetRandomRace(RaceEnum[] excludingRaces) {
		List<RaceEnum> availableToChoose = [];
		for (int i = 0; i < Enum.GetNames(typeof(RaceEnum)).Length; i++) {
			RaceEnum race = (RaceEnum) i;
			if (excludingRaces.Contains(race)) {
				continue;
			}
			availableToChoose.Add(race);
		}

		Random random = new();
		return availableToChoose[random.Next(0, availableToChoose.Count)];
	}

	private static RaceEnum[] sNonConstructRaces = null;
	private static RaceEnum GetRandomNonConstructRace(RaceEnum[] excludingRaces) {
		if (sNonConstructRaces == null) {
			List<RaceEnum> nonConstructRaces = [];
			foreach (RaceEnum race in Enum.GetValues(typeof(RaceEnum))) {
				if (!IsRaceConstruct(race)) {
					nonConstructRaces.Add(race);
				}
			}
			sNonConstructRaces = [.. nonConstructRaces.Distinct()];
		}

		List<RaceEnum> availableToChoose = [];
		foreach (RaceEnum race in sNonConstructRaces) {
			if (excludingRaces.Contains(race)) {
				continue;
			}
			availableToChoose.Add(race);
		}

		Random random = new();
		return availableToChoose[random.Next(0, availableToChoose.Count)];
	}

	private static RaceEnum[] sConstructRaces = null;
	private static RaceEnum GetRandomConstructRace(RaceEnum[] excludingRaces) {
		if (sConstructRaces == null) {
			List<RaceEnum> constructRaces = [];
			foreach (RaceEnum race in Enum.GetValues(typeof(RaceEnum))) {
				if (IsRaceConstruct(race)) {
					constructRaces.Add(race);
				}
			}
			sConstructRaces = [.. constructRaces.Distinct()];
		}

		List<RaceEnum> availableToChoose = [];
		foreach (RaceEnum race in sConstructRaces) {
			if (excludingRaces.Contains(race)) {
				continue;
			}
			availableToChoose.Add(race);
		}

		Random random = new();
		return availableToChoose[random.Next(0, availableToChoose.Count)];
	}

	private static string[] sRandomNames = null;
	private static string GetRandomName() {
		if (sRandomNames == null) {
			string path = "res://resources/frankish_names.txt";
			if (FileAccess.FileExists(path)) {
				using FileAccess file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
				sRandomNames = file.GetAsText().Split("\n");
				for (int i = 0; i < sRandomNames.Length; i++) {
					sRandomNames[i] = sRandomNames[i].Trim();
				}
			} else {
				sRandomNames = ["Default Name"];
			}
		}
		Random random = new();
		return sRandomNames[random.Next(0, sRandomNames.Length)];
	}

  private static int testLevelCalculationFlavorMissCount = 0;
  public static void TestLevelCalculationFlavor() {
	  GD.Print("========== Level Calculation Flavor Test ==========");
	  DetermineLevel(3000, 2500, 8, "Blue-Eyes White Dragon");
	  DetermineLevel(2500, 2100, 7, "Dark Magician");
	  DetermineLevel(2400, 2000, 7, "Red-Eyes Black Dragon");
	  DetermineLevel(2650, 2250, 7, "Skull Knight");
	  DetermineLevel(1200, 800, 3, "Silver Fang");
	  DetermineLevel(800, 2000, 4, "Mystical Elf");
	  DetermineLevel(2300, 2100, 7, "Gaia the Fierce Knight");
	  DetermineLevel(1300, 1400, 4, "Feral Imp");
	  DetermineLevel(1900, 900, 4, "Gemini Elf");
	  DetermineLevel(2000, 1500, 5, "Curse of Dragon");
	  DetermineLevel(1400, 1200, 4, "Celtic Guardian");
	  DetermineLevel(1500, 800, 4, "Blackland Fire Dragon");
	  DetermineLevel(1200, 1500, 4, "Beaver Warrior");
	  DetermineLevel(900, 500, 2, "Laughing Flower");
	  DetermineLevel(800, 1000, 3, "Arlownay");
	  DetermineLevel(2000, 1800, 6, "Rose Spectre of Dunn");
	  DetermineLevel(1200, 2200, 5, "Illusionist Faceless Mage");
	  DetermineLevel(700, 600, 2, "Phantom Dewan");
	  DetermineLevel(1400, 1800, 4, "Hannibal Necromancer");
	  DetermineLevel(2100, 1800, 6, "Flame Cerebrus");
	  DetermineLevel(1200, 1500, 4, "Jellyfish");
	  DetermineLevel(1600, 1300, 5, "Spike Seadra");
	  DetermineLevel(1800, 1500, 5, "Kairyu-Shin");
	  DetermineLevel(1300, 1400, 4, "Harpy Lady");
	  DetermineLevel(2000, 1900, 6, "Monstrous Bird");
	  DetermineLevel(1700, 1200, 5, "Ansatsu");
	  DetermineLevel(1700, 1150, 4, "Axe Raider");
	  DetermineLevel(1650, 1900, 5, "Rock Spirit");
	  DetermineLevel(1400, 700, 3, "The Judgement Hand");
	  DetermineLevel(1400, 1800, 5, "Giant Turtle Who Feeds On Flames");
	  DetermineLevel(2000, 1400, 5, "Patrician of Darkness");
	  DetermineLevel(1900, 1600, 5, "Deepsea Shark");
	  DetermineLevel(1800, 1500, 5, "Garoozis");
	  DetermineLevel(850, 1800, 4, "Blocker");
	  DetermineLevel(1000, 1000, 3, "Ray & Temperature");
	  DetermineLevel(500, 1300, 3, "Wing Egg Elf");
	  DetermineLevel(300, 200, 1, "Dancing Elf");
	  DetermineLevel(1600, 1400, 5, "Doma the Angel of Silence");
	  DetermineLevel(1800, 1700, 5, "Dark Witch");
	  DetermineLevel(1800, 2000, 6, "Gyakutenno Megumi");
	  DetermineLevel(2850, 2350, 8, "Tri-Horned Dragon");
	  DetermineLevel(2900, 2450, 8, "Cosmo Queen");
	  DetermineLevel(2800, 2600, 8, "Magician of Black Chaos");
	  DetermineLevel(1800, 1000, 4, "La Jinn the Mystical Genie of the Lamp");
	  DetermineLevel(4500, 3800, 12, "Blue-Eyes Ultimate Dragon");
	  DetermineLevel(2300, 2000, 7, "Kaiser Dragon");
	  DetermineLevel(2200, 2350, 7, "Makazukinoyaiba");
	  DetermineLevel(2200, 1500, 6, "Judge Man");
	  DetermineLevel(2750, 2500, 8, "Sengenjin");
	  DetermineLevel(800, 400, 2, "Kagemusha of the Blue Flame");
	  DetermineLevel(1100, 1100, 4, "Masaki the Legendary Swordsman");
	  DetermineLevel(1800, 1600, 5, "Rude Kaiser");
	  DetermineLevel(1800, 1500, 5, "Dungeon Worm");
	  DetermineLevel(600, 1500, 3, "Saggi the Dark Clown");
	  DetermineLevel(200, 1800, 3, "Dragon Piper");
	  DetermineLevel(2000, 2300, 7, "Slot Machine");
	  DetermineLevel(2000, 1530, 5, "King of Yamimakai");
	  DetermineLevel(920, 1930, 4, "Castle of Dark Illusions");
	  DetermineLevel(300, 200, 1, "Skull Servant");
	  DetermineLevel(700, 900, 3, "Graveyard and the Hand of Invitation");
	  DetermineLevel(2200, 2000, 6, "Bracchio-radius");
	  DetermineLevel(1600, 1200, 4, "Two-Headed King Rex");
	  DetermineLevel(500, 700, 2, "Basic Insect");
	  DetermineLevel(1500, 2000, 5, "Hercules Beetle");
	  DetermineLevel(1200, 1500, 4, "Big Insect");
	  DetermineLevel(2600, 2500, 8, "Great Moth");
	  DetermineLevel(2450, 2550, 8, "Javelin Beetle");
	  DetermineLevel(2500, 1200, 6, "Summoned Skull");
	  DetermineLevel(700, 600, 2, "Morphing Jar");
	  DetermineLevel(300, 200, 1, "Kuriboh");
	  DetermineLevel(1200, 2000, 4, "Aqua Madoor");
	  DetermineLevel(1800, 1600, 5, "Flame Swordsman");
	  DetermineLevel(1200, 800, 3, "Mammoth Graveyard");
	  DetermineLevel(800, 900, 3, "Dark Gray");
	  DetermineLevel(1300, 900, 4, "Trial of Nightmare");
	  DetermineLevel(1100, 800, 3, "Charubin the Fire Knight");
	  DetermineLevel(800, 700, 3, "Nemuriko");
	  DetermineLevel(600, 900, 3, "Monster Egg");
	  DetermineLevel(1200, 900, 3, "The 13th Grave");
	  DetermineLevel(1500, 1250, 4, "Darkfire Dragon");
	  DetermineLevel(1200, 800, 3, "Dark King of the Abyss");
	  DetermineLevel(1000, 1500, 4, "Turtle Tiger");
	  DetermineLevel(600, 700, 2, "Petit Dragon");
	  DetermineLevel(600, 900, 3, "Petit Angel");
	  DetermineLevel(1000, 800, 3, "Flame Ghost");
	  DetermineLevel(900, 700, 3, "Two-Mouth Darkruler");
	  DetermineLevel(900, 1000, 3, "Dissolverock");
	  DetermineLevel(800, 700, 3, "The Furious Sea King");
	  DetermineLevel(500, 1600, 3, "Green Phantom King");
	  DetermineLevel(800, 1000, 3, "Mystical Sheep #2");
	  DetermineLevel(800, 800, 3, "Kurama");
	  DetermineLevel(1000, 900, 3, "King Fog");
	  DetermineLevel(800, 600, 2, "Fire Eye");
	  DetermineLevel(800, 900, 3, "Synchar");
	  DetermineLevel(900, 500, 2, "Doron");
	  DetermineLevel(900, 700, 3, "Twin Long Rods #1");
	  DetermineLevel(1100, 1000, 4, "One Who Hunts Souls");
	  DetermineLevel(900, 700, 3, "Water Element");
	  DetermineLevel(800, 1200, 3, "Wood Clown");
	  DetermineLevel(900, 700, 3, "Vishwar Randi");
	  DetermineLevel(800, 1000, 3, "Fairywitch");
	  DetermineLevel(900, 900, 3, "Wicked Dragon with the Ersatz Head");
	  DetermineLevel(900, 600, 3, "Megirus Light");
	  DetermineLevel(1400, 1300, 4, "Bean Soldier");
	  DetermineLevel(1100, 800, 3, "Bolt Penguin");
	  DetermineLevel(1200, 1000, 4, "Stone Ghost");
	  DetermineLevel(1100, 1400, 4, "The Statue of Easter Island");
	  DetermineLevel(1100, 700, 3, "Corroding Shark");
	  DetermineLevel(1200, 1000, 4, "Winged Dragon, Gaurdian of the Fortress #2");
	  DetermineLevel(1250, 900, 4, "Wow Warrior");
	  DetermineLevel(950, 700, 3, "Alinsection");
	  DetermineLevel(1100, 700, 3, "Little D");
	  DetermineLevel(1000, 1200, 4, "Wilmee");
	  DetermineLevel(1250, 700, 3, "Oscillo Hero");
	  DetermineLevel(1050, 1200, 4, "Key Mace #2");
	  DetermineLevel(1300, 1100, 4, "Shining Friendship");
	  DetermineLevel(1700, 1400, 5, "Mabarrel");
	  DetermineLevel(1700, 1400, 5, "Akihiron");
	  DetermineLevel(300, 350, 1, "Bat");
	  DetermineLevel(500, 400, 2, "White Dolphin");
	  DetermineLevel(600, 700, 2, "Zarigun");
	  DetermineLevel(650, 500, 2, "Boo Koo");
	  DetermineLevel(750, 400, 2, "Abyss Flower");
	  DetermineLevel(800, 600, 2, "Man-Eating Plant");
	  DetermineLevel(700, 600, 2, "Wings of Wicked Flame");
	  DetermineLevel(900, 800, 3, "Nightmare Scorpion");
	  DetermineLevel(300, 400, 1, "Dark Plant");
	  DetermineLevel(700, 1000, 3, "Battle Warrior");
	  DetermineLevel(800, 700, 3, "The Shadow Who Controls the Dark");
	  DetermineLevel(500, 700, 2, "The Melting Red Shadow");
	  DetermineLevel(500, 800, 2, "Dig Beak");
	  DetermineLevel(400, 200, 1, "Ancient Jar");
	  DetermineLevel(900, 200, 2, "Hurrical");
	  DetermineLevel(800, 500, 2, "Happy Lover");
	  DetermineLevel(500, 400, 2, "Mech Mole Zombie");
	  DetermineLevel(600, 500, 2, "Droll Bird");
	  DetermineLevel(500, 750, 2, "Embryonic Beast");
	  DetermineLevel(1600, 800, 4, "Great White");
	  DetermineLevel(1300, 1100, 4, "Tiger Axe");
	  DetermineLevel(1200, 1000, 4, "The Which Feeds on Life");
	  DetermineLevel(1400, 1200, 4, "Spirit of the Books");
	  DetermineLevel(1200, 1700, 5, "LaMoon");
	  DetermineLevel(900, 1300, 4, "Temple of Skulls");
	  DetermineLevel(1200, 1300, 4, "Rhaimundos of the Red Sword");
	  DetermineLevel(1300, 1300, 4, "Lizark");
	  DetermineLevel(1200, 1400, 4, "Doriado");
	  DetermineLevel(1600, 800, 4, "Beautiful Headhuntress");
	  DetermineLevel(900, 1200, 3, "Wodan the Resident of the Forest");
	  DetermineLevel(800, 600, 2, "Yashinoki");
	  DetermineLevel(1250, 1000, 4, "Kuwagata α");
	  DetermineLevel(1900, 1700, 6, "Skull Bird");
	  DetermineLevel(1900, 1700, 6, "Kwager Hercules");
	  DetermineLevel(1350, 1200, 5, "Leo Wizard");
	  DetermineLevel(1300, 1400, 4, "Mon Larvas");
	  DetermineLevel(900, 1100, 3, "Living Vase");
	  DetermineLevel(850, 900, 3, "Muse-A");
	  DetermineLevel(1100, 1200, 4, "Hourglass of Courage");
	  DetermineLevel(1900, 1700, 6, "Warrior of Tradition");
	  DetermineLevel(1200, 900, 3, "Sonic Maid");
	  DetermineLevel(1450, 1000, 4, "Takuhee");
	  DetermineLevel(2000, 1700, 6, "Dark Magician Girl");
	  DetermineLevel(1400, 700, 3, "The Wicked Worm Beast");
	  DetermineLevel(1200, 1400, 4, "Fiend Kraken");
	  DetermineLevel(300, 1300, 3, "Gorgon Egg");
	  DetermineLevel(1500, 1100, 4, "Faith Bird");
	  DetermineLevel(1300, 1100, 4, "Dark Titan of Terror");
	  DetermineLevel(1800, 1500, 5, "Orion the Battle King");
	  DetermineLevel(250, 200, 1, "Zone Eater");
	  DetermineLevel(250, 250, 1, "Swordsman from a Distant Land");
	  DetermineLevel(1800, 1500, 5, "Emperor of the Land and Sea");
	  DetermineLevel(400, 500, 2, "Karakuri Spider");
	  DetermineLevel(1850, 800, 4, "Mechanicalchaser");
	  DetermineLevel(1800, 1500, 5, "Mech Bass");
	  DetermineLevel(2250, 1900, 6, "Aqua Dragon");
	  DetermineLevel(1800, 800, 4, "Giant Red Seasnake");
	  DetermineLevel(1000, 1300, 4, "Barrel Rock");
	  DetermineLevel(1100, 1300, 4, "Sea Kamen");
	  DetermineLevel(800, 900, 3, "Cockroach Knight");
	  DetermineLevel(1300, 1000, 4, "Brave Scizzar");
	  DetermineLevel(1900, 1700, 6, "Turtle Bird");
	  DetermineLevel(1300, 1800, 5, "Spirit of the Mountain");
	  DetermineLevel(2100, 1300, 5, "Man-eating Black Shark");
	  DetermineLevel(1500, 1300, 4, "Maiden of the Moonlight");
	  DetermineLevel(1400, 1700, 5, "Winged Egg of New Life");
	  DetermineLevel(1800, 1500, 5, "Queen of Autumn Leaves");
	  DetermineLevel(1100, 1200, 4, "Fairy Dragon");
	  DetermineLevel(1600, 1100, 4, "Fairy of the Fountain");
	  DetermineLevel(1300, 1400, 4, "Amazon of the Seas");
	  DetermineLevel(1300, 700, 3, "Gruesome Goo");
	  DetermineLevel(800, 1600, 4, "Toon Alligator");
	  DetermineLevel(1500, 1200, 4, "Alligator's Sword");
	  DetermineLevel(1850, 1700, 6, "Metal Dragon");
	  DetermineLevel(1800, 2000, 6, "Meteor Dragon");
	  DetermineLevel(3500, 2000, 8, "Meteor Black Dragon");
	  DetermineLevel(2400, 2000, 7, "Red Eyes Black Dragon");
	  DetermineLevel(1500, 1200, 4, "The Snake Hair");
	  DetermineLevel(1200, 800, 3, "Wolf");
	  DetermineLevel(1000, 900, 3, "Wetha");
	  DetermineLevel(1600, 0, 3, "Dragon Zombie");
	  DetermineLevel(500, 1100, 3, "Trap Master");
	  DetermineLevel(1400, 1000, 4, "Water Magician");
	  DetermineLevel(1050, 900, 3, "Aqua Snake");
	  DetermineLevel(2200, 2000, 6, "Machine King");
	  DetermineLevel(1800, 1500, 5, "Wing Eagle");
	  DetermineLevel(2200, 1800, 6, "Soul Hunter");
	  DetermineLevel(1250, 2100, 5, "30,000-Year White Turtle");
	  DetermineLevel(1600, 1200, 4, "Crawling Dragon #2");
	  DetermineLevel(1500, 1000, 4, "Solar Flare Dragon");
	  GD.Print("======================= END =======================");
	  GD.Print("Flavor Miss Count: " + testLevelCalculationFlavorMissCount);
  }

  private static int DetermineLevel(int maxActionPoints, int maxHitPoints, int expected = 0, string name = null) {
	  // 8 Misses:
	  // double resultFromPoints = (maxActionPoints * 1.5 + maxHitPoints) / 830;

	  // 7 Misses:
	  // double resultFromPoints = (maxActionPoints * 1.25 + maxHitPoints) / 730;

	  // 5 Misses:
	  // double resultFromPoints = (maxActionPoints * 1.15 + maxHitPoints) / 700;

	  // 5 Misses:
	  // double resultFromPoints = (maxActionPoints * 1.1 + maxHitPoints) / 690;

	  // 2 Misses:
	  // double resultFromPoints = (Math.Pow(maxActionPoints, 0.95) * 1.1 + Math.Pow(maxHitPoints, 0.95)) / 470;

	  // 2 Misses:
	  // double resultFromPoints = (Math.Pow(maxActionPoints, 0.97) * 1.1 + Math.Pow(maxHitPoints, 0.97)) / 545;

	  // 2 Misses:
	  // double resultFromPoints = (Math.Pow(maxActionPoints, 0.95) * 1.05 + Math.Pow(maxHitPoints, 0.95)) / 460;

	  // 15 Misses (new):
	  // double resultFromPoints = (Math.Pow(maxActionPoints, 0.95) * 1.04 + Math.Pow(maxHitPoints, 0.95) * 1.04) / 460;

	  // 12 Misses (new):
	  // double resultFromPoints = (Math.Pow(maxActionPoints, 0.93) * 1.1 + Math.Pow(maxHitPoints, 0.93) * 1.1) / 420;

	  // 6 Misses (new):
	  double resultFromPoints = (Math.Pow(maxActionPoints, 0.91) + Math.Pow(maxHitPoints, 0.91)) / 329.17;

	  int clampedResult = Math.Clamp((int) Math.Round(resultFromPoints), 1, 12);
	  if (expected > 0 && clampedResult != expected) {
			GD.Print((name != null ? (name + " - ") : "") + "Expected: " + expected + ", Actual: " + resultFromPoints);
			testLevelCalculationFlavorMissCount++;
	  }

	  return clampedResult;
  }

  private static ElementGroupEnum[] DetermineElementGroups(ElementEnum[] elements) {
	  List<ElementGroupEnum> groups = new List<ElementGroupEnum>();
	  foreach (ElementEnum element in elements) {
		switch (element) {
			case ElementEnum.BLACK:
			case ElementEnum.DREAM:
			case ElementEnum.UNHOLY:
			  groups.Add(ElementGroupEnum.PURPLE);
			  break;
			case ElementEnum.RAY:
			case ElementEnum.LIGHTNING:
			case ElementEnum.HOLY:
			  groups.Add(ElementGroupEnum.YELLOW);
			  break;
			case ElementEnum.WATER:
			case ElementEnum.MIST:
			case ElementEnum.FROST:
			case ElementEnum.POISON:
			  groups.Add(ElementGroupEnum.CYAN);
			  break;
			case ElementEnum.FIRE:
			case ElementEnum.LASER:
			case ElementEnum.LAVA:
			  groups.Add(ElementGroupEnum.RED);
			  break;
			case ElementEnum.DIRT:
			case ElementEnum.METAL:
			case ElementEnum.WOOD:
			case ElementEnum.FLESH:
			  groups.Add(ElementGroupEnum.BLACK);
			  break;
			case ElementEnum.WIND:
			case ElementEnum.THUNDER:
			  groups.Add(ElementGroupEnum.GREEN);
			  break;
		  }
	  }
	  return [.. groups.Distinct()];
  }

  private static ClassGroupEnum[] DetermineClassGroups(ClassEnum[] classes) {
	  List<ClassGroupEnum> groups = new List<ClassGroupEnum>();
	  foreach (ClassEnum clas in classes) {
		switch (clas) {
				case ClassEnum.WARRIOR:
				case ClassEnum.KNIGHT:
				case ClassEnum.ARCHER:
				case ClassEnum.RANGER:
				case ClassEnum.ROGUE:
		  		groups.Add(ClassGroupEnum.FIGHTER);
		  		break;
				case ClassEnum.WIZARD:
				case ClassEnum.CLERIC:
				case ClassEnum.CLOWN:
					groups.Add(ClassGroupEnum.CASTER);
					break;
				case ClassEnum.SMITH:
				case ClassEnum.TAILOR:
				case ClassEnum.CHEF:
				case ClassEnum.FARMER:
				case ClassEnum.MASON:
				case ClassEnum.MINER:
				case ClassEnum.STRUMPET:
				case ClassEnum.GOVERNOR:
					groups.Add(ClassGroupEnum.TRADER);
					break;
			}
		}
		return [.. groups.Distinct()];
  }

  private static RaceGroupEnum[][] DetermineRaceGroups(RaceEnum[] races) {
		List<RaceGroupEnum> groupsStrong = [];
		List<RaceGroupEnum> groupsWeak = [];
		foreach (RaceEnum race in races) {
			switch (race) {
			case RaceEnum.DRAGON:
				groupsWeak.Add(RaceGroupEnum.REPTILIAN);
				break;
			case RaceEnum.DINOSAUR:
			case RaceEnum.SERPENT:
			case RaceEnum.LIZARD:
			case RaceEnum.TORTOISE:
				groupsStrong.Add(RaceGroupEnum.REPTILIAN);
				break;
			case RaceEnum.CHIMERA:
				// groupsStrong.Add(RaceGroupEnum.ANIMAL);
				break;
			case RaceEnum.SPHINX:
				groupsWeak.Add(RaceGroupEnum.MAMMAL);
				groupsWeak.Add(RaceGroupEnum.HOLY);
				break;
			case RaceEnum.APE:
			case RaceEnum.RODENT:
			case RaceEnum.FELINE:
			case RaceEnum.CANINE:
			case RaceEnum.BEAR:
			case RaceEnum.OVID:
			case RaceEnum.HORSE:
			case RaceEnum.BAT:
			case RaceEnum.CETACEAN:
				groupsStrong.Add(RaceGroupEnum.MAMMAL);
				if (race == RaceEnum.CETACEAN) {
					groupsStrong.Add(RaceGroupEnum.AQUATIC);
				}
				break;
			case RaceEnum.AMPHIBIAN:
			case RaceEnum.SLUG:
			case RaceEnum.SLIME:
				groupsWeak.Add(RaceGroupEnum.AQUATIC);
				if (race == RaceEnum.SLIME) {
					groupsWeak.Add(RaceGroupEnum.FLORA);
				}
				break;
			case RaceEnum.FISH:
			case RaceEnum.CEPHALOPOD:
				groupsStrong.Add(RaceGroupEnum.AQUATIC);
				break;
			case RaceEnum.INSECT:
			case RaceEnum.SPIDER:
			case RaceEnum.CRUSTACEAN:
				groupsStrong.Add(RaceGroupEnum.ARTHROPOD);
				if (race == RaceEnum.CRUSTACEAN) {
					groupsStrong.Add(RaceGroupEnum.AQUATIC);
				}
				break;
			case RaceEnum.FUNGUS:
				groupsWeak.Add(RaceGroupEnum.FLORA);
				break;
			case RaceEnum.PLANT:
				groupsStrong.Add(RaceGroupEnum.FLORA);
				break;
			case RaceEnum.MAN:
				groupsStrong.Add(RaceGroupEnum.HUMAN);
				break;
			case RaceEnum.ELF:
			case RaceEnum.GNOME:
			case RaceEnum.GIANT:
			case RaceEnum.HOMUNCULUS:
			case RaceEnum.ORC:
				groupsWeak.Add(RaceGroupEnum.HUMAN);
				if (race == RaceEnum.HOMUNCULUS) {
					groupsWeak.Add(RaceGroupEnum.CONSTRUCT);
				}
				break;
			case RaceEnum.GHOST:
				groupsStrong.Add(RaceGroupEnum.SPIRIT);
				groupsWeak.Add(RaceGroupEnum.UNDEAD);
				break;
			case RaceEnum.GROTESQUE:
				groupsWeak.Add(RaceGroupEnum.HOLY);
				groupsWeak.Add(RaceGroupEnum.UNHOLY);
				break;
			case RaceEnum.FAIRY:
				groupsWeak.Add(RaceGroupEnum.HOLY);
				groupsWeak.Add(RaceGroupEnum.SPIRIT);
				break;
			case RaceEnum.ANGEL:
				groupsWeak.Add(RaceGroupEnum.SPIRIT);
				groupsStrong.Add(RaceGroupEnum.HOLY);
				break;
			case RaceEnum.AVATAR:
				groupsStrong.Add(RaceGroupEnum.HOLY);
				break;
			case RaceEnum.HORROR:
				groupsStrong.Add(RaceGroupEnum.UNHOLY);
				break;
			case RaceEnum.DEVIL:
				groupsWeak.Add(RaceGroupEnum.SPIRIT);
				groupsStrong.Add(RaceGroupEnum.UNHOLY);
				break;
			case RaceEnum.SKELETON:
			case RaceEnum.ZOMBIE:
			case RaceEnum.VAMPIRE:
				groupsStrong.Add(RaceGroupEnum.UNDEAD);
				groupsStrong.Add(RaceGroupEnum.UNHOLY);
				if (race == RaceEnum.VAMPIRE) {
					groupsWeak.Add(RaceGroupEnum.HUMAN);
				}
				break;
			case RaceEnum.ILLUSION:
				groupsWeak.Add(RaceGroupEnum.SPIRIT);
				groupsWeak.Add(RaceGroupEnum.CONSTRUCT);
				break;
			case RaceEnum.DOLL:
			case RaceEnum.GOLEM:
			case RaceEnum.MACHINE:
				groupsStrong.Add(RaceGroupEnum.CONSTRUCT);
				break;
			}
		}

		groupsStrong.AddRange(DetermineRaceGroupsExtra(groupsStrong));
		groupsWeak.AddRange(DetermineRaceGroupsExtra(groupsWeak));

		if (groupsStrong.Contains(RaceGroupEnum.HUMAN)) {
			if (groupsStrong.Contains(RaceGroupEnum.UNDEAD) || groupsWeak.Contains(RaceGroupEnum.UNDEAD)) {
				groupsStrong.RemoveAll(item => item == RaceGroupEnum.HUMAN);
				groupsWeak.Add(RaceGroupEnum.HUMAN);
			}
		}

		return [
			[.. groupsStrong.Distinct()],
			[.. groupsWeak.Distinct()]
		];
  }

  private static ICollection<RaceGroupEnum> DetermineRaceGroupsExtra(ICollection<RaceGroupEnum> raceGroups) {
		List<RaceGroupEnum> groupsExtra = [];
		foreach (RaceGroupEnum raceGroup in raceGroups) {
			switch (raceGroup) {
				case RaceGroupEnum.MAMMAL:
				case RaceGroupEnum.REPTILIAN:
				case RaceGroupEnum.ARTHROPOD:
					break;
			}
		}
		return [.. groupsExtra.Distinct()];
  }

	private static bool IsElementNatural(ElementEnum element) {
		return element switch {
			ElementEnum.RAY or
			ElementEnum.LIGHTNING or
			ElementEnum.WATER or
			ElementEnum.MIST or
			ElementEnum.FROST or
			ElementEnum.POISON or
			ElementEnum.FIRE or
			ElementEnum.LASER or
			ElementEnum.LAVA or
			ElementEnum.DIRT or
			ElementEnum.METAL or
			ElementEnum.WOOD or
			ElementEnum.FLESH or
			ElementEnum.WIND or
			ElementEnum.THUNDER => true,
			_ => false,
		};
	}

	private static bool IsRaceConstruct(RaceEnum race) {
		return DetermineRaceGroups([race])[0].Contains(RaceGroupEnum.CONSTRUCT);
	}
}
}

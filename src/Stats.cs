using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Main {
	public class Stats {
		public enum ElementEnum {
			BLACK, DREAM, UNHOLY,
			RAY, LIGHTNING, HOLY,
			WATER, MIST, FROST,
			FIRE, LASER, LAVA,
			DIRT, METAL, WOOD, FLESH,
			WIND, THUNDER
		}
		public enum ElementGroupEnum {
			PURPLE, YELLOW, CYAN, RED, BLACK, GREEN
		}

		public enum ClassEnum {
			WARRIOR, KNIGHT, ARCHER, RANGER, ROGUE,
			WIZARD, CLERIC, CLOWN,
			SMITH, TAILOR, CHEF, FARMER, MASON, MINER, STRUMPET, GOVERNOR
		}
		public enum ClassGroupEnum {
			FIGHTER, CASTER, TRADER
		}

		public enum RaceEnum {
			DRAGON, DINOSAUR, CHIMERA, SPHINX,
			APE, RODENT, FELINE, CANINE, BEAR, OVID, HORSE, BAT, CETACEAN,
			REPTILE, AMPHIBIAN, FISH, CEPHALOPOD,
			INSECT, SPIDER, CRUSTACEAN, WORM, SLUG, JELLYFISH,
			SLIME, PLANT, FUNGUS, BACTERIA,
			MAN, ELF, GNOME, GIANT, HOMUNCULUS, ORC,
			GHOST, GROTESQUE, FAIRY, ANGEL, AVATAR,
			HORROR, DEVIL, SKELETON, ZOMBIE, VAMPIRE,
			ILLUSION, DOLL, GOLEM, MACHINE
		}
		public enum RaceGroupEnum {
			HUMAN, ANIMAL, MAMMAL, REPTILIAN, AQUATIC, ARTHROPOD, FLORA, SPIRIT, HOLY, UNHOLY, UNDEAD, CONSTRUCT
		}

		private ElementEnum[] elements;
		private ElementGroupEnum[] elementGroups;
		private ClassEnum[] classes;
		private ClassGroupEnum[] classGroups;
		private RaceEnum[] races;
		private RaceGroupEnum[] raceGroupsStrong, raceGroupsWeak;
		private int maxActionPoints, maxLifePoints;

		public String Name { get; private set;}
		public int Level { get; private set; }
		public int MaxActionPoints { get { return maxActionPoints; } }
		public int MaxLifePoints { get { return maxLifePoints; } }
		public ElementGroupEnum ElementGroup { get { return elementGroups[0]; } }

		public Stats(
			string name,
			ElementEnum[] elements, ClassEnum[] classes, RaceEnum[] races,
			int maxActionPoints, int maxLifePoints) {

			Name = name;
			this.elements = elements;
			elementGroups = DetermineElementGroups(elements);

			this.classes = classes;
			classGroups = DetermineClassGroups(classes);

			this.races = races;
			this.maxActionPoints = maxActionPoints;
			this.maxLifePoints = maxLifePoints;

			Level = DetermineLevel(maxActionPoints, maxLifePoints);
		}

		public static Stats CreateRandom() {
			Random random = new Random();

			ElementEnum[] elements = new ElementEnum[random.Next(1, 2)];
			for (int i = 0; i < elements.Length; i++) {
				elements[i] = (ElementEnum) random.Next(0, Enum.GetNames(typeof(ElementEnum)).Length);
			}

			ClassEnum[] classes = new ClassEnum[random.Next(0, 2)];
			for (int i = 0; i < classes.Length; i++) {
				classes[i] = (ClassEnum) random.Next(0, Enum.GetNames(typeof(ClassEnum)).Length);
			}

			RaceEnum[] races = new RaceEnum[random.Next(1, 2)];
			for (int i = 0; i < races.Length; i++) {
				races[i] = (RaceEnum) random.Next(0, Enum.GetNames(typeof(RaceEnum)).Length);
			}

			int maxActionPoints = random.Next(0, 2500) / 100 * 100,
				maxHitPoints = random.Next(0, 2500) / 100 * 100;

			return new Stats("Random", elements, classes, races, maxActionPoints, maxHitPoints);
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
			DetermineLevel(1250, 700, 4, "Oscillo Hero");
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
			DetermineLevel(1200, 1000, 4, "The Witch Feeds on Life");
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
			double resultFromPoints = (Math.Pow(maxActionPoints, 0.95) * 1.1 + Math.Pow(maxHitPoints, 0.95)) / 470;

			// 2 Misses:
			// double resultFromPoints = (Math.Pow(maxActionPoints, 0.97) * 1.1 + Math.Pow(maxHitPoints, 0.97)) / 545;

			// 2 Misses:
			// double resultFromPoints = (Math.Pow(maxActionPoints, 0.95) * 1.05 + Math.Pow(maxHitPoints, 0.95)) / 460;
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
			return groups.Distinct().ToArray();
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
			return groups.Distinct().ToArray();
		}

		private static RaceGroupEnum[][] DetermineRaceGroups(RaceEnum[] racesStrong, RaceEnum[] racesWeak) {
			List<RaceGroupEnum> groupsStrong = new List<RaceGroupEnum>();
			List<RaceGroupEnum> groupsWeak = new List<RaceGroupEnum>();
			
			RaceGroupEnum[][] determinedStrong = DetermineRaceGroups(racesStrong);
			groupsStrong.AddRange(determinedStrong[0]);
			groupsWeak.AddRange(determinedStrong[1]);

			RaceGroupEnum[][] determinedWeak = DetermineRaceGroups(racesWeak);
			groupsWeak.AddRange(determinedWeak[0]);

			return new RaceGroupEnum[][] {
				groupsStrong.Distinct().ToArray(),
				groupsWeak.Distinct().ToArray()
			};
		}

		private static RaceGroupEnum[][] DetermineRaceGroups(RaceEnum[] races) {
			List<RaceGroupEnum> groupsStrong = new List<RaceGroupEnum>();
			List<RaceGroupEnum> groupsWeak = new List<RaceGroupEnum>();
			foreach (RaceEnum race in races) {
				switch (race) {
					case RaceEnum.DRAGON:
						groupsWeak.Add(RaceGroupEnum.REPTILIAN);
						break;
					case RaceEnum.DINOSAUR:
					case RaceEnum.REPTILE:
						groupsStrong.Add(RaceGroupEnum.REPTILIAN);
						break;
					case RaceEnum.CHIMERA:
						groupsStrong.Add(RaceGroupEnum.ANIMAL);
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
						} else {
							groupsStrong.Add(RaceGroupEnum.ANIMAL);
						}
						break;
					case RaceEnum.FISH:
					case RaceEnum.CEPHALOPOD:
						groupsStrong.Add(RaceGroupEnum.AQUATIC);
						groupsStrong.Add(RaceGroupEnum.ANIMAL);
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

			return new RaceGroupEnum[][] {
				groupsStrong.Distinct().ToArray(),
				groupsWeak.Distinct().ToArray()
			};
		}

		private static ICollection<RaceGroupEnum> DetermineRaceGroupsExtra(ICollection<RaceGroupEnum> raceGroups) {
			List<RaceGroupEnum> groupsExtra = new List<RaceGroupEnum>();
			foreach (RaceGroupEnum raceGroup in raceGroups) {
				switch (raceGroup) {
					case RaceGroupEnum.MAMMAL:
					case RaceGroupEnum.REPTILIAN:
					case RaceGroupEnum.ARTHROPOD:
						groupsExtra.Add(RaceGroupEnum.ANIMAL);
						break;
				}
			}
			return groupsExtra.Distinct().ToArray();
		}
	}
}

using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Main {
    public partial class Creature : Node2D {
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
        private int maxActionPoints, maxHitPoints, actionPoints, hitPoints;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready() {
        }

        // Called every frame. 'delta' is the elapsed time since the previous frame.
        public override void _Process(double delta) {
        }

        public void SetStats(
            string name,
            ElementEnum[] elements, ClassEnum[] classes, RaceEnum[] races,
            int maxActionPoints, int maxHitPoints) {

            Name = name;
            this.elements = elements;
            elementGroups = DetermineElementGroups(elements);

            this.classes = classes;
            classGroups = DetermineClassGroups(classes);

            this.races = races;
            this.maxActionPoints = maxActionPoints;
            this.maxHitPoints = maxHitPoints;
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


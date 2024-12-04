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
            FAUNA, FLORA, MAMMAL, REPTILIAN, AQUATIC, SPIRIT, HOLY, UNHOLY, UNDEAD, CONSTRUCT
        }

        private ElementEnum[] elements;
        private ElementGroupEnum[] elementGroups;
        private ClassEnum[] classes;
        private ClassGroupEnum[] classGroups;
        private RaceEnum[] races;
        private RaceGroupEnum[] raceGroups;
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
    }
}


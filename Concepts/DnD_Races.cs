using QuizCanners.Inspect;
using QuizCanners.Utils;
using System.Collections.Generic;

namespace Dungeons_and_Dragons
{

    public static class CharacterRaceExtensions 
    {
        private static readonly Dictionary<Race, CharacterRace> allClasses = new()
        {
            {Race.Dwarf, new Dwarf() },
            {Race.Elf , new Elf() },
            {Race.Halfling, new Halfling() },
            {Race.Human , new Human() },
            {Race.Dragonborn, new Dragonborn() },
            {Race.Gnome , new Gnome() },
            {Race.Half_Elf, new Half_Elf() },
            {Race.Half_Ork , new Half_Ork() },
            {Race.Tiefling, new Tiefling() },
            {Race.Lineages, new Lineages() },
            {Race.Fairy, new Fairy() },
            {Race.Harengon, new Harengon() },
        };

        public static CharacterRace GetData(this Race race) => allClasses[race];
    } 


    public enum Race { Dwarf, Elf, Halfling, Human, Dragonborn, Gnome, Half_Elf, Half_Ork, Tiefling, Lineages, Fairy, Harengon,  }

    public abstract class CharacterRace
    {
        protected virtual HashSet<Trait> Traits { get; set; } = new HashSet<Trait>();


        public virtual bool this[Trait trait] => Traits.Contains(trait);
        public virtual Goodness FallbackAllignmentGoodness() => Goodness.Neutral;
        public virtual Order FallbackAllignmentOrder() => Order.Neutral;

        public virtual int AbilityScoreRacialBonus(AbilityScore stat, int subRace)
            => AbilityScoreBaseRaceBonus(stat)
            + (TryGetSubraces(out var subs) ? subs.GetAbilityScoreSubRaceBonus(stat, subRace) : 0);

        protected virtual int AbilityScoreBaseRaceBonus(AbilityScore stat) => 0;

        public virtual Proficiency this[AbilityScore stat] => Proficiency.None;


        public GridDistance GetSpeed(SpeedType type, int subrace)
        {
            switch (type) 
            {
                case SpeedType.Walking: 
                case SpeedType.Swimming: 
                case SpeedType.Climbing:

                    var speed = WalkingSpeed;

                    if (TryGetSubraces(out var sub))
                    {
                        speed += sub.GetSubraceWalkingSpeedBonus(subrace);
                    }

                    return (type == SpeedType.Walking) ? speed : speed.Half();

                default: return GridDistance.FromCells(0);
            }
        }
        protected virtual GridDistance WalkingSpeed => GridDistance.FromCells(6);
            
        

        public virtual Proficiency this[Skill skill] => Proficiency.None;

        public virtual bool TryGetSubraces (out SubRaceBase subRaces) 
        {
            subRaces = null;
            return false;
        } 
    }


    public class Dwarf : CharacterRace 
    {
       
        protected override HashSet<Trait> Traits { get; set; } = new HashSet<Trait>()
        {
            Trait.Darkvision,
            Trait.Dwarven_Resilence,
            Trait.Dwarven_Toughtness,
            Trait.Stonecunning
        };


        protected override GridDistance WalkingSpeed => GridDistance.FromCells(5);


        public override Goodness FallbackAllignmentGoodness() => Goodness.Good;
        public override Order FallbackAllignmentOrder() => Order.Lawful;

        protected override int AbilityScoreBaseRaceBonus(AbilityScore stat) => stat == AbilityScore.Constitution ? 2 : 0;         
        
        public override bool TryGetSubraces(out SubRaceBase subRaces)
        {
            subRaces = _subraces;
            return true;
        }
        public enum Subraces { Hill, Mountain }
        private readonly DwarfSubraces _subraces = new();
        public class DwarfSubraces : SubraceGeneric<Subraces>
        {
            protected override int GetAbilityScoreSubRaceBonus(AbilityScore stat, Subraces subRace)
            {
                switch (subRace) 
                {
                    case Subraces.Hill: return stat == AbilityScore.Wisdom ? 1 : 0;
                    case Subraces.Mountain: return stat == AbilityScore.Strength ? 2 : 0;

                    default: UnityEngine.Debug.LogError(QcLog.CaseNotImplemented(subRace, nameof(GetAbilityScoreSubRaceBonus))); return 0;
                }
            }
        }

    }
    public class Elf : CharacterRace 
    {
        protected override int AbilityScoreBaseRaceBonus(AbilityScore stat) => stat switch {
                AbilityScore.Dexterity => 2,
                _ => 0,
            };
        

        public override bool TryGetSubraces(out SubRaceBase subRaces)
        {
            subRaces = _subraces;
            return true;
        }
        public enum Subraces { High, Wood, Dark_Drow }
        private readonly ElfSubraces _subraces = new();
        public class ElfSubraces : SubraceGeneric<Subraces>
        {

            public override GridDistance GetSubraceSpeedBonus(Subraces subRace)
            {
                return subRace switch
                {
                    Subraces.Wood => GridDistance.FromCells(1),
                    _ => GridDistance.FromCells(0),
                };
            }
            protected override int GetAbilityScoreSubRaceBonus(AbilityScore stat, Subraces subRace)
            {
                switch (subRace)
                {
                    case Subraces.High: return stat == AbilityScore.Intelligence ? 1 : 0;
                    case Subraces.Wood: return stat == AbilityScore.Wisdom ? 1 : 0;
                    case Subraces.Dark_Drow: return stat == AbilityScore.Charisma ? 1 : 0;

                    default: UnityEngine.Debug.LogError(QcLog.CaseNotImplemented(subRace, nameof(GetAbilityScoreSubRaceBonus))); return 0;
                }
            }
        }


    }
    public class Halfling : CharacterRace { }
    public class Human : CharacterRace { }
    public class Dragonborn : CharacterRace { }
    public class Gnome : CharacterRace { }
    public class Half_Elf : CharacterRace { }
    public class Half_Ork : CharacterRace { }
    public class Tiefling : CharacterRace { }
    public class Lineages : CharacterRace { }
    public class Fairy : CharacterRace { }
    public class Harengon : CharacterRace { }

    public abstract class SubRaceBase
    {
        public abstract void Inspect(ref int selectedSubrace);

        public abstract int GetAbilityScoreSubRaceBonus(AbilityScore stat, int subRace);

        public abstract GridDistance GetSubraceWalkingSpeedBonus(int subRace); 
    }

    public abstract class SubraceGeneric<T> : SubRaceBase
    {
        public override int GetAbilityScoreSubRaceBonus(AbilityScore stat, int subRace) =>
            GetAbilityScoreSubRaceBonus(stat, QcSharp.ToEnum<T>(subRace));

        public override GridDistance GetSubraceWalkingSpeedBonus(int subRace) => GetSubraceSpeedBonus(QcSharp.ToEnum<T>(subRace));

        public virtual GridDistance GetSubraceSpeedBonus(T subRace) => GridDistance.FromCells(0);

        protected abstract int GetAbilityScoreSubRaceBonus(AbilityScore stat, T subRace);



        public override void Inspect(ref int selectedSubrace)
        {
            "Subrace".PegiLabel(90).Edit_Enum<T>(ref selectedSubrace);
        }

    }

}

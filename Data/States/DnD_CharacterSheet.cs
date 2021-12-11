using QuizCanners.Inspect;
using QuizCanners.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dungeons_and_Dragons
{
    [Serializable]
    public class CharacterSheet : Creature
    {
        [SerializeField] private bool _nameDecided = false;
        [SerializeField] protected NameFallback _nameFallback = new();
        [SerializeField] public CharacterLevel Level = new();
        [SerializeField] protected RaceFallback _raceEnumFallback = new();
        [SerializeField] protected ClassFallback _classEnumFallback = new();
        [SerializeField] internal protected int subRace;
        [SerializeField] internal protected Gender gender;
        [SerializeField] internal protected int subClass;
        [SerializeField] internal ArmorProficiencies ArmorProficiencies = new();
        [SerializeField] internal WeaponProficiencies WeaponProficiencies = new();
        [SerializeField] internal ToolProficiencies ToolProficiencies = new();
        [SerializeField] public Wallet Wallet = new();
        [SerializeField] public FeatsSet Feats = new();

        #region Seeded Fallbacks 

        protected string CharacterName
        {
            get => _nameFallback.Get(() => GetFallback(tables => tables.GetName(this), "Bob"));
            set => _nameFallback.ManualValue = value;
        }

        [Serializable] protected class NameFallback : Fallback.FallbackValueGeneric<string> { }

        protected Race RaceEnum 
        {
            get => _raceEnumFallback.Get(()=> GetFallback(tables => tables.GetRace(this), Race.Human));
            set => _raceEnumFallback.ManualValue = value;
        }
        public CharacterRace RaceData => RaceEnum.GetData();
        [Serializable] protected class RaceFallback : Fallback.FallbackValueGeneric<Race> { }

        protected Class ClassEnum
        {
            get => _classEnumFallback.Get(() => GetFallback(tables => tables.GetClass(this), Class.Fighter));
            set => _classEnumFallback.ManualValue = value;
        }
        public CharacterClass ClassData => ClassEnum.Get();
        [Serializable] protected class ClassFallback : Fallback.FallbackValueGeneric<Class> { }

        #endregion

        protected virtual int MaxScore(AbilityScore score) => 20;

        protected override int GetTotalScore(AbilityScore stat)
        {
            int total = GetAbilityBaseScore(stat) + RaceData.AbilityScoreRacialBonus(stat, subRace);

            switch (stat) 
            {
                case AbilityScore.Strength: 
                    AddOne(Feat.Athlete);
                    AddOne(Feat.HeavilyArmored);
                    AddOne(Feat.HeavyArmorMaster);

                    break;
                case AbilityScore.Dexterity: 
                    AddOne(Feat.LightlyArmored); 
                    break;
                case AbilityScore.Charisma: AddOne(Feat.Actor); break;
                case AbilityScore.Constitution: AddOne(Feat.Durable); break;
                case AbilityScore.Intelligence: AddOne(Feat.KeenMind); AddOne(Feat.Linguist); break;
            }

            void AddOne(Feat feat) 
            {
                if (Feats[feat])
                    total++;
            }

            return Math.Min(MaxScore(stat), total);
        }

        #region Passive Score

        public override bool TrySurprise(RollResult stealthRoll)
        {
            if (Feats[Feat.Alert])
                return false;

            return stealthRoll.Value >= PassiveScore(AbilityScore.Wisdom, Skill.Perception);
        }

        #endregion

        #region Abiltity Check
        public override RollResult RollInitiative()
        {
            return AbilityCheck(AbilityScore.Dexterity) + (Feats[Feat.Alert] ? 5 : 0);
        }
        protected override Proficiency GetProficiency(Skill skill)
        {
            Proficiency prof = RaceData[skill].And(base.GetProficiency(skill));

            return prof;
        }

        #endregion

        #region Saving Throw

        protected override Proficiency SavingThrowProficiency(AbilityScore stat) => RaceData[stat].And(ClassData[stat]).And(base.SavingThrowProficiency(stat));


        #endregion

        #region Combat 
        public override RollInfluence GetInfluenceWhenAttackingMe(bool seeingAttacker)
        {
            if (Feats[Feat.Alert])
                return RollInfluence.None;

            return base.GetInfluenceWhenAttackingMe(seeingAttacker);
        }

        #endregion

        public override bool TryGetConcept<T>(out T value) 
        {
            if (typeof(T) == typeof(Gender)) 
            {
                value = (T)((object)gender);
                return true;
            }

            if (typeof(T) == typeof(Class))
            {
                value = (T)((object)ClassEnum);
                return true;
            }

            if (typeof(T) == typeof(Race))
            {
                value = (T)((object)RaceEnum);
                return true;
            }

            return base.TryGetConcept(out value);
        }


        [Serializable]
        private class AbilityScoresFallback : IPEGI
        {
            [NonSerialized] private Gate.Integer _nameHasGate = new Gate.Integer();
            [NonSerialized] private List<int> _rolledStats;
            [SerializeField] public bool UseStandardArray;

            public int GetRandomBySeed (AbilityScore stat, int seed, CharacterClass cls)
            {
                if (_nameHasGate.TryChange(seed))
                {
                    var tmp = new int[6];
                    using (QcMath.RandomBySeedDisposable(seed))
                    {
                        for (int i = 0; i < 6; i++)
                        {
                            tmp[i] = Dice.D6.Roll(4, drop: Drop.Lowest).Value;
                        }
                    }

                    Array.Sort(tmp);

                    _rolledStats = new List<int>();
                    for (int i=0; i<6; i++) 
                    {
                        var score = (AbilityScore)i;
                        _rolledStats.Add(tmp[5-cls.GetStatPrioroty(score)]);
                    }
                }

                return _rolledStats[(int)stat];
            }

            public void Inspect()
            {
                "Use Standard Array".PegiLabel().ToggleIcon(ref UseStandardArray).Nl();

                "Random Fallback by Name + Class + Race + Gender".PegiLabel(pegi.Styles.ListLabel);

                if (Icon.Refresh.Click().Nl())
                    _nameHasGate = new Gate.Integer();

                if (_rolledStats.IsNullOrEmpty() == false && _rolledStats.Count >= 6) {
                    for (int i = 0; i < 6; i++)
                    {
                        var score = (AbilityScore)i;
                        var val = _rolledStats[i];
                        score.ToString().PegiLabel().Edit(ref val).OnChanged(() => _rolledStats[i] = val).Nl();
                    }
                }
            }
        }

        private AbilityScoresFallback _abilityBySeed = new AbilityScoresFallback();

        internal override int GetDefaultValue(AbilityScore stat)
        {
            if (_abilityBySeed.UseStandardArray)
                return ClassData.GetStandardArrayBaseScoreFor(stat);
            else
            {
                var seed = CharacterName.GetHashCode() + ((int)RaceEnum) * 123 + ((int)ClassEnum) * 456 + ((int)gender)*789 ;
                return _abilityBySeed.GetRandomBySeed(stat, seed, ClassData);
            }
        }
        protected override int ProficiencyBonus => Level.ProficiencyBonus;

        public override bool this[Trait trait] => base[trait] || RaceData[trait];

        public bool this[Feature feature] => ClassData.HasFeature(feature, classLevel: Level, subClass: subClass);

        public override GridDistance this[SpeedType type] => RaceData.GetSpeed(type, subRace);

        public override Size Size => Size.Medium;

        public override int ArmorClass => 10 + this[AbilityScore.Dexterity] + (this[Feature.UnarmoredDefense] ? this[AbilityScore.Constitution] : 0);

        public override int MaxHitPoints
        {
            get
            {
                var hitDice = ClassData.HitDie;
                var mod = this[AbilityScore.Constitution];

                return hitDice.MaxRoll() + hitDice.AvargeRoll() * (Level - 1) + mod * Level;
            }
        }

        private Attack GetUnarmedStrike()
        {
            List<Dice> hitDie = new();

            if (Feats[Feat.TavernBrawler])
                hitDie.Add(Dice.D4);

            return new("Unarmed Srike", 
                isRange: false, 
                attackBonus: ProficiencyBonus + this[AbilityScore.Strength], 
                new Damage() 
                {
                    DamageBonus = 1 + this[AbilityScore.Strength],
                    DamageDice = hitDie,
                    DamageType = DamageType.Bludgeoning
                });
        }

        public override List<Attack> GetAttacks()
        {
            var lst = new List<Attack>
            {
                GetUnarmedStrike()
            };
            return lst;
        }

        #region Inspector

        public override string NameForInspector
        {
            get => CharacterName;
            set => CharacterName = value;
        }

        public static CharacterSheet inspected;

        private readonly pegi.EnterExitContext _otherProfiianciesContext = new();

        protected override void InspectAbilities()
        {
            base.InspectAbilities();

            pegi.Nl();

            _abilityBySeed.Nested_Inspect();
        }

        protected override void Inspect_StatBlock_Header()
        {
            ToString().PegiLabel(style: pegi.Styles.HeaderText).Nl();
        }

        protected override void Inspect_Contextual()
        {
            inspected = this;

            if (enterExitContext.IsAnyEntered == false) 
            {
                var r = RaceEnum;
                "Race".PegiLabel(50).Edit_Enum(ref r).OnChanged(()=> RaceEnum = r);

                if (RaceData.TryGetSubraces(out var sub))
                    sub.Inspect(ref subRace);

                pegi.Nl();

                var cl = ClassEnum;
                "Class".PegiLabel(50).Edit_Enum(ref cl).OnChanged(()=> ClassEnum = cl);

                if (ClassData.TryGetSubClass(out var subC))
                    subC.Inspect(ref subClass);

                pegi.Nl();

                "Level".PegiLabel(60).Write();
                Level.Nested_Inspect(fromNewLine: false).Nl();

                "Gender".PegiLabel(60).Edit_Enum(ref gender).Nl();

                if (_nameFallback.IsSet)
                    "Use Fallback name".PegiLabel(toolTip: "Use default Name instead").ClickConfirm(confirmationTag: "ClearName").Nl()
                        .OnChanged(() => _nameFallback.IsSet = false);
                   
            }

            if ("Features".PegiLabel().IsEntered().Nl())
            {
                Feature[] all = (Feature[])Enum.GetValues(typeof(Feature));

                foreach (Feature f in all)
                {
                    if (this[f])
                        f.GetNameForInspector().PegiLabel().Nl();
                }
            }

            "Feats".PegiLabel().Enter_Inspect(Feats).Nl();

            "Wallet".PegiLabel().Enter_Inspect(Wallet).Nl();

            if ("Other Proficiencies".PegiLabel().IsEntered().Nl())
            {
                using (_otherProfiianciesContext.StartContext())
                {
                    ArmorProficiencies.Enter_Inspect().Nl();
                    WeaponProficiencies.Enter_Inspect().Nl();
                    ToolProficiencies.Enter_Inspect().Nl();
                }
            }
        }

        public override void InspectInList(ref int edited, int ind)
        {
            "{0} {1}".F(RaceEnum, ClassEnum).PegiLabel(120).Write();

            if (!_nameDecided && _nameFallback.IsSet)
            {
                Icon.Done.Click(() => _nameDecided = true, "Use This Name");
                Icon.Clear.Click(() => _nameFallback.IsSet = false, "Use Default Name");
            }
            base.InspectInList(ref edited, ind);
        }

        public override string ToString() => "{0} a {1} {2} lvl {3} {4}".F(
            NameForInspector, 
            RaceData, 
            ClassData, 
            Level.Value,
            ClassData.TryGetSubClass(out var subC) ? " ({0})".F(subC.GetNameForInspector()) : ""
            );

        public override System.Collections.IEnumerator SearchKeywordsEnumerator()
        {
            yield return base.SearchKeywordsEnumerator();

            yield return ClassData;
            yield return RaceData;
        }


        #endregion

        [Serializable]
        public class SmartId : DnD_SmartId<CharacterSheet>
        {
            protected override Dictionary<string, CharacterSheet> GetEnities()
            {
                var d = Data;
                if (d)
                    return d.Characters;

                return null;
            }
            public SmartId() { }
            public SmartId(CharacterSheet sheet) 
            {
                SetEntity(sheet);
            }
        }
    }

    [Serializable] public class CharactersDictionary: SerializableDictionary<string, CharacterSheet> { }
}

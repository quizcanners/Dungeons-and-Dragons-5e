using QuizCanners.Inspect;
using QuizCanners.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dungeons_and_Dragons
{
    [Serializable]
    public abstract class Creature : IPEGI, IGotName, IPEGI_ListInspect, IConceptValueProvider, ISearchable
    {
        protected const int STATS_COUNT = 6;

        [SerializeField] public RanDndSeed Seed = new();
        [SerializeField] protected string _name;

        [SerializeField] public Allignment Allignment = new();
        [SerializeField] protected AbilityScores Stats = new();
        [SerializeField] public List<Language> LanguagesKnown = new() { Language.Common };
        [SerializeField] protected SkillSet skillSet = new();
        [SerializeField] protected SavingThrowProficiencies savingThrowProficiencies = new();
        [SerializeField] private SensesDictionary senses = new();


        protected static T GetFallback<T>(Func<SeededFallacks, T> getter, T defaultValue)
        {
            return Singleton.TryGetValue<Singleton_DnD, T>(valueGetter: s => getter(s.Fallbacks), defaultValue: defaultValue);
        }

        public abstract GridDistance this[SpeedType type] { get; }
        public bool this[Language lang] 
        {
            get => LanguagesKnown.Contains(lang);
            set 
            {
                var contains = LanguagesKnown.Contains(lang);
                if (value && !contains) 
                {
                    LanguagesKnown.Add(lang);
                } else if (!value && contains) 
                {
                    LanguagesKnown.Remove(lang);
                }    
            }
        }
        public abstract Size Size { get; }
        public GridDistance this [Sense sense]  => senses.TryGet(sense);
        protected int this[Proficiency prof]
        {
            get
            {
                switch (prof)
                {
                    case Proficiency.None: return 0;
                    case Proficiency.Normal: return ProficiencyBonus;
                    case Proficiency.Expertiese: return ProficiencyBonus * 2;
                    default:
                        Debug.LogError(QcLog.CaseNotImplemented(prof,  "Creature")); return 0;
                }
            }
        }
        protected abstract int ProficiencyBonus { get; }
        public abstract int ArmorClass { get; }
       
        public abstract int MaxHitPoints { get; }

        public virtual bool this[Trait trait] => false;

        internal int this[AbilityScore stat] => Mathf.FloorToInt((GetTotalScore(stat) - 10) / 2f);

        #region Ability Scores
        protected virtual int GetTotalScore(AbilityScore stat) => GetAbilityBaseScore(stat);
        protected int GetAbilityBaseScore(AbilityScore stat)
        {
            if (Stats.TryGetValue(stat, out var st))
                return st.Value;
            else
                return GetDefaultValue(stat);
        }
        internal abstract int GetDefaultValue(AbilityScore stat);
        #endregion

        #region Passive Score

        protected virtual int PassiveScore(AbilityScore score, Skill skill)
        {
            return 10 + this[score] + this[GetProficiency(skill)];
        }

        protected virtual int PassiveScore(AbilityScore score) 
        {
            return 10 + this[score];
        }

        public virtual RollInfluence GetInfluenceWhenAttackingMe(bool seeingAttacker) 
        {
            return seeingAttacker ? RollInfluence.None : RollInfluence.Advantage;
        }

        public virtual bool TrySurprise(RollResult stealthRoll) => stealthRoll.Value >= PassiveScore(AbilityScore.Wisdom, Skill.Perception);


        #endregion

        #region Ability Check
        public virtual RollResult RollInitiative() => Dice.D20.Roll() + this[AbilityScore.Dexterity];
        
        public RollResult AbilityCheck(Skill skill, RollInfluence influence = RollInfluence.None) => AbilityCheck(skill.GetDefaultRelevantAbility(), skill, influence);

        public virtual RollResult AbilityCheck(AbilityScore ability, RollInfluence influence = RollInfluence.None) => Dice.D20.Roll(influence: influence) + this[ability];
        public RollResult AbilityCheck(AbilityScore ability, Skill skill, RollInfluence influence = RollInfluence.None) => AbilityCheck(ability, influence) + this[GetProficiency(skill)];

        protected virtual Proficiency GetProficiency(Skill stat) => skillSet[stat];

        #endregion

        #region Saving Throw

        public virtual RollResult SavingThrow(AbilityScore stat, RollInfluence influence = RollInfluence.None) => Dice.D20.Roll(influence: influence) + SavingThrowBonus(stat);
        protected virtual int SavingThrowBonus(AbilityScore stat) => this[stat] + this[SavingThrowProficiency(stat)];

        protected virtual Proficiency SavingThrowProficiency(AbilityScore stat) => savingThrowProficiencies[stat];

        #endregion

        #region Combat
        public int ApplyDamageRessistance(Damage damage, bool isCritical) =>
             GetDamageResistance(damage.DamageType).ModifyDamage(damage.Roll(isCritical: isCritical));
        

        public virtual DamageResistance GetDamageResistance(DamageType damageType) => DamageResistance.None;
        #endregion

        public virtual bool TryGetAttack(Weapon.SmartId weapon, out Attack attack) 
        {
            if (weapon == null) 
            {
                attack = null;
                return false;
            }

            if (!weapon.TryGetEntity(out Weapon prot)) 
            {
                attack = null;
                return false;
            }

            var ability = (prot[Weapon.Property.Finesse] ? Math.Max(this[AbilityScore.Strength], this[AbilityScore.Dexterity]) : this[AbilityScore.Strength]);

            attack= new Attack(
                name: prot.NameForInspector,
                isRange: prot.IsRanged,
                attackBonus: ProficiencyBonus + ability,
                damage: new Damage()
                    {
                        DamageType = prot.DamageType,
                        DamageDice = new List<Dice>() { prot.Damage },
                        DamageBonus = ability
                    }
            );

            return true;
        }

        public abstract List<Attack> GetAttacks();

        #region Inspector

        public virtual bool TryGetConcept<T>(out T value) where T : IComparable
        {
            if (typeof(T) == typeof(Size))
            {
                value = (T)((object)Size);
                return true;
            }

            value = default;
            return false;
        }


        internal static Creature inspectedCreature;
        protected pegi.EnterExitContext enterExitContext = new();
        public virtual string NameForInspector { get => _name; set => _name = value; }

        protected virtual void Inspect_StatBlock_Header()
        {
            NameForInspector.PegiLabel(style: pegi.Styles.HeaderText).Nl();
        }

        protected virtual void Inspect_StatBlock_AcHpSpeedBlock()
        {
            "Armor Class {0}".F(ArmorClass).PegiLabel().Nl();
            "Hit Points {0}".F(MaxHitPoints).PegiLabel().Nl();
            "Speed {0}".F(this[SpeedType.Walking]).PegiLabel().Nl();

        }

        protected virtual void Inspect_StatBlock_Abiities()
        {
            var ablNames = new System.Text.StringBuilder().Append("   ");
           // var ablValues = new System.Text.StringBuilder();
            for (int i=0; i<6; i++) 
            {
                var ab = (AbilityScore)i;
                ablNames.Append(ab.GetShortName()).Append("      ");
                var modifier = this[ab];
                string abilityScoreText = "{0}({1})".F(GetTotalScore(ab), modifier >= 0 ? "+{0}".F(modifier) : modifier.ToString());

                "{0}{1}{2}".F(ab.GetShortName(), pegi.EnvironmentNl, abilityScoreText).PegiLabel(width: 60, style: pegi.Styles.ListLabel).Write();

                //ablValues.Append(abilityScoreText).Append(") ");
            }
            //ablNames.ToString().nl();
            //ablValues.ToString().nl();

        }

        protected virtual void Inspect_StatBlock_Proficiencies()
        {
            "Proficiency Bonus: + {0}".F(ProficiencyBonus).PegiLabel().Nl();
        }

        protected virtual void Inspect_StatBlock_Actions(List<Attack> attacks)
        {
            if (attacks.Count == 0)
                return;

            "Actions".PegiLabel(style: pegi.Styles.ListLabel).Nl();
            pegi.Line(Color.red);

            foreach (var a in attacks) 
            {
                a.GetNameForInspector().PegiLabel().WriteBig();
            }
        }

        public virtual void Inspect_StatBlock(List<Attack> attacks) 
        {
            Inspect_StatBlock_Header();

            pegi.Line(Color.red);

            Inspect_StatBlock_AcHpSpeedBlock();

            pegi.Line(Color.red);

            Inspect_StatBlock_Abiities();

            pegi.Line(Color.red);

            Inspect_StatBlock_Proficiencies();

            Inspect_StatBlock_Actions(attacks);
        }

        
        protected virtual void InspectAbilities() 
        {
            if (Stats.Count > 0 && Icon.Clear.ClickConfirm("clStts", toolTip: "Will Clear All Stat Rolls"))
                Stats.Clear();

            if ("Reroll".PegiLabel(toolTip: "This will rerolls all your Ability Scores Base Values.").ClickConfirm(confirmationTag: "ReRollSt").Nl())
                RerollAbilityScores();

            Stats.Nested_Inspect().Nl();
        }


        protected virtual void Inspect_Contextual() {}

        public void Inspect()
        {
            using (enterExitContext.StartContext())
            {
                inspectedCreature = this;

                if (enterExitContext.IsAnyEntered == false)
                {
                    var tmpName = NameForInspector;
                    if ("Name [Key]".PegiLabel(90).Edit(ref tmpName))
                        NameForInspector = tmpName;

                    var tmp = this;
                    pegi.CopyPaste.InspectOptionsFor(ref tmp);
                    if (Icon.Dice.ClickConfirm(confirmationTag: "Change Random seed? Will affec all Fallback values"))
                        Seed.Randomize();
                    pegi.Nl();
                }

                if ("Block".PegiLabel().IsEntered().Nl())
                    Inspect_StatBlock(GetAttacks());

                if ("Ability Scores [{0}]".F(Stats.IsNullOrEmpty() ? "Fallback" : "Manual").PegiLabel().IsEntered().Nl())
                {
                    InspectAbilities();
                }


                if ("Skills".PegiLabel().IsEntered().Nl())
                {
                    var skills = (Skill[])Enum.GetValues(typeof(Skill));

                    for (int i = 0; i < skills.Length; i++)
                    {
                        Skill skill = skills[i];

                        skillSet.Inspect(skill);

                        "{0} {1} ({2})".F((this[GetProficiency(skill)] + this[skill.GetDefaultRelevantAbility()]).ToSignedNumber(), skill.GetNameForInspector(), skill.GetDefaultRelevantAbility().GetShortName()).PegiLabel(120).Write();

                        Icon.Dice.Click(() => pegi.GameView.ShowNotification(AbilityCheck(skill).ToString()));

                        pegi.Nl();
                    }
                }

                if ("Saving Throws".PegiLabel().IsEntered().Nl())
                {
                    for (int i = 0; i < STATS_COUNT; i++)
                    {
                        var enm = ((AbilityScore)i);

                        savingThrowProficiencies.Inspect(enm);

                        "{0} ({1})".F(enm.ToString(), SavingThrowBonus(enm).ToSignedNumber()).PegiLabel().Write();

                        Icon.Dice.Click(() =>
                            pegi.GameView.ShowNotification(SavingThrow(enm).ToString()));

                        pegi.Nl();
                    }
                }

                "Senses".PegiLabel().IsEntered().Nl().If_Entered(()=> senses.Nested_Inspect());
                
                Allignment.ToString().PegiLabel().Enter_Inspect(Allignment).Nl();

                Inspect_Contextual();
            }
        }

        public virtual void InspectInList(ref int edited, int ind)
        {
            var name = NameForInspector;
            if (pegi.Edit(ref name))
                NameForInspector = name;

            if (Icon.Enter.Click())
                edited = ind;
        }

        public virtual IEnumerator SearchKeywordsEnumerator()
        {
            foreach (var l in LanguagesKnown)
                yield return l.ToString();

            yield return skillSet;
        }

        protected void RerollAbilityScores()
        {
            for (int i = 0; i < STATS_COUNT; i++)
                Stats[(AbilityScore)i] = new CoreStat(Dice.D6.Roll(4, Drop.Lowest));
        }


        #endregion

    }
}
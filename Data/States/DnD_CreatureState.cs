using QuizCanners.Inspect;
using QuizCanners.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace Dungeons_and_Dragons
{
    public abstract class CreatureStateGeneric<T> : CreatureStateBase, IPEGI where T: Creature, new()
    {
        public T Creature => CreatureId?.GetEntity();
        protected override Creature CreatureData => Creature;
        public abstract DnD_SmartId<T> CreatureId { get; }

        #region Inspector

        protected override void Inspect_Context()
        {
            typeof(T).ToPegiStringType().PegiLabel().Enter_Inspect(CreatureId).Nl();
        }

        public override void InspectInList(ref int edited, int ind)
        {
            base.InspectInList(ref edited, ind);
            var ent = CreatureId.GetEntity();
            if (ent != null)
                ent.InspectInList(ref edited, ind);
        }
        #endregion
    }

    public abstract class CreatureStateBase : IPEGI_ListInspect
    {
        [SerializeField] private bool _hpInitialized;
        [SerializeField] protected int _currentHp;
        [SerializeField] public int Initiative;
        [SerializeField] private Exhaustion _exhaustion = new();
        [SerializeField] public ConditionsSet Conditions = new();
        [SerializeField] private CreatureHealthState _creatureHealthState;
        [SerializeField] private SurpriseState _surpriseState = SurpriseState.Unaware;
        [SerializeField] private DeathSavingThrowsData deathSavingThrows = new();

        public CreatureHealthState HealthState => _creatureHealthState;

        private int CurrentHitPoints
        {
            set
            {
                _hpInitialized = true;
                _currentHp = Mathf.Max(0, value);
            }
            get => _hpInitialized ? System.Math.Min(_currentHp, MaxHp) : MaxHp;
        }

        protected abstract Creature CreatureData { get; }
        private enum SurpriseState { Unaware = 0, Surprised = 1, NoticedThreat = 2 }

        public int ExhaustionLevel 
        {
            get => _exhaustion.Level;
            set 
            {
                _exhaustion.Level = value;
                CurrentHitPoints = CurrentHitPoints;
                if (_exhaustion.Death)
                    _creatureHealthState = CreatureHealthState.Dead;
            }
        }

        public bool TryTakeHit(Attack attack, RollInfluence influence)
        {
            int criticalOn = 20;

            switch (_creatureHealthState)
            {
                case CreatureHealthState.UnconsciousDeathSavingThrows:
                    influence = influence.And(RollInfluence.Advantage);
                    if (!attack.IsRanged)
                    {
                        criticalOn = 0;
                    }
                    break;
            }


            if (!attack.RollAttack(influence, ArmorClass, out bool isCritical, criticalOn: criticalOn))
            {
                return false;
            }

            switch (_creatureHealthState)
            {
                case CreatureHealthState.UnconsciousDeathSavingThrows:
                    deathSavingThrows.Feed(isSuccess: false, isCritical: isCritical);
                    if (deathSavingThrows.IsDead)
                    {
                        _creatureHealthState = CreatureHealthState.Dead;
                    }
                    break;
            }

            if (CreatureData != null)
            {
                SubtractHitPoints(CreatureData.ApplyDamageRessistance(attack.Damage, isCritical: isCritical));
            }
            else
            {
                SubtractHitPoints(attack.Damage.Roll(isCritical));
            }

            return true;
        }

        public abstract bool TryPass(SavingThrow savingThrow, RollInfluence influence);

        public virtual void RollInitiative() 
        {
            Initiative = CreatureData.RollInitiative().Value;
        }

     
        public void Kill() 
        {
            _creatureHealthState = CreatureHealthState.Dead;
            CurrentHitPoints = 0;
        }

        public enum CreatureHealthState { Alive, UnconsciousDeathSavingThrows, UnconsciousStable, Dead }

        protected virtual int ArmorClass => CreatureData == null ? 10 : CreatureData.ArmorClass;

        public int MaxHp 
        { 
            get 
            {
                int value;
                var creatureBase = CreatureData;
                if (creatureBase == null)
                    value = 8;
                else
                    value = creatureBase.MaxHitPoints;

                return _exhaustion.HitPointMaximum.ApplyTo(value);
            } 
        }

        public void Resurect() 
        {
            ExhaustionLevel -= 1;
            _creatureHealthState = CreatureHealthState.Alive;
            CurrentHitPoints = Mathf.Max(1, CurrentHitPoints);
        }

        public void AddHitPoints(int toAdd) 
        {
            if (toAdd <= 0)
            {
                Debug.LogError("Adding {0} Hp??".F(toAdd));
                return;
            }

            if (_creatureHealthState == CreatureHealthState.Dead) 
            {
                return;
            }

            CurrentHitPoints = Mathf.Min(MaxHp, CurrentHitPoints + toAdd);

            switch (_creatureHealthState)
            {
                case CreatureHealthState.Alive:  break;
                case CreatureHealthState.UnconsciousDeathSavingThrows: _creatureHealthState = CreatureHealthState.Alive; break;
                case CreatureHealthState.UnconsciousStable: _creatureHealthState = CreatureHealthState.Alive; break;
            }
        }

        public void SubtractHitPoints(int toSubtract)
        {
            if (toSubtract <= 0)
            {
                Debug.LogError("Subtracting {0} Hp??".F(toSubtract));
                return;
            }

            switch (_creatureHealthState) 
            {
                case CreatureHealthState.Alive:
                    int damage = Mathf.Min(CurrentHitPoints, toSubtract);
                    CurrentHitPoints -= toSubtract;

                    var remaining = toSubtract - damage;
                    if (remaining > MaxHp)
                    {
                        _creatureHealthState = CreatureHealthState.Dead;
                    } else 
                    {
                        if (CurrentHitPoints < 1)
                            _creatureHealthState = CreatureHealthState.UnconsciousDeathSavingThrows;
                    }
                    break;
                case CreatureHealthState.UnconsciousDeathSavingThrows:
                    //TODO: Process damage
                    break;
                case CreatureHealthState.UnconsciousStable:
                    _creatureHealthState = CreatureHealthState.UnconsciousDeathSavingThrows;
                    break;
            }

         
        }

        public virtual RollInfluence GetInfluenceWhenAttackingMe(bool seeingAttacker) =>
             CreatureData.GetInfluenceWhenAttackingMe(seeingAttacker: seeingAttacker);
        
        public void TrySneakBy (RollResult stealthCheck) 
        {
            if (_surpriseState == SurpriseState.NoticedThreat) 
                return;
            
            if (_creatureHealthState != CreatureHealthState.Alive || Conditions[Condition.Unconscious]) 
                _surpriseState = SurpriseState.Surprised;
            else 
            {
                if (CreatureData.TrySurprise(stealthCheck))
                    _surpriseState = SurpriseState.Surprised;
                else
                    _surpriseState = SurpriseState.NoticedThreat;
            }
        }

        public void OnEndTurn()
        {
            if (_surpriseState == SurpriseState.Surprised) 
            {
                _surpriseState = SurpriseState.NoticedThreat;
            }
        }

        public void OnStartTurn() 
        {
            switch (_creatureHealthState)
            {
                case CreatureHealthState.UnconsciousDeathSavingThrows:
                    deathSavingThrows.Roll();
                    if (deathSavingThrows.IsCauncous)
                        Resurect();
                    else if (deathSavingThrows.IsDead)
                        _creatureHealthState = CreatureHealthState.Dead;
                    break;
            }
        }

      

       
      

        [SerializeField] private pegi.EnterExitContext inspectedStuff = new();

        public virtual List<Attack> GetAttacksList()
        {
            var lst = new List<Attack>();

            if (CreatureData != null)
            {
                lst.AddRange(CreatureData.GetAttacks());
            }

            return lst;
        }




        #region Inspector
        public override string ToString() => CreatureData == null ? "NULL" : CreatureData.GetNameForInspector();

        public void Inspect()
        {
            if (!inspectedStuff.IsAnyEntered)
            {
                InspectHealthState();

                pegi.Line(Color.gray);
            }

            using (inspectedStuff.StartContext()) 
            {
                Inspect_Context();

                if (CreatureData != null)
                    if (CreatureData.GetNameForInspector().PegiLabel().IsEntered().Nl())
                    CreatureData.Inspect_StatBlock(GetAttacksList());
            }

        }

        protected virtual void Inspect_Context() { }

        public void LongRest() 
        {
            _hpInitialized = false;
            _creatureHealthState = CreatureHealthState.Alive;
            _exhaustion.Level -= 1;
        }

        protected virtual void InspectHealthState() 
        {
            var hp = CurrentHitPoints;

            if (_hpInitialized)
                Icon.Refresh.Click(LongRest);

            switch (_creatureHealthState)
            {
                case CreatureHealthState.Alive:

                    var ex = ExhaustionLevel;
                    if (ex > 0)
                    {
                        if ("Ex".PegiLabel(20).Edit(ref ex, 25))
                            ExhaustionLevel = ex;
                    }

                    if ("Hp".PegiLabel(30).Edit_Delayed(ref hp, 40))
                        CurrentHitPoints = hp;

                    if (CurrentHitPoints != MaxHp)
                    {
                        "/{0}".F(MaxHp).PegiLabel(40).Write();
                    }
                    break;
                case CreatureHealthState.Dead:
                    if ("Resurect".PegiLabel().Click())
                        Resurect();
                    break;
                case CreatureHealthState.UnconsciousDeathSavingThrows:
                    deathSavingThrows.InspectInList_Nested();
                    break;
                default: _creatureHealthState.ToString().PegiLabel().Write(); break;
            }
        }

        public virtual void InspectInList(ref int edited, int ind)
        {
            InspectHealthState();
        }
        #endregion
    }
}
using QuizCanners.Inspect;
using QuizCanners.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dungeons_and_Dragons
{

    [Serializable]
    public class ArmyCheckCalculator : IPEGI
    {
        [SerializeField] private string _creature;
        [SerializeField] private int _units;
        [SerializeField] private int _dc;
        [SerializeField] private int _bonus;
        [SerializeField] private int _effectApplyCount;
        [SerializeField] private RollInfluence _influence = RollInfluence.None;
        [SerializeField] private KindOfD20Roll _kindOfRoll;


        [Header("When passed:")]
        [SerializeField] private List<EffectRoll> _onPass = new List<EffectRoll>();
        [SerializeField] private List<int> _damage = new List<int>();
        [SerializeField] private bool _halfOnFailed;

        private bool CountFailures => _kindOfRoll == KindOfD20Roll.Saving_Throw;
        private bool AddHalfForSucces => _kindOfRoll == KindOfD20Roll.Saving_Throw && _halfOnFailed;

        readonly pegi.CollectionInspectorMeta _onPassList = new("If passed");
        public void Inspect()
        {
            var changes = pegi.ChangeTrackStart();
            "Creature".PegiLabel(60).Edit(ref _creature);
            "x".PegiLabel(25).Edit(ref _units).Nl();

            "Roll".PegiLabel(40).Edit_Enum(ref _kindOfRoll).Nl();

            _kindOfRoll.GetRollName().PegiLabel(90).Edit(ref _bonus).Nl();

            switch (_kindOfRoll) 
            {
                case KindOfD20Roll.Ability_Check:
                case KindOfD20Roll.Saving_Throw:
                    "DC".PegiLabel(30).Edit(ref _dc);
                    break;
                case KindOfD20Roll.Attack_Roll:
                    "Target AC".PegiLabel(70).Edit(ref _dc);
                    break;
            }
           
           
            "Influences".PegiLabel(90).Edit_Enum(ref _influence).Nl();

            

            "Result: {0} {1} / {2}".F(_effectApplyCount, CountFailures ? "Failed" : "Hits", _units).PegiLabel().DrawProgressBar(_effectApplyCount / (float)_units);

            Icon.Dice.Click();

            pegi.Nl();

            if (_kindOfRoll!= KindOfD20Roll.Ability_Check && "Damage".PegiLabel().IsFoldout().Nl()) 
            {
                if (CountFailures)
                {
                    "Half damage on Success".PegiLabel().ToggleIcon(ref _halfOnFailed).Nl();
                }

                int allEffects = 0;
                foreach (var e in _damage)
                    allEffects += e;

                "Total Applied Damage: {0}".F(allEffects).PegiLabel().Nl();

                _onPassList.Edit_List(_onPass).Nl();


                for (int i=0; i< _damage.Count; i++) 
                {
                    var el = _damage[i];
                    if (el != 0) 
                    {
                        "{0} {1}: {2} damage".F(_creature, i, el).PegiLabel().Nl();
                    }
                }
            }

            if (changes)
            {
                _effectApplyCount = 0;

                _damage.Clear();

                for (int i = 0; i < _units; i++)
                {
                    var rollFailed = ((Dice.D20.Roll(_influence) + _bonus) < _dc);

                    var countEffect = (rollFailed == CountFailures);

                    _effectApplyCount += countEffect ? 1 : 0;


                    if (countEffect)
                        _damage.Add(RollEffects());
                    else if (AddHalfForSucces)
                        _damage.Add(Mathf.FloorToInt(RollEffects() * 0.5f));
                    else
                        _damage.Add(0);
                }
            }
        }

        private int RollEffects() 
        {
            int sum = 0;
            foreach (var r in _onPass)
                sum += r.GetNewResult();

            return sum;
        }


        [Serializable]
        internal class EffectRoll : IPEGI_ListInspect
        {
            [SerializeField] private Dice _dice = Dice.D6;
            [SerializeField] private int _count = 1;
            [SerializeField] private int _toAdd;

            public int GetNewResult() => _dice.Roll(diceCount: _count).Value + _toAdd;

            public override string ToString() => "{0}d{1} {2}".F(_count, (int)_dice, _toAdd >= 0 ? "+{1}".F(_toAdd) : _toAdd);

            public void InspectInList(ref int edited, int index)
            {
                pegi.Edit(ref _count, width: 30);
                pegi.Edit_Enum(ref _dice);
              
                "+".PegiLabel(15).Edit(ref _toAdd);
            }
        }



    }
}
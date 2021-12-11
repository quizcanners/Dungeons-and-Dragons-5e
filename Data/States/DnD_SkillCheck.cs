
using QuizCanners.Inspect;
using QuizCanners.Utils;
using System;
using UnityEngine;

namespace Dungeons_and_Dragons
{
    [System.Serializable]
    public class AbilityCheck : IPEGI
    {
        [SerializeField] protected AbilityFallback _abilityScore = new();
        public Skill SkillToCheck = Skill.Perception;
        public int DC = 15;


        [Serializable]
        protected class AbilityFallback : Fallback.FallbackValueGeneric<AbilityScore> { }

        public AbilityScore AbilityScore
        {
            get => _abilityScore.Get(defaultValueGetter: ()=> SkillToCheck.GetDefaultRelevantAbility()); 
            set => _abilityScore.ManualValue = value;
        }

        public bool TryAbilityCheck(Creature creature, RollInfluence influence) => creature.AbilityCheck(AbilityScore, SkillToCheck, influence) >= DC;

        public void Inspect()
        {
            "DC".PegiLabel(40).Edit( ref DC, 50);

            if (_abilityScore.IsSet)
            {
                Icon.Clear.Click("Use Default Ability").OnChanged(()=> _abilityScore.IsSet = false);
                    
                var score = AbilityScore;
                pegi.Edit_Enum(ref score, width: 70).OnChanged(() => AbilityScore = score);
                   
            } else 
                AbilityScore.GetNameForInspector().PegiLabel("Change Ability to test").Click(() => AbilityScore = SkillToCheck.GetDefaultRelevantAbility());
            
            pegi.Edit_Enum(ref SkillToCheck, width: 70);
        }

        public override string ToString() => "DC {0} {1} ({2})".F(DC, AbilityScore, SkillToCheck);
    }
}
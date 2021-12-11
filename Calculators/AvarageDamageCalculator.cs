
using QuizCanners.Inspect;
using QuizCanners.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dungeons_and_Dragons
{
    [System.Serializable]
    public class AvarageDamageCalculator : IPEGI
    {
        public static int ACDebug = 15;

        public List<AttackForCalculator> Attacks = new();

        private int _inspectedAttack = -1;

        public void Inspect()
        {
            "Armor Class".PegiLabel(90).Edit_WithButtons(ref ACDebug);

            "Attacks".PegiLabel().Edit_List(Attacks, ref _inspectedAttack).Nl();
         
            "DAMAGE".PegiLabel(style: pegi.Styles.ListLabel).Nl();

            float totalDam = 0;

            foreach (var a in Attacks)
                totalDam += a.DamagePerRound(ACDebug);

            "Total Damage Per Round: {0}".F(Mathf.FloorToInt(totalDam)).PegiLabel().Nl();
            
        }

       
        [Serializable]
        public class AttackForCalculator : IPEGI_ListInspect
        {
            public Attack AttackData = new(
                name: "For Calculator", 
                isRange: false, 
                attackBonus: 1, 
                new Damage() 
                {
                    DamageBonus = 1,
                    DamageDice = new List<Dice>(),
                    DamageType = DamageType.Piercing
                });

            public bool CritOn19;
            public bool Advantage;

            private int GetHitResults(int AC) => Mathf.Clamp(21 + AttackData.AttackBonus - AC, min: CritOn19 ? 2 : 1, max: 19);

            public int TargetRoll(int AC) => 21 - GetHitResults(AC);

            public float GetHitChance(int AC) 
            {
                int hitResults = GetHitResults(AC);

                float hitChance = hitResults / 20f;
                if (Advantage)
                {
                    hitChance = (2 - hitChance) * hitChance;
                }
                return hitChance;
            }

            public float DamagePerRound(int AC) 
            {
                const float CHANCE_OF_CRIT = 0.05f;
                const float CHANCE_OF_19_CRIT = CHANCE_OF_CRIT * 2;

                var avgDamageRoll = AttackData.Damage.DamageDice.AvargeRoll();
                float criticalAvgDamage = avgDamageRoll * (Advantage ? (CritOn19 ? 0.19f : 0.0975f) : (CritOn19 ? CHANCE_OF_19_CRIT : CHANCE_OF_CRIT));

                return (avgDamageRoll + AttackData.Damage.DamageBonus) * GetHitChance(AC) + criticalAvgDamage;
            }

            public void InspectInList(ref int edited, int index)
            {
                AttackData.InspectInList(ref edited, index);

                //var hitRes = GetHitResults(ACDebug);
                var hitChance = GetHitChance(ACDebug);
                "= {0} DPR (THAC {1} | {2}%) ".F(Math.Round(DamagePerRound(ACDebug)), TargetRoll(ACDebug), Mathf.FloorToInt(hitChance * 100)).PegiLabel(160).Write();

                "Adv".PegiLabel(40).Toggle(ref Advantage);

                "19 Crit".PegiLabel(40).Toggle(ref CritOn19);
            }
        }

    }
}
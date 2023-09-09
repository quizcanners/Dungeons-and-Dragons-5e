using QuizCanners.Inspect;
using QuizCanners.Utils;
using System;
using System.Collections.Generic;


namespace Dungeons_and_Dragons
{
    [Serializable]
    public class Damage : IPEGI_ListInspect
    {
        public List<Dice> DamageDice = new List<Dice>();
        public DamageType DamageType;
        public int DamageBonus;

        public int Avarage => DamageDice.AvargeRoll() + DamageBonus;

        public int Roll(bool isCritical) => DamageDice.Roll().Value + (isCritical ? DamageDice.Roll().Value : 0) + DamageBonus;

        #region Inspector
        public override string ToString() => "Hit: {0} {1} damage.".F(DamageDice.ToDescription(DamageBonus), DamageType);
        public void InspectInList(ref int edited, int index)
        {
            if (DamageDice.Count == 0)
            {
                "+Dice".PegiLabel().Click(() => DamageDice.Add(Dice.D6));
                "Dmg".PegiLabel(30).Edit(ref DamageBonus, 30);
            }
            else
            {
                int cnt = DamageDice.CountOccurances(DamageDice[0]);
                var first = DamageDice[0];

                var changed = pegi.ChangeTrackStart();

                pegi.Edit_Delayed(ref cnt, 25);
                "d".PegiLabel(15).Edit_Enum(ref first, d => ((int)d).ToString(), 35);

                if (changed)
                {
                    DamageDice.Clear();
                    for (int i = 0; i < cnt; i++)
                        DamageDice.Add(first);
                }
                "+".PegiLabel(15).Edit(ref DamageBonus, 25);
                "dmg.".PegiLabel(35).Write();
            }
        }
        #endregion
    }
}
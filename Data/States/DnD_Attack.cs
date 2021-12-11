using QuizCanners.Inspect;
using QuizCanners.Utils;
using System;

namespace Dungeons_and_Dragons
{
    [Serializable]
    public class Attack : IPEGI_ListInspect
    {
        public string Name;
        public int AttackBonus;
        public bool IsRanged;
        public FeetDistance range = new FeetDistance() { ft = 5 };
        public Damage Damage;
        

        public bool RollAttack(RollInfluence influence, int armorClass, out bool isCriticalHit, int criticalOn = 20)
        {
            var roll = Dice.D20.Roll(influence).Value;
            isCriticalHit = (roll >= criticalOn);

            if (isCriticalHit)
                return true;

            if (roll == 1)
                return false;

            return (roll + AttackBonus)>= armorClass;
        }

        public Attack (string name, bool isRange, int attackBonus, Damage damage) 
        {
            Name = name;
            AttackBonus = attackBonus;
            IsRanged = isRange;
            Damage = damage;
        }

        public void InspectInList(ref int edited, int index)
        {
            "+".PegiLabel(15).Edit(ref AttackBonus, 20);

            "To Hit, ".PegiLabel(40).Write();
            Damage.InspectInList(ref edited, index);
        }

        public override string ToString()
        {
            return "{0}. {1} Weapon Attack: +{2} to hit, reach {3}, one target. {4}".
                F(Name, IsRanged ? "Range" : "Melee", AttackBonus, range.ToString(), Damage);
        }
    }
}

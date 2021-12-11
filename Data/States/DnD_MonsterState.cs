using System;
using QuizCanners.Inspect;

namespace Dungeons_and_Dragons
{
    [Serializable]
    public class MonsterState : CreatureStateGeneric<Monster>, IPEGI, IPEGI_ListInspect
    {
        public Monster.SmartId MonsterId = new();

        public Monster Data => MonsterId.GetEntity();

        public override DnD_SmartId<Monster> CreatureId => MonsterId;

        public override bool TryPass(SavingThrow savingThrow, RollInfluence influence)
        {
            if (Data == null)
                return Dice.D20.Roll(influence) >= savingThrow.DC;

            return Data.SavingThrow(savingThrow.Score, influence) > savingThrow.DC;
        }
    }
}
using QuizCanners.Inspect;
using System;
using System.Collections.Generic;

namespace Dungeons_and_Dragons
{
    [Serializable]
    public class CharacterState : CreatureStateGeneric<CharacterSheet>
    {
        public List<Weapon.SmartId> Weapons = new();
        public CharacterSheet.SmartId CharacterId = new();

        public CharacterSheet Data => CharacterId.GetEntity();

        public override List<Attack> GetAttacksList()
        {
            var lst = base.GetAttacksList();

            var prot = Creature;
            if (prot != null) 
            {
                foreach (var w in Weapons)
                    if (Creature.TryGetAttack(w, out var attack))
                        lst.Add(attack);
            }

            return lst;
        }

        public override DnD_SmartId<CharacterSheet> CreatureId => CharacterId;

        protected override void Inspect_Context() 
        {
            "Weapons".PegiLabel().Enter_List(Weapons).Nl();
        }

        public override bool TryPass(SavingThrow savingThrow, RollInfluence influence)
            => Data.SavingThrow(savingThrow.Score, influence) > savingThrow.DC;

        public CharacterState() { }
        public CharacterState(CharacterSheet.SmartId origin) 
        {
            CharacterId = new CharacterSheet.SmartId();
            CharacterId.SetEntityId(origin);
        }
    }
}

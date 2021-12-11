using Dungeons_and_Dragons.Tables;
using QuizCanners.Inspect;
using System;
using UnityEngine;

namespace Dungeons_and_Dragons
{
    [Serializable]
    public class SeededFallacks : IPEGI
    {
        [SerializeField] private RandomElementsRollTables _name;
        [SerializeField] private RandomElementsRollTables _race;
        [SerializeField] private RandomElementsRollTables _class;


        public string GetName(Creature creature) => _name ? _name.GetRolledElementName(creature.Seed, creature, shortText: true) : "No names table";
        public Race GetRace(Creature creature) => GetFallback_Internal(_race, creature, Race.Human);
        public Class GetClass(Creature creature) => GetFallback_Internal(_class, creature, Class.Fighter);

        private T GetFallback_Internal<T>(RandomElementsRollTables table, Creature creature, T defaultValue) 
        {
            if (table && table.TryGetConcept(out T value, creature.Seed, creature))
                return value;

            return defaultValue;
        }


        [SerializeField] protected pegi.EnterExitContext _inspectedFallback = new();
        public void Inspect()
        {
            using (_inspectedFallback.StartContext())
            {
                "Name".PegiLabel().Edit_Enter_Inspect(ref _name).Nl();
                "Race".PegiLabel().Edit_Enter_Inspect(ref _race).Nl();
                "Class".PegiLabel().Edit_Enter_Inspect(ref _class).Nl();
            }
        }
    }
}

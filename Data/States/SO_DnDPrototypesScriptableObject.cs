using Dungeons_and_Dragons.Calculators;
using Dungeons_and_Dragons.Tables;
using QuizCanners.Inspect;
using QuizCanners.Utils;
using UnityEngine;

namespace Dungeons_and_Dragons
{
    [CreateAssetMenu(fileName = FILE_NAME, menuName = QcUnity.SO_CREATE_MENU + Singleton_DnD.SO_CREATE_DND + FILE_NAME)]
    public class SO_DnDPrototypesScriptableObject : ScriptableObject, IPEGI
    {
        public const string FILE_NAME = "Prototypes of Dungeons & Dragons";

        public CharactersDictionary Characters = new();
        public MonstersDictionary Monsters = new();
        public RandomElementsRollTablesDictionary RollTables = new();
        public EncounterCalculator EncounterCalculator = new();
        public CombatTracker CombatTracker = new();
        public AvarageDamageCalculator DamageCalculator = new();
        public DiceCalculator DiceCalculator = new();
        public ArmyCheckCalculator ArmyCheckCalculator = new();

        public SeededFallacks Fallbacks;

        #region Inspector

        [SerializeField] private pegi.CollectionInspectorMeta _characterListMeta =   new("Characters",     showCopyPasteOptions: true);
        [SerializeField] private pegi.CollectionInspectorMeta _monsterListMeta =     new("Monsters",       showCopyPasteOptions: true);
        [SerializeField] private pegi.CollectionInspectorMeta _rollTablesListMeta =  new("Roll Tables",    showCopyPasteOptions: true);

        protected pegi.EnterExitContext context = new();
        protected pegi.EnterExitContext _calculators = new();

        public void Inspect()
        {
            if (!Singleton.Get <Singleton_DnD>()) 
            {
                "Instance is Null. Have {0} in the scene to initialize internal singleton".F(nameof(Singleton_DnD)).PegiLabel().WriteWarning();
                return;
            }

            pegi.Nl();

            using (context.StartContext())
            {
                _characterListMeta.Enter_Dictionary(Characters).Nl();
                _monsterListMeta.Enter_Dictionary(Monsters).Nl();
                _rollTablesListMeta.Enter_Dictionary(RollTables).Nl();
                "Seeded Fallbacks".PegiLabel().Enter_Inspect(Fallbacks).Nl();

                if ("Calculators".PegiLabel().IsEntered().Nl())
                {
                    using (_calculators.StartContext())
                    {
                        "Encounter Calculator".PegiLabel().Enter_Inspect(EncounterCalculator).Nl();
                        "Combat Tracker".PegiLabel().Enter_Inspect(CombatTracker).Nl();
                        "Damage Calculator".PegiLabel().Enter_Inspect(DamageCalculator).Nl();
                        "Dice Calculator".PegiLabel().Enter_Inspect(DiceCalculator).Nl();
                        "Army Roll".PegiLabel().Enter_Inspect(ArmyCheckCalculator).Nl();
                    }
                }
            }

        }
        #endregion
    }

    [PEGI_Inspector_Override(typeof(SO_DnDPrototypesScriptableObject))] 
    internal class DungeonsAndDragonsCharactersScriptableObjectDrawer : PEGI_Inspector_Override { }
}
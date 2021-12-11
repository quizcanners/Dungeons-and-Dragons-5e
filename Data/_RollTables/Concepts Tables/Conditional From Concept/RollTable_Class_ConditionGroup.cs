using QuizCanners.Inspect;
using UnityEngine;

namespace Dungeons_and_Dragons.Tables
{

    [CreateAssetMenu(fileName = FILE_NAME, menuName = SO_CREATE_PATH + SO_MENU_NAME_CONCEPT + FILE_NAME)]
    public class RollTable_Class_ConditionGroup : DnD_RollTable_ConceptConditionGeneric<Class>
    {
        public const string FILE_NAME = "Conditional Table Group "+nameof(Class);

    }

    [PEGI_Inspector_Override(typeof(RollTable_Class_ConditionGroup))] internal class RollTable_Class_ConditionGroupDrawer : PEGI_Inspector_Override { }
}

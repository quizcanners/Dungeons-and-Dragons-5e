using QuizCanners.Inspect;
using UnityEngine;


namespace Dungeons_and_Dragons.Tables
{

    [CreateAssetMenu(fileName = FILE_NAME, menuName = SO_CREATE_PATH + SO_MENU_NAME_CONCEPT + FILE_NAME)]
    public class RollTable_Maturity_ConditionGroup : DnD_RollTable_ConceptConditionGeneric<Maturity>
    {
        public const string FILE_NAME = "Conditional Table Group "+nameof(Maturity);
    }
    [PEGI_Inspector_Override(typeof(RollTable_Maturity_ConditionGroup))] internal class RollTable_Maturity_ConditionGroupDrawer : PEGI_Inspector_Override { }

}
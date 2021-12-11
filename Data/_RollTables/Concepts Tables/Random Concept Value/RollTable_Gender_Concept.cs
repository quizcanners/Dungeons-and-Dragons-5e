using QuizCanners.Inspect;
using UnityEngine;

namespace Dungeons_and_Dragons.Tables
{
    [CreateAssetMenu(fileName = FILE_NAME, menuName = SO_CREATE_PATH + SO_MENU_NAME_CONCEPT + FILE_NAME)]
    public class RollTable_Gender_Concept : RollTable_ForEnum_Generic<Gender>
    {
        public const string FILE_NAME = "Random " + nameof(Gender);
    }

    [PEGI_Inspector_Override(typeof(RollTable_Gender_Concept))] internal class RollTable_Gender_ConceptDrawer : PEGI_Inspector_Override { }
}
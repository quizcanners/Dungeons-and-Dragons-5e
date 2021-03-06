using QuizCanners.Inspect;
using UnityEngine;

namespace Dungeons_and_Dragons.Tables
{
    [CreateAssetMenu(fileName = FILE_NAME, menuName = SO_CREATE_PATH + SO_MENU_NAME_CONCEPT + FILE_NAME)]
    public class RollTable_Race_Concept : RollTable_ForEnum_Generic<Race>
    {
        public const string FILE_NAME = "Random "+nameof(Race);
    }

    [PEGI_Inspector_Override(typeof(RollTable_Race_Concept))] internal class RollTable_Race_ConceptDrawer : PEGI_Inspector_Override { }
}
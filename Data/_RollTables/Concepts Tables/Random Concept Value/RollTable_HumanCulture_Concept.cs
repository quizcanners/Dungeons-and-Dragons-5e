using QuizCanners.Inspect;
using UnityEngine;

namespace Dungeons_and_Dragons.Tables
{
    [CreateAssetMenu(fileName = FILE_NAME, menuName = SO_CREATE_PATH + SO_MENU_NAME_CONCEPT + FILE_NAME)]
    public class RollTable_HumanCulture_Concept : RollTable_ForEnum_Generic<Human_Culture>
    {
        public const string FILE_NAME = "Random "+nameof(Human_Culture);
    }

    [PEGI_Inspector_Override(typeof(RollTable_HumanCulture_Concept))] internal class RollTable_HumanCulture_ConceptDrawer : PEGI_Inspector_Override { }
}
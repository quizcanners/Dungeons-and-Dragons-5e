using QuizCanners.Inspect;
using QuizCanners.Utils;
using UnityEngine;

namespace Dungeons_and_Dragons
{
    [ExecuteAlways]
    [AddComponentMenu("Quiz ñ'Anners/Dungeons And Dragons")]
    public class Singleton_DnD : Singleton.BehaniourBase
    {
        internal const string SO_CREATE_DND = "Dungeons & Dragons/";

        public SO_DnDPrototypesScriptableObject DnDPrototypes;

        public SeededFallacks Fallbacks => DnDPrototypes ? DnDPrototypes.Fallbacks : null;

        #region Inspector

        public override string InspectedCategory => "";

        public override string NeedAttention()
        {
            if (!DnDPrototypes)
                return "No Prototypes";

            return base.NeedAttention();
        }

        public override string ToString() => "Dungeons & Dragons 5e";

        public override void Inspect()
        {
            base.Inspect();

            if (!DnDPrototypes)
                "Prototypes".PegiLabel().Edit(ref DnDPrototypes).Nl();
            else
                DnDPrototypes.Nested_Inspect();
        }

        #endregion
    }
    

  [PEGI_Inspector_Override(typeof(Singleton_DnD))] internal class DnD_ManagerDrawer : PEGI_Inspector_Override { }

}
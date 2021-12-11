using QuizCanners.Inspect;
using QuizCanners.Utils;
using System;


namespace Dungeons_and_Dragons
{
    [Serializable]
    public class SavingThrow : IPEGI_ListInspect
    {
        public AbilityScore Score;
        public int DC;

        #region Inspector
        public override string ToString() => "DC {0} {1} saving throw".F(DC, Score);
        
        public void InspectInList(ref int edited, int index)
        {
            "DC".PegiLabel(30).Edit(ref DC);
            pegi.Edit_Enum(ref Score);
        }
        #endregion
    }
}

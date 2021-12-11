using QuizCanners.Inspect;
using QuizCanners.Utils;
using System;


namespace Dungeons_and_Dragons
{
    [Serializable]
    public class SavingThrow : IPEGI_ListInspect, IGotReadOnlyName
    {
        public AbilityScore Score;
        public int DC;

        #region Inspector
        public string GetReadOnlyName() => "DC {0} {1} saving throw".F(DC, Score);
        
        public void InspectInList(ref int edited, int index)
        {
            "DC".PegiLabel(30).Edit(ref DC);
            pegi.Edit_Enum(ref Score);
        }
        #endregion
    }
}

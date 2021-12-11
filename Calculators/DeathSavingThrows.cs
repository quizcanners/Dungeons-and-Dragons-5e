using QuizCanners.Inspect;
using UnityEngine;

namespace Dungeons_and_Dragons
{
    public class DeathSavingThrowsData : IPEGI_ListInspect
    {
        public int Success;
        public int Failure;

        public bool IsDead => Failure >= 3;
        public bool IsCauncous => Success >= 3;

        public void Roll()
        {
            var value = Dice.D20.Roll();
            Feed(isSuccess: value >= 10, isCritical: value == 20 || value == 1);
        }

        public void Feed(bool isSuccess, bool isCritical)
        {
            if (isSuccess)
                Success = Mathf.Min(3, Success + (isCritical ? 2 : 1));
            else
                Failure = Mathf.Min(3, Failure + (isCritical ? 2 : 1));
        }

        public void Reset()
        {
            Success = 0;
            Failure = 0;
        }

        public void InspectInList(ref int edited, int index)
        {
            for (int i = 0; i < 3; i++)
                ((Success > i) ? Icon.Done : Icon.Empty).Draw();
            "|".PegiLabel(20).Write();
            for (int i = 0; i < 3; i++)
                ((Failure > i) ? Icon.Close : Icon.Empty).Draw();

            Icon.Dice.Click(Roll);
        }
    }
}
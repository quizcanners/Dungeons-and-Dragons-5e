using System;
using QuizCanners.Inspect;
using QuizCanners.Utils;

namespace Dungeons_and_Dragons
{

    [Serializable]
    public class Allignment : IPEGI
    {
        public bool Alligned;
        public Goodness Goodness;
        public Order Order;

        public void Inspect()
        {
            "Allignment".PegiLabel().ToggleIcon(ref Alligned).Nl();
            if (Alligned)
            {
                pegi.Edit_Enum(ref Order);
                pegi.Edit_Enum(ref Goodness);
            }
        }

        public override string ToString() => Alligned ? "{0} {1}".F(Order.ToString(), Goodness.ToString()) : "Unalligned";
    }

    public enum Goodness { Neutral = 0, Good = 1,  Evil =2 }
    public enum Order { Neutral = 0, Lawful = 1, Chaotic = 2 }
}

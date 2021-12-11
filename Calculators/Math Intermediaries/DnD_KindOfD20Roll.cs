namespace Dungeons_and_Dragons
{
    public enum KindOfD20Roll { Ability_Check, Attack_Roll, Saving_Throw }

    public static class KindOfRollExtensions 
    {
        public static string GetRollName(this KindOfD20Roll kind) 
        {
            switch (kind)
            {
                case KindOfD20Roll.Ability_Check: return "Skill";
                case KindOfD20Roll.Attack_Roll: return "Attack Bonus";
                case KindOfD20Roll.Saving_Throw: return "Saving Throw";
                default: return kind.ToString();
            }
        }
    }
}
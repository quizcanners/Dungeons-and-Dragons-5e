using System;

namespace Dungeons_and_Dragons
{
    public static class DnDTime 
    {
        public const int SECONDS_PER_TURN = 6;
        public static TimeSpan SHORT_REST = TimeSpan.FromHours(1);
        public static TimeSpan LONG_REST = TimeSpan.FromHours(8);
        public static TimeSpan TEN_DAY = TimeSpan.FromDays(10);

    }
}
namespace EVESharp.Database.EVEMath;

public static class Skills
{
    public static double GetSkillPointsForLevel (long level, double timeConstant, double multiplier)
    {
        if (level > 5 || level == 0)
            return 0;

        return System.Math.Ceiling (timeConstant * multiplier * System.Math.Pow (2, 2.5 * (level - 1)));
    }

    public static double GetSkillPointsPerMinute (double primarySpPerMin, double secondarySpPerMin, int learningLevel, double currentSkillpoints)
    {
        double spPerMin = primarySpPerMin + (secondarySpPerMin / 2.0f);
        spPerMin = spPerMin * (1.0f + (0.02f * learningLevel));

        if (currentSkillpoints < 1600000.0f)
            spPerMin = spPerMin * 2.0f;

        return spPerMin;
    }
}
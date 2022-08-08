namespace EVESharp.Node.Database;

public enum SkillHistoryReason
{
    None                   = 0,
    SkillClonePenalty      = 34,
    SkillTrainingStarted   = 36,
    SkillTrainingComplete  = 37,
    SkillTrainingCancelled = 38,
    GMGiveSkill            = 39,
    SkillTrainingComplete2 = 53
}
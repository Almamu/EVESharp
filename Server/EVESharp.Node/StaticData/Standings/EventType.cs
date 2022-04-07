﻿namespace EVESharp.Node.StaticData.Standings;

public enum EventType : int
{
    StandingAgentBuyOff                 = 71,
    StandingAgentDonation               = 72,
    StandingAgentMissionBonus           = 80,
    StandingAgentMissionCompleted       = 73,
    StandingAgentMissionDeclined        = 75,
    StandingAgentMissionFailed          = 74,
    StandingAgentMissionOfferExpired    = 90,
    StandingCombatAggression            = 76,
    StandingCombatOther                 = 79,
    StandingCombatPodKill               = 78,
    StandingCombatShipKill              = 77,
    StandingDecay                       = 49,
    StandingDerivedModificationNegative = 83,
    StandingDerivedModificationPositive = 82,
    StandingInitialCorpAgent            = 52,
    StandingInitialFactionAlly          = 70,
    StandingInitialFactionCorp          = 54,
    StandingInitialFactionEnemy         = 69,
    StandingPirateKillSecurityStatus    = 89,
    StandingPlayerCorpSetStanding       = 68,
    StandingPlayerSetStanding           = 65,
    StandingReCalcEntityKills           = 58,
    StandingReCalcMissionFailure        = 61,
    StandingReCalcMissionSuccess        = 55,
    StandingReCalcPirateKills           = 57,
    StandingReCalcPlayerSetStanding     = 67,
    StandingSlashSet                    = 84,
    StandingStandingReset               = 25,
    StandingTutorialAgentInitial        = 81,
    StandingUpdateStanding              = 45,
}
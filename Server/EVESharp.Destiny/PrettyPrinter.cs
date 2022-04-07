using System.Text;

namespace EVESharp.Destiny;

public static class PrettyPrinter
{
    public const string Indention = "  ";

    public static string Print(UpdateReader reader)
    {
        return Print(reader, "");
    }

    public static string Print(UpdateReader reader, string indention)
    {
        StringBuilder sb = new StringBuilder();
        PrintHeader(sb, indention, reader.Header);
        foreach (Ball ball in reader.Balls)
            PrintBall(sb, indention, ball);
        return sb.ToString();
    }

    private static void PrintBall(StringBuilder sb, string indention, Ball ball)
    {
        sb.AppendLine(indention + "[Ball]");
        sb.AppendLine(indention + Indention + "[Name: " + ball.Name + "]");
        sb.AppendLine(indention + Indention + "[FormationId: " + ball.FormationId + "]");
        PrintBallHeader(sb, indention + Indention, ball.Header);

        if (ball.ExtraHeader != null)
            PrintBallExtraHeader(sb, indention + Indention, ball.ExtraHeader);

        if (ball.Data != null)
            PrintBallData(sb, indention + Indention, ball.Data);

        switch (ball.Header.Mode)
        {
            case BallMode.Goto:
                PrintGotoState(sb, indention + Indention, ball.GotoState);
                break;

            case BallMode.Warp:
                PrintWarpState(sb, indention + Indention, ball.WarpState);
                break;

            case BallMode.Missile:
                PrintMissileState(sb, indention + Indention, ball.MissileState);
                break;

            case BallMode.Formation:
                PrintFormationState(sb, indention + Indention, ball.FormationState);
                break;

            case BallMode.Follow:
                PrintFollowState(sb, indention + Indention, ball.FollowState);
                break;

            case BallMode.Mushroom:
                PrintMushroomState(sb, indention + Indention, ball.MushroomState);
                break;

            case BallMode.Troll:
                PrintTrollState(sb, indention + Indention, ball.TrollState);
                break;
        }

        if (ball.MiniBalls != null)
            foreach (MiniBall miniBall in ball.MiniBalls)
                PrintMiniBall(sb, indention + Indention, miniBall);
    }

    private static void PrintMiniBall(StringBuilder sb, string indention, MiniBall miniBall)
    {
        sb.AppendLine(indention + "[MiniBall]");
        sb.AppendLine(indention + Indention + "[Radius: " + miniBall.Radius + "]");
        sb.AppendLine(indention + Indention + "[Offset: " + miniBall.Offset + "]");
    }

    private static void PrintTrollState(StringBuilder sb, string indention, TrollState trollState)
    {
        sb.AppendLine(indention + "[TrollState]");
        sb.AppendLine(indention + Indention + "[Unk01: " + trollState.Unk01 + "]");
    }

    private static void PrintMushroomState(StringBuilder sb, string indention, MushroomState mushroomState)
    {
        sb.AppendLine(indention + "[MushroomState]");
        sb.AppendLine(indention + Indention + "[Unk01: " + mushroomState.Unk01 + "]");
        sb.AppendLine(indention + Indention + "[Unk02: " + mushroomState.Unk02 + "]");
        sb.AppendLine(indention + Indention + "[Unk03: " + mushroomState.Unk03 + "]");
        sb.AppendLine(indention + Indention + "[Unk04: " + mushroomState.Unk04 + "]");
    }

    private static void PrintFollowState(StringBuilder sb, string indention, FollowState followState)
    {
        sb.AppendLine(indention + "[FollowState]");
        sb.AppendLine(indention + Indention + "[UnkFollowId: " + followState.UnkFollowId + "]");
        sb.AppendLine(indention + Indention + "[UnkRange: " + followState.UnkRange + "]");
    }

    private static void PrintFormationState(StringBuilder sb, string indention, FormationState formationState)
    {
        sb.AppendLine(indention + "[FormationState]");
        sb.AppendLine(indention + Indention + "[Unk01: " + formationState.Unk01 + "]");
        sb.AppendLine(indention + Indention + "[Unk02: " + formationState.Unk02 + "]");
        sb.AppendLine(indention + Indention + "[Unk03: " + formationState.Unk03 + "]");
    }

    private static void PrintBallExtraHeader(StringBuilder sb, string indention, ExtraBallHeader extraHeader)
    {
        sb.AppendLine(indention + "[ExtraHeader]");
        sb.AppendLine(indention + Indention + "[AllianceId: " + extraHeader.AllianceId + "]");
        sb.AppendLine(indention + Indention + "[CorporationId: " + extraHeader.CorporationId + "]");
        sb.AppendLine(indention + Indention + "[CloakMode: " + extraHeader.CloakMode + "]");
        sb.AppendLine(indention + Indention + "[Harmonic: " + extraHeader.Harmonic + "]");
        sb.AppendLine(indention + Indention + "[Mass: " + extraHeader.Mass + "]");
    }

    private static void PrintMissileState(StringBuilder sb, string indention, MissileState missileState)
    {
        sb.AppendLine(indention + "[MissileState]");
        sb.AppendLine(indention + Indention + "[UnkFollowId: " + missileState.UnkFollowId + "]");
        sb.AppendLine(indention + Indention + "[UnkSourceId: " + missileState.UnkSourceId + "]");
        sb.AppendLine(indention + Indention + "[Unk01: " + missileState.Unk01 + "]");
        sb.AppendLine(indention + Indention + "[Unk02: " + missileState.Unk02 + "]");
        sb.AppendLine(indention + Indention + "[Unk03: " + missileState.Unk03 + "]");
    }

    private static void PrintWarpState(StringBuilder sb, string indention, WarpState warpState)
    {
        sb.AppendLine(indention + "[WarpState]");
        sb.AppendLine(indention + Indention + "[Location: " + warpState.Location + "]");
        sb.AppendLine(indention + Indention + "[OwnerId: " + warpState.OwnerId + "]");
        sb.AppendLine(indention + Indention + "[FollowId: " + warpState.FollowId + "]");
        sb.AppendLine(indention + Indention + "[EffectStamp: " + warpState.EffectStamp + "]");
        sb.AppendLine(indention + Indention + "[Unk01: " + warpState.Unk01 + "]");
    }

    private static void PrintGotoState(StringBuilder sb, string indention, GotoState gotoState)
    {
        sb.AppendLine(indention + "[GotoState]");
        sb.AppendLine(indention + Indention + "[Location: " + gotoState.Location + "]");
    }

    private static void PrintBallData(StringBuilder sb, string indention, BallData data)
    {
        sb.AppendLine(indention + "[Data]");
        sb.AppendLine(indention + Indention + "[Velocity: " + data.Velocity + "]");
        sb.AppendLine(indention + Indention + "[MaxVelocity: " + data.MaxVelocity + "]");
        sb.AppendLine(indention + Indention + "[SpeedFraction: " + data.SpeedFraction + "]");
        sb.AppendLine(indention + Indention + "[Unk03: " + data.Unk03 + "]");
    }

    private static void PrintBallHeader(StringBuilder sb, string indention, BallHeader header)
    {
        sb.AppendLine(indention + "[Header]");
        sb.AppendLine(indention + Indention + "[ItemId: " + header.ItemId + "]");
        sb.AppendLine(indention + Indention + "[Mode: " + header.Mode + " (" + (int) header.Mode + ")]");
        sb.AppendLine(indention + Indention + "[Flags: " + header.Flags + " (" + (int) header.Flags + ")]");
        sb.AppendLine(indention + Indention + "[Radius: " + header.Radius + "]");
        sb.AppendLine(indention + Indention + "[Location: " + header.Location + "]");
    }

    private static void PrintHeader(StringBuilder sb, string indention, Header header)
    {
        sb.AppendLine(indention + "[Destiny Header]");
        sb.AppendLine(indention + Indention + "[PacketType: " + header.PacketType + "]");
        sb.AppendLine(indention + Indention + "[Stamp: " + header.Stamp + "]");
    }
}
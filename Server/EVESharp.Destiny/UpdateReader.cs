using System.Collections.Generic;
using System.IO;
using System.Text;

namespace EVESharp.Destiny;

public class UpdateReader
{
    public Header      Header { get; private set; }
    public List <Ball> Balls  { get; private set; }

    public void Read (Stream str)
    {
        this.ReadHeader (str);
        Balls = new List <Ball> (5);
        while (str.Position < str.Length)
            Balls.Add (ReadBallFromStream (str));
    }

    private static Ball ReadBallFromStream (Stream str)
    {
        Ball ret = new Ball {Header = str.ReadStruct <BallHeader> ()};

        if (ret.Header.Mode != BallMode.Rigid)
            ret.ExtraHeader = str.ReadStruct <ExtraBallHeader> ();

        if (ret.Header.Flags.HasFlag (BallFlag.IsFree))
            ret.Data = str.ReadStruct <BallData> ();

        BinaryReader reader = new BinaryReader (str);
        ret.FormationId = reader.ReadByte ();

        switch (ret.Header.Mode)
        {
            case BallMode.Follow:
            case BallMode.Orbit:
                ret.FollowState = str.ReadStruct <FollowState> ();

                break;

            case BallMode.Formation:
                ret.FormationState = str.ReadStruct <FormationState> ();

                break;

            case BallMode.Troll:
                ret.TrollState = str.ReadStruct <TrollState> ();

                break;

            case BallMode.Missile:
                ret.MissileState = str.ReadStruct <MissileState> ();

                break;

            case BallMode.Goto:
                ret.GotoState = str.ReadStruct <GotoState> ();

                break;

            case BallMode.Warp:
                ret.WarpState = str.ReadStruct <WarpState> ();

                break;

            case BallMode.Mushroom:
                ret.MushroomState = str.ReadStruct <MushroomState> ();

                break;

            case BallMode.Stop:
            case BallMode.Field:
            case BallMode.Rigid:
                // no extra data for these
                break;
        }

        if (ret.Header.Flags.HasFlag (BallFlag.HasMiniBalls))
            ret.MiniBalls = ReadMiniBalls (reader);

        // Crucible:
        // no more names in destiny data
        // most of them were invalid anyway, and slimitems have the name, so sensible change for CCP
        //ret.Name = ReadString(reader);

        return ret;
    }

    private static string ReadString (BinaryReader reader)
    {
        byte nameWords = reader.ReadByte ();

        if (nameWords > 0)
        {
            byte [] rawName = reader.ReadBytes (nameWords * 2);

            return Encoding.Unicode.GetString (rawName);
        }

        return null;
    }

    private void ReadHeader (Stream str)
    {
        Header = str.ReadStruct <Header> ();

        if (Header.PacketType != 0 && Header.PacketType != 1)
            throw new InvalidDataException ("Unknown packet type; expected 0 or 1, got " + Header.PacketType);
    }

    private static MiniBall [] ReadMiniBalls (BinaryReader reader)
    {
        int         extraCount = reader.ReadInt16 ();
        MiniBall [] ret        = new MiniBall[extraCount];
        if (extraCount > 0)
            for (int i = 0; i < extraCount; i++)
                ret [i] = reader.ReadStruct <MiniBall> ();

        return ret;
    }
}
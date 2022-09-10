using EVESharp.Types.Serialization;
using NUnit.Framework;

namespace EVESharp.Types.Unit.Marshaling;

public class TokenMarshalingTests
{
    private static string sTokenMarshal_Empty = "";
    private static string sTokenMarshal_Normal = "Test";
    private static string sTokenMarshal_TooLong = "pC78GCVf2cAkVxDxALVQnW7T3hAR2D6T8nZg7vPc75t38n7E3RLbx7cBYFVP2BfWeUMKLXeXqfmmKr464cc8J8MsmbXZ9NsAt4YLDYDFgrgPY9vnVJ3cXdz4Q9ukjtguekrBfNdFCHhTL95WYcq4TyVRbNTkdvGgvkjXgR4PD4Jc9AFtWJcQfPcqk9qrvNDrzuHh6VnUjhJ8P6LKTbf4E74mBFJkQBbNV53P2qKTVBsuRt5MTnvvPBxaEb8prfxX6W6E9Ayh";

    private static byte[] sTokenMarshal_EmptyBuffer  = new byte[] {0x02, 0x00};
    private static byte[] sTokenMarshal_NormalBuffer = new byte[] {0x02, 0x04, 0x54, 0x65, 0x73, 0x74};
        
    [Test]
    public void TokenMarshaling_Empty()
    {
        PyToken token = new PyToken(sTokenMarshal_Empty);

        byte[] output = Marshal.ToByteArray(token, false);
            
        Assert.AreEqual(sTokenMarshal_EmptyBuffer, output);
    }
        
    [Test]
    public void TokenMarshaling_Normal()
    {
        PyToken token = new PyToken(sTokenMarshal_Normal);

        byte[] output = Marshal.ToByteArray(token, false);
            
        Assert.AreEqual(sTokenMarshal_NormalBuffer, output);
    }
        
    [Test]
    public void TokenMarshaling_TooLong()
    {
        PyToken token = new PyToken(sTokenMarshal_TooLong);

        Assert.Catch(() => Marshal.ToByteArray(token, false));
    }

    [Test]
    public void TokenUmarshal_Empty()
    {
        PyDataType result = Unmarshal.ReadFromByteArray(sTokenMarshal_EmptyBuffer, false);
            
        Assert.IsInstanceOf<PyToken>(result);

        PyToken pyToken = result as PyToken;
            
        Assert.AreEqual(sTokenMarshal_Empty.Length, pyToken.Length);
        Assert.AreEqual(sTokenMarshal_Empty,        pyToken.Token);
    }

    [Test]
    public void TokenUmarshal_Normal()
    {
        PyDataType result = Unmarshal.ReadFromByteArray(sTokenMarshal_NormalBuffer, false);
            
        Assert.IsInstanceOf<PyToken>(result);

        PyToken pyToken = result as PyToken;
            
        Assert.AreEqual(sTokenMarshal_Normal.Length, pyToken.Length);
        Assert.AreEqual(sTokenMarshal_Normal,        pyToken.Token);
    }
}
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace EVESharp.Destiny;

public static class Utility
{
    public static T ReadStruct <T> (this BinaryReader s)
    {
        return ReadStructure <T> (s.BaseStream);
    }

    public static T ReadStruct <T> (this Stream s)
    {
        return ReadStructure <T> (s);
    }

    public static T ReadStructure <T> (this Stream s)
    {
        return ReadStructure <T> (s, Marshal.SizeOf (typeof (T)));
    }

    public static T ReadStructure <T> (this Stream s, int size)
    {
        return (T) ReadStructure (s, typeof (T), size);
    }

    public static object ReadStructure (this Stream s, Type stype, int size)
    {
        BinaryReader reader = new BinaryReader (s);
        byte []      data   = reader.ReadBytes (size);

        if (data.Length < size)
            throw new Exception ("Not enough data");

        IntPtr nativeMemory = IntPtr.Zero;

        try
        {
            nativeMemory = Marshal.AllocHGlobal (size);

            if (nativeMemory == IntPtr.Zero)
                throw new Exception ("Failed to allocate unmanaged memory");

            Marshal.Copy (data, 0, nativeMemory, size);
            object result = Marshal.PtrToStructure (nativeMemory, stype);

            if (result == null)
                throw new Exception ("PtrToStructure failed");

            return result;
        }
        finally
        {
            if (nativeMemory != IntPtr.Zero)
                Marshal.FreeHGlobal (nativeMemory);
        }
    }

    public static uint ToBigEndian (uint source)
    {
        return (source >> 24) |
               ((source << 8) & 0x00FF0000) |
               ((source >> 8) & 0x0000FF00) |
               (source << 24);
    }

    public static ushort ToBigEndian (ushort source)
    {
        return (ushort) ((source >> 8) | (source << 8));
    }

    public static byte [] StringToByteArray (string hex)
    {
        byte [] ret = new byte[hex.Length / 2];
        for (int i = 0; i < hex.Length / 2; i++)
            ret [i] = Convert.ToByte (hex.Substring (i * 2, 2), 16);

        return ret;
    }

    public static string ByteArrayToString (byte [] data)
    {
        StringBuilder r = new StringBuilder (data.Length * 2);
        foreach (byte b in data)
            r.Append (b.ToString ("X2"));

        return r.ToString ();
    }

    public static void ReadFully (this Stream stream, byte [] buffer, int index, int length)
    {
        int left = length;

        while (left > 0)
        {
            int read = stream.Read (buffer, index, left);
            left  -= read;
            index += read;
        }
    }

    public static void ReadFully (this Stream stream, byte [] buffer)
    {
        ReadFully (stream, buffer, 0, buffer.Length);
    }

    public static uint ReadSizeEx (this BinaryReader reader)
    {
        byte len = reader.ReadByte ();

        if (len == 0xFF)
            return reader.ReadUInt32 ();

        return len;
    }

    public static void WriteSizeEx (this BinaryWriter writer, uint len)
    {
        if (len < 0xFF)
        {
            writer.Write ((byte) len);
        }
        else
        {
            writer.Write ((byte) 0xFF);
            writer.Write (len);
        }
    }

    public static void WriteSizeEx (this BinaryWriter writer, int len)
    {
        WriteSizeEx (writer, (uint) len);
    }

    public static string HexDump (byte [] bytes)
    {
        if (bytes == null) return "<null>";

        int           len    = bytes.Length;
        StringBuilder result = new StringBuilder ((len + 15) / 16 * 78);
        char []       chars  = new char[78];
        // fill all with blanks
        for (int i = 0; i < 75; i++)
            chars [i] = ' ';
        chars [76] = '\r';
        chars [77] = '\n';

        for (int i1 = 0; i1 < len; i1 += 16)
        {
            chars [0] = HexChar (i1 >> 28);
            chars [1] = HexChar (i1 >> 24);
            chars [2] = HexChar (i1 >> 20);
            chars [3] = HexChar (i1 >> 16);
            chars [4] = HexChar (i1 >> 12);
            chars [5] = HexChar (i1 >> 8);
            chars [6] = HexChar (i1 >> 4);
            chars [7] = HexChar (i1 >> 0);

            int offset1 = 11;
            int offset2 = 60;

            for (int i2 = 0; i2 < 16; i2++)
            {
                if (i1 + i2 >= len)
                {
                    chars [offset1]     = ' ';
                    chars [offset1 + 1] = ' ';
                    chars [offset2]     = ' ';
                }
                else
                {
                    byte b = bytes [i1 + i2];
                    chars [offset1]     = HexChar (b >> 4);
                    chars [offset1 + 1] = HexChar (b);
                    chars [offset2]     = b < 32 ? '·' : (char) b;
                }

                offset1 += i2 == 7 ? 4 : 3;
                offset2++;
            }

            result.Append (chars);
        }

        return result.ToString ();
    }

    private static char HexChar (int value)
    {
        value &= 0xF;

        if (value >= 0 && value <= 9)
            return (char) ('0' + value);

        return (char) ('A' + (value - 10));
    }

    /// <summary>
    /// Reads the contents of the stream into a byte array.
    /// data is returned as a byte array. An IOException is
    /// thrown if any of the underlying IO calls fail.
    /// </summary>
    /// <param name="source">The stream to read.</param>
    /// <returns>A byte array containing the contents of the stream.</returns>
    /// <exception cref="NotSupportedException">The stream does not support reading.</exception>
    /// <exception cref="ObjectDisposedException">Methods were called after the stream was closed.</exception>
    /// <exception cref="System.IO.IOException">An I/O error occurs.</exception>
    public static byte [] ReadAllBytes (this Stream source)
    {
        byte [] readBuffer = new byte[4096];

        int totalBytesRead = 0;
        int bytesRead;

        while ((bytesRead = source.Read (readBuffer, totalBytesRead, readBuffer.Length - totalBytesRead)) > 0)
        {
            totalBytesRead += bytesRead;

            if (totalBytesRead == readBuffer.Length)
            {
                int nextByte = source.ReadByte ();

                if (nextByte != -1)
                {
                    byte [] temp = new byte[readBuffer.Length * 2];
                    Buffer.BlockCopy (readBuffer, 0, temp, 0, readBuffer.Length);
                    Buffer.SetByte (temp, totalBytesRead, (byte) nextByte);
                    readBuffer = temp;
                    totalBytesRead++;
                }
            }
        }

        byte [] buffer = readBuffer;

        if (readBuffer.Length != totalBytesRead)
        {
            buffer = new byte[totalBytesRead];
            Buffer.BlockCopy (readBuffer, 0, buffer, 0, totalBytesRead);
        }

        return buffer;
    }
}
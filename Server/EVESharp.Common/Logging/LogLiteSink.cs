using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Text;
using EVESharp.Common.Configuration;
using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;

namespace EVESharp.Common.Logging;

public class LogLiteSink : ILogEventSink, IDisposable
{
    enum ConnectionMessage
    {
        CONNECTION_MESSAGE,
        SIMPLE_MESSAGE,
        LARGE_MESSAGE,
        CONTINUATION_MESSAGE,
        CONTINUATION_END_MESSAGE,
    }

    enum Severity
    {
        SEVERITY_INFO,
        SEVERITY_NOTICE,
        SEVERITY_WARN,
        SEVERITY_ERR,
    }

    private static readonly Dictionary<LogEventLevel, Severity> MessageTypeToSeverity = new Dictionary<LogEventLevel, Severity>()
    {
        {LogEventLevel.Information, Severity.SEVERITY_INFO},
        {LogEventLevel.Debug, Severity.SEVERITY_INFO},
        {LogEventLevel.Error, Severity.SEVERITY_ERR},
        {LogEventLevel.Fatal, Severity.SEVERITY_ERR},
        {LogEventLevel.Verbose, Severity.SEVERITY_NOTICE},
        {LogEventLevel.Warning, Severity.SEVERITY_WARN}
    };

    private const int PROTOCOL_VERSION = 2;

    private readonly LogLite mConfiguration;
    private readonly Socket  mSocket;
    
    private string Name           { get; }
    private string ExecutablePath { get; }
    private long   PID            { get; }

    public LogLiteSink (LogLite configuration)
    {
        this.mConfiguration = configuration;
        
        // fill in important information
        this.PID            = Process.GetCurrentProcess ().Id;
        this.ExecutablePath = Process.GetCurrentProcess ().ProcessName;
        this.Name           = Process.GetCurrentProcess ().MachineName;

        // setup the socket for the loglite server
        this.mSocket = new Socket (AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
        this.mSocket.SetSocketOption (SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, false);
        this.mSocket.Connect (this.mConfiguration.Hostname, int.Parse (this.mConfiguration.Port));
        
        // send the first message so the loglite server authorizes us
        this.SendConnectionMessage ();
    }
    
    private void SendConnectionMessage()
    {
        // prepare the machineName and executablePath
        byte[] machineName = new byte[32];
        Encoding.ASCII.GetBytes(this.Name, 0, Math.Min(31, this.Name.Length), machineName, 0);

        byte[] executablePath = new byte[260];
        Encoding.ASCII.GetBytes(this.ExecutablePath, 0, Math.Min(259, this.ExecutablePath.Length), executablePath, 0);

        MemoryStream stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);

        using (stream)
        using (writer)
        {
            writer.Write((int) ConnectionMessage.CONNECTION_MESSAGE);
            writer.Write((int) 0);
            writer.Write((uint) PROTOCOL_VERSION);
            writer.Write((int) 0);
            writer.Write((long) this.PID);
            writer.Write(machineName);
            writer.Write(executablePath);
            // fill the packet with empty data to fill the 344 size in packets
            writer.Write(new byte[344 - (4 + 4 + 4 + 4 + 8 + 32 + 260)]);

            this.mSocket.Send(stream.ToArray());
        }
    }

    private void SendTextMessage(LogEventLevel type, DateTimeOffset time, string origin, string message)
    {
        byte[] module = new byte[32];
        byte[] channel = new byte[32];
        byte[] byteMessage = new byte[256];

        Encoding.ASCII.GetBytes(origin,   0, Math.Min(31,   origin.Length),  channel,     0);
        Encoding.ASCII.GetBytes (message, 0, Math.Min (255, message.Length), byteMessage, 0);

        MemoryStream stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);

        using (stream)
        using (writer)
        {
            if (message.Length > 255)
            {
                int offset = 255;

                writer.Write((uint) ConnectionMessage.LARGE_MESSAGE);
                writer.Write((int) 0);
                writer.Write((ulong) time.ToUnixTimeMilliseconds());
                writer.Write((uint) MessageTypeToSeverity[type]);
                writer.Write(module);
                writer.Write(channel);
                writer.Write(byteMessage);
                // fill the packet with empty data to fill the 344 size in packets
                writer.Write(new byte[344 - (4 + 4 + 8 + 4 + 32 + 32 + 256)]);

                while (offset < message.Length)
                {
                    byteMessage = new byte[256];
                    Encoding.ASCII.GetBytes(message, offset, Math.Min(255, message.Length - offset), byteMessage, 0);
                    
                    if ((message.Length - offset) > 255)
                        writer.Write((uint) ConnectionMessage.CONTINUATION_MESSAGE);
                    else
                        writer.Write((uint) ConnectionMessage.CONTINUATION_END_MESSAGE);

                    writer.Write((int) 0);
                    writer.Write((ulong) time.ToUnixTimeMilliseconds());
                    writer.Write((uint) MessageTypeToSeverity[type]);
                    writer.Write(module);
                    writer.Write(channel);
                    writer.Write(byteMessage);
                    // fill the packet with empty data to fill the 344 size in packets
                    writer.Write(new byte[344 - (4 + 4 + 8 + 4 + 32 + 32 + 256)]);

                    offset += 255;
                }
            }
            else
            {
                writer.Write((uint) ConnectionMessage.SIMPLE_MESSAGE);
                writer.Write((int) 0);
                writer.Write((ulong) time.ToUnixTimeMilliseconds());
                writer.Write((uint) MessageTypeToSeverity[type]);
                writer.Write(module);
                writer.Write(channel);
                writer.Write(byteMessage);
                // fill the packet with empty data to fill the 344 size in packets
                writer.Write(new byte[344 - (4 + 4 + 8 + 4 + 32 + 32 + 256)]);
            }

            this.mSocket.Send(stream.ToArray());
        }
    }

    public void Emit (LogEvent logEvent)
    {
        // prevent long messages from being sent as they tend to be a big issue with loglite server
        string message = logEvent.RenderMessage ();
        string name    = "Program";
        
        if (message.Length > 6000)
        {
            message = message.Substring(0, 2048) + "\n[...]\n" + message.Substring(message.Length - 2048);
        }

        if (
            logEvent.Properties.TryGetValue ("Name",          out LogEventPropertyValue prop) == true ||
            logEvent.Properties.TryGetValue ("SourceContext", out prop) == true
        )
        {
            string value = prop.ToString ();
            name = value.Substring (value.LastIndexOf ('.') + 1).TrimEnd ('"');
        }
        
        this.SendTextMessage (logEvent.Level, logEvent.Timestamp, name, message);
    }

    public void Dispose ()
    {
        this.mSocket.Dispose ();
    }
}

public static class LogLiteSinkExtension
{
    public static LoggerConfiguration LogLite (this LoggerSinkConfiguration sinkConfiguration, LogLite configuration)
    {
        return sinkConfiguration.Sink (new LogLiteSink (configuration));
    }
}
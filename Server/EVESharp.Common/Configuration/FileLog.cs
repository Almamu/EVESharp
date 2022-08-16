using EVESharp.Common.Configuration.Attributes;

namespace EVESharp.Common.Configuration;

[ConfigSection("logfile", true)]
public class FileLog
{
    private string mLogFile;
    [ConfigValue("directory")]
    public virtual string Directory { get; set; }
    [ConfigValue ("logfile")]
    public virtual string LogFile
    {
        get => this.mLogFile;
        set
        {
            this.mLogFile = value;
            this.Enabled  = true;
        }
    }
    public virtual bool Enabled { get; set; } = false;
}
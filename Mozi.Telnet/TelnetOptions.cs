using System;

namespace Mozi.Telnet
{


    public enum TelnetCommand
    {
        EOF = 0xEC,        // 文件结束符
        SUSP = 0xED,	     // 挂起当前进程（作业控制）
        ABORT = 0xEE,	 // 异常中止进程
        EOR = 0xEF,       // 记录结束符
        SE = 0xF0,	     // 自选项结束
        NOP = 0xF1,	     // 无操作
        DM	= 0xF2,	     // 数据标记
        BRK	= 0xF3,	     // 中断
        IP	= 0xF4,	     // 中断进程
        AO	= 0xF5,	     // 异常中止输出
        AYT	= 0xF6,	     // 对方是否还在运行？
        EC	= 0xF7,	     // 转义字符
        EL	= 0xF8,	     // 删除行
        GA	= 0xF9,	     // 继续进行
        SB = 0xFA,        // (FA)	子选项开始
        WILL = 0xFB,      // (FB)	同意启动（enable）选项
        WONT = 0xFC,      // (FC)	拒绝启动选项
        DO = 0xFD,        // (FD)	认可选项请求
        DONT = 0xFE,      // (FE)	拒绝选项请求
        IAC=0xFF          // (IAC)  指令开始位
    }

    public enum TelnetSubOptions
    {
        BINARY = 0x00,           // Binary Transmission
        ECHO = 0x01,             // Echo
        RECONNECT = 0x02,        // Reconnection
        SGA = 0x03,              // Suppress Go Ahead
        AMSN = 0x04,             // Approx Message Size Negotiation
        STATUS = 0x05,           // Status
        TIMING_MARK = 0x06,      // Timing Mark
        RCTE = 0x07,             // Remote Controlled Trans and Echo -BELL
        OLW = 0x08,              // Output Line Width
        OPS = 0x09,              // Output Page Size
        OCRD = 0x0A,             // Output Carriage-Return Disposition
        OHTS = 0x0B,             // Output Horizontal Tab Stops
        OHTD = 0x0C,             // Output Horizontal Tab Disposition
        OFD = 0x0D,              // Output Formfeed Disposition
        OVT = 0x0E,              // Output Vertical Tabstops
        OVTD = 0x0F,             // Output Vertical Tab Disposition
        OLD = 0x10,              // Output Linefeed Disposition
        EA = 0x11,               // Extended ASCII
        LOGOUT = 0x12,           // Logout
        BYTE_MACRO = 0x13,       // Byte Macro
        DET = 0x14,              // Data Entry Terminal
        SUPDUP = 0x15,           // SUPDUP
        SUPDUP_OUTPUT = 0x16,    // SUPDUP Output
        SL = 0x17,               // Send Location
        TERMTYPE = 0x18,         // Terminal Type
        EOR = 0x19,              // End of Record
        TACACS_ID = 0x1A,        // TACACS User Identification
        OM = 0x1B,               // Output Marking
        TLN = 0x1C,              // Terminal Location Number
        REGIME3270 = 0x1D,       // 3270 regime
        X3PAD = 0x1E,            // X.3 PAD
        NAWS = 0x1F,             // Window size
        TERM_SPEED = 0x20,       // Terminal speed
        RFLOW = 0x21,            // Remote flow control
        LINEMODE = 0x22,         // Linemode option
        XDISPLOC = 0x23,         // X Display Location
        ENV = 0x24,              // Environment
        AUTH = 0x25,             // Authenticate
        ENCRYPT = 0x26,          // Encryption option
        NEWENV = 0x27,
        TN3270E = 0x28,
        XAUTH = 0x29,
        CHARSET = 0x2A,
        RSP = 0x2B,
        COMPORT = 0x2C,
        SUPLOCALECHO = 0x2D,
        STARTTLS = 0x2E,
        KERMIT = 0x2F,
        SEND_URL = 0x30,
        FORWARD_X = 0x31,
        // 50-137 = Unassigned
        PRAGMA_LOGON = 0x8A,
        SSPI_LOGON = 0x8B,
        PRAGMA_HEARTBEAT = 0x8C,
        EOL = 0xFF,                 // Extended-Options-List
        // Sub options
        TERM_IS = 0x00,
        TERMTYPE_SEND = 0x01,       // Sub option
        REPLY = 0x02,
        NAME = 0x03,
    }
    //Auth commands
    public enum AuthCommand {       
        IS = 0x00,
        SEND = 0x01,
        REPLY = 0x02,
        NAME = 0x03,
    }
    public enum AuthOptions {
        // Auth options  0x25
        NULL = 0x00,
        KERBEROS_V4 = 0x01,
        KERBEROS_V5 = 0x02,
        SPX = 0x03,
        MINK = 0x04,
        SRP = 0x05,
        RSA = 0x06,
        SSL = 0x07,
        LOKI = 0x0A,
        SSA = 0x0B,
        KEA_SJ = 0x0C,
        KEA_SJ_INTEG = 0x0D,
        DSS = 0x0E,
        NTLM = 0x0F,

        //Kerberos4 Telnet Authentication suboption commands
        KRB4_AUTH = 0x00,
        KRB4_REJECT = 0x01,
        KRB4_ACCEPT = 0x02,
        KRB4_CHALLENGE = 0x03,
        KRB4_RESPONSE = 0x04,
        KRB4_FORWARD = 0x05,
        KRB4_FORWARD_ACCEPT = 0x06,
        KRB4_FORWARD_REJECT = 0x07,
        KRB4_EXP = 0x08,
        KRB4_PARAMS = 0x09,

        //Kerberos5 Telnet Authentication suboption commands
        KRB5_AUTH = 0x00,
        KRB5_REJECT = 0x01,
        KRB5_ACCEPT = 0x02,
        KRB5_RESPONSE = 0x03,
        KRB5_FORWARD = 0x04,
        KRB5_FORWARD_ACCEPT = 0x05,
        KRB5_FORWARD_REJECT = 0x06,

        //DSS Telnet Authentication suboption commands
        DSS_INITIALIZE = 0x01,
        DSS_TOKENBA = 0x02,
        DSS_CERTA_TOKENAB = 0x03,
        DSS_CERTB_TOKENBA2 = 0x04,

        //SRP Telnet Authentication suboption commands
        SRP_AUTH = 0x00,
        SRP_REJECT = 0x01,
        SRP_ACCEPT = 0x02,
        SRP_CHALLENGE = 0x03,
        SRP_RESPONSE = 0x04,
        SRP_EXP = 0x08,
        SRP_PARAMS = 0x09,

        // KEA_SJ and KEA_SJ_INTEG Telnet Authenticatio suboption commands
        KEA_CERTA_RA = 0x01,
        KEA_CERTB_RB_IVB_NONCEB = 0x02,
        KEA_IVA_RESPONSEB_NONCEA = 0x03,
        KEA_RESPONSEA = 0x04,
    }
        //Telnet Encryption Types (Option 38)
    public enum AuthEncryptType { 
        //  commands
        IS = 0x00,
        SUPPORT = 0x01,
        REPLY = 0x02,
        START = 0x03,
        END = 0x04,
        REQUEST_START = 0x05,
        REQUEST_END = 0x06,
        ENC_KEYID = 0x07,
        DEC_KEYID = 0x08,
        //  types
        NULL = 0x00,
        DES_CFB64 = 0x01,
        DES_OFB64 = 0x02,
        DES3_CFB64 = 0x03,
        DES3_OFB64 = 0x04,
        CAST5_40_CFB64 = 0x08,
        CAST5_40_OFB64 = 0x09,
        CAST128_CFB64 = 0x0A,
        CAST128_OFB64 = 0x0B,
        AES_CCM = 0x0C,

        //DES3_CFB64 Telnet Encryption type suboption commands
        CFB64_IV = 0x01,
        CFB64_IV_OK = 0x02,
        CFB64_IV_BAD = 0x03,

        //CAST5_40_OFB64 and CAST128_OFB64 Telnet Encryption types suboption commands
        OFB64_IV = 0x01,
        OFB64_IV_OK = 0x02,
        OFB64_IV_BAD = 0x03,

        //CAST5_40_CFB64 and CAST128_CFB64 Telnet Encryption types suboption commands
        //same as DES3_CFB64 Telnet Encryption type suboption commands

        //DES_CFB64 Telnet Encryption type
        //same as DES3_CFB64 Telnet Encryption type suboption commands

        //DES_OFB64 Telnet Encryption type
        //same as CAST5_40_OFB64 and CAST128_OFB64 Telnet Encryption types suboption commands
    }

    /// <summary>
    /// 用户登陆信息
    /// </summary>
    public class UserInfo
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Token { get; set; }        
        public bool IsValid { get; set; }
        public int TryCount { get; set; }
        public bool Login { get; set; }
        public DateTime LoginTime { get; set; }
        public DateTime LogoutTime { get; set; }

        public void Retry()
        {
            UserName = "";
            Password = "";
            Token = "";
            IsValid = false;
        }
    }

    /// <summary>
    /// 会话对象
    /// </summary>
    public class Session
    {
        /// <summary>
        /// 会话标识
        /// </summary>
        public string Id { get; set; }
        public string TerminalType { get; set; }
        public UserInfo User { get; set; }
        public bool Echo { get; set; }
        public string InputBuffer { get; set; }
        /// <summary>
        /// 会话开始时间
        /// </summary>
        public DateTime StartTime { get; set; }
        /// <summary>
        /// 会话结束时间
        /// </summary>
        public DateTime StopTime { get; set; }
        /// <summary>
        /// 会话丢失时间
        /// </summary>
        public DateTime LostTime { get; set; }
        /// <summary>
        /// 会话是否丢失
        /// </summary>
        public bool Lost { get; set; }

        public void ResetInput()
        {
            InputBuffer = "";
        }
    }
}

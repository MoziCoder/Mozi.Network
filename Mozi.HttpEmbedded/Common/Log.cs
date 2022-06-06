using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Mozi.HttpEmbedded.Common
{
    //TODO 对日志进行统一管理 用户动作的记录 日志的存储
    /// <summary>
    /// 记录类
    /// </summary>
    public static class Log
    {
        private static readonly string LogDir = AppDomain.CurrentDomain.BaseDirectory + @"Log\";

        private static readonly ReaderWriterLockSlim _writeLock = new ReaderWriterLockSlim();
        /// <summary>
        /// 默认日志文件扩展名
        /// </summary>
        private const string LogFileExt = ".log";
        /// <summary>
        /// 默认记录器名
        /// </summary>
        private static readonly string DefaultLoggerName = "error";
        /// <summary>
        /// 追加式写入日志
        /// </summary>
        /// <param name="name"></param>
        /// <param name="info"></param>
        /// <param name="level"></param>
        public static void Save(string name, string info, LogLevel level)
        {
            DirectoryInfo dir = new DirectoryInfo(LogDir);
            if (!dir.Exists)
            {
                dir.Create();
            }
            Parallel.Invoke(() =>
            {
                try
                {
                    _writeLock.EnterWriteLock();
                    StreamWriter sw = new StreamWriter(LogDir + name + LogFileExt, true);
                    string loginfo = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + Enum.GetName(typeof(LogLevel), level) + " | " + info;
                    sw.WriteLine(loginfo);
                    Console.WriteLine(loginfo);
                    sw.Flush();
                    sw.Close();
                }
                finally
                {
                    _writeLock.ExitWriteLock();
                }
            });
        }
        /// <summary>
        /// 自定义提示
        /// </summary>
        /// <param name="name"></param>
        /// <param name="info"></param>
        public static void Save(string name, string info)
        {
            Save(name, info, LogLevel.Info);
        }
        /// <summary>
        /// 提示
        /// </summary>
        /// <param name="info"></param>
        public static void Info(string info)
        {
            Save(DefaultLoggerName, info, LogLevel.Info);
        }
        /// <summary>
        /// 错误
        /// </summary>
        /// <param name="info"></param>
        public static void Error(string info)
        {
            Save(DefaultLoggerName, info, LogLevel.Error);
        }
        /// <summary>
        /// 调试
        /// </summary>
        /// <param name="info"></param>
        public static void Debug(string info)
        {
            Save(DefaultLoggerName, info, LogLevel.Debug);
        }
        /// <summary>
        /// 警告
        /// </summary>
        /// <param name="info"></param>
        public static void Warn(string info)
        {
            Save(DefaultLoggerName, info, LogLevel.Warn);
        }
        /// <summary>
        /// 日志级别
        /// </summary>
        public enum LogLevel
        {
            Error = 1,
            Debug = 2,
            Info = 3,
            Warn = 4
        }
    }
}

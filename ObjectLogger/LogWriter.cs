using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Compression;

namespace ObjectLogger
{
    public class LogWriter
    {
        private static DateTime dayBefore = DateTime.UtcNow;
        private static async Task LogAsync(string sFilePath, string sErrMsg)
        {
            try
            {
                byte[] encodedText = Encoding.UTF8.GetBytes(sErrMsg);
                using (FileStream fs = new FileStream(sFilePath, FileMode.Append,
                    FileAccess.Write, FileShare.None,
                    bufferSize: 4096, useAsync: true))
                {
                    await fs.WriteAsync(encodedText, 0, encodedText.Length);
                };
            }
            catch (Exception)
            {
                throw;
            }
        }
        public static void LogActivity(string message, string status = "")
        {
            try
            {
                //string LogFolderPath = ConfigurationManager.AppSettings["LogFile"];
                string LogFolderPath = null;
                if (string.IsNullOrEmpty(LogFolderPath))
                {
                    LogFolderPath = Environment.CurrentDirectory;
                }
                if (!Directory.Exists(LogFolderPath))
                {
                    DirectoryInfo di = Directory.CreateDirectory(LogFolderPath);
                }

                string date = "appLog_" + DateTime.UtcNow.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                string dateFolder = Path.Combine(LogFolderPath, date);
                LogFolderPath = dateFolder + ".log";
                ArchiveToZipLogFiles();

                _ = LogAsync(LogFolderPath, GetLogMessage(message, status));
            }
            catch (Exception)
            {
            }
        }

        private static string GetLogMessage(string message, string status)
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.Append("\n---------------------------- Start -----------------------------------\n" + Environment.NewLine);
            stringBuilder.Append("Time : " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString() + Environment.NewLine);
            stringBuilder.Append("Status : " + status + Environment.NewLine);
            stringBuilder.Append("Message : " + message + Environment.NewLine);
            stringBuilder.Append("\n---------------------------- End -----------------------------------\n" + Environment.NewLine);
            return stringBuilder.ToString();
        }
        #region ArchiveToZipLogFiles
        private static void ArchiveToZipLogFiles()
        {
            if (dayBefore.Day >= DateTime.UtcNow.Day)
            {
                dayBefore = DateTime.UtcNow;

                string pathFolder = Environment.CurrentDirectory;
                string pathLogsZip = Path.Combine(pathFolder, "logs.zip");

                if (!File.Exists(pathLogsZip))
                {
                    using (var archives = ZipFile.Open(pathLogsZip, ZipArchiveMode.Create)) { };
                }

                IEnumerable<string> di = Directory.EnumerateFiles(path: pathFolder);
                var logFiles = di.Where(lg => lg.EndsWith(".log"));

                if (logFiles != null)
                {
                    List<string> creationFileTimeFilter = GetLogFilesWithoutTodayFile(logFiles);
                    if (IsAnyFiles(creationFileTimeFilter.Count))
                    {
                        using var archive = ZipFile.Open(pathLogsZip, ZipArchiveMode.Update);
                        foreach (string log in creationFileTimeFilter)
                        {
                            archive.CreateEntryFromFile(log, "archive-" + log, CompressionLevel.Optimal);
                            File.Delete(Path.Combine(pathFolder, log));
                        }
                    }
                }
            }
        }

        private static bool IsAnyFiles(int listCount)
        {
            return listCount > 0;
        }

        private static List<string> GetLogFilesWithoutTodayFile(IEnumerable<string> logFiles)
        {
            List<string> creationFileTimeFilter = new List<string>();
            foreach (string logFileItem in logFiles)
            {
                if (Directory.GetCreationTimeUtc(logFileItem).Date < DateTime.UtcNow.Date)
                {
                    creationFileTimeFilter.Add(Path.GetFileName(logFileItem));
                }
            }

            return creationFileTimeFilter;
        }
        #endregion
    }
}

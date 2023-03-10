using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wrench.Model
{
    internal class ModemReflasher
    {
        private const string logfile = ".\\Log\\tfi.log";

        public static bool TryRestoreFirmware(Action<string> logger, string reflasherDirectory)
        {
            var reflasher = Directory.GetFiles(reflasherDirectory, "LE910C*.exe").FirstOrDefault();

            var logfilepath = Path.Combine(Environment.CurrentDirectory, logfile);

            if (string.IsNullOrEmpty(reflasher) || !File.Exists(reflasher))
                throw new FileNotFoundException("Reflasher does not exist in working directory");

            if (!File.Exists(logfilepath))
            {
                if (!Directory.Exists(Path.GetDirectoryName(logfilepath))) Directory.CreateDirectory(Path.GetFullPath(logfilepath));
                File.Create(logfilepath).Close();
            }

            using var reflashProcess = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = $"{reflasher}",
                    Arguments = $"--logfile {logfilepath} -a",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };

            reflashProcess.OutputDataReceived += ReadData;

            reflashProcess.Start();
            reflashProcess.BeginOutputReadLine();
            reflashProcess.WaitForExit();
            reflashProcess.CancelOutputRead();

            return reflashProcess.ExitCode == 0;

            void ReadData(object sender, DataReceivedEventArgs e) => LogImportant(e.Data?.ToString(), logger);
        }

        private static void LogImportant(string? v, Action<string> logger)
        {
            var fields = new ReflasherAnswers().GetType().GetFields();

            foreach (var f in fields)
            {
                var expected = f.GetValue(fields)?.ToString();
                if (v?.Contains(expected ?? string.Empty) ?? false) logger(v);
            }
        }

        private struct ReflasherAnswers
        {
            public const string doneMsg = "all done";
            public const string elapsedMsg = "elapsed";
            public const string binariesMsg = "binaries";
            public const string successMsg = "successful";
            public const string sendingMsg = "sending";
            public const string writingMsg = "writing";
            public const string finalizingMsg = "finalizing";
            public const string rebootingMsg = "rebooting";
            public const string waitingMsg = "waiting";
            public const string usingMsg = "using";
            public const string factorydefaultsMsg = "factory defaults";
            public const string failMsg = "fail";
        }
    }
}

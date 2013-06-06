﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using PlasmaShared;

namespace Benchmarker
{
    public class SpringRun
    {
        StringBuilder LogLines;
        Process process;

        public Action<string> LineAdded = s => { };

        public void Abort() {
            if (process != null) process.Kill();
        }

        public string Start(SpringPaths paths, TestCase test, Benchmark benchmark) {
            LogLines = new StringBuilder();

            paths.SetEnginePath(paths.GetEngineFolderByVersion(test.Engine));

            var optirun = Environment.GetEnvironmentVariable("OPTIRUN");
            
            process = new Process();
            process.StartInfo.CreateNoWindow = true;
            if (string.IsNullOrEmpty(optirun)) {
                process.StartInfo.FileName = test.UseMultithreaded ? paths.MtExecutable : paths.Executable;
            }
            else {
                Trace.TraceInformation("Using optirun {0} to start the game (OPTIRUN env var defined)", optirun);
                process.StartInfo.FileName = optirun;
                process.StartInfo.Arguments += test.UseMultithreaded ? paths.MtExecutable : paths.Executable;
            }



            process.StartInfo.WorkingDirectory = Path.GetDirectoryName(paths.Executable);

            process.StartInfo.Arguments += string.Format("--config \"{0}\"", Path.Combine(test.Config.ConfigPath, "springsettings.cfg"));
            if (test.BenchmarkArg > 0) process.StartInfo.Arguments += " --benchmark " + test.BenchmarkArg;


            var dataDirList = new List<string>()
            {
                test.Config.ConfigPath,
                Directory.GetParent(benchmark.BenchmarkPath).Parent.FullName,
                paths.WritableDirectory,
            };
            dataDirList.AddRange(paths.DataDirectories);
            dataDirList.Add(Path.GetDirectoryName(paths.Executable));
            var datadirs = string.Join(Environment.OSVersion.Platform == PlatformID.Unix ? ":" : ";", dataDirList.Distinct());

            process.StartInfo.EnvironmentVariables["SPRING_DATADIR"] = datadirs;
            process.StartInfo.EnvironmentVariables["SPRING_ISOLATED"] = test.Config.ConfigPath;
            process.StartInfo.EnvironmentVariables["SPRING_WRITEDIR"] = test.Config.ConfigPath;
            process.StartInfo.EnvironmentVariables["OMP_WAIT_POLICY"] = "ACTIVE";
            

            var scriptPath = Path.GetTempFileName();
            File.WriteAllText(scriptPath, test.StartScript.GetScriptForTestCase(test, benchmark));
            process.StartInfo.Arguments += string.Format(" \"{0}\"", scriptPath);

            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;

            process.ErrorDataReceived += (sender, args) =>
                {
                    LineAdded(args.Data);
                    LogLines.AppendLine(args.Data);
                };
            process.OutputDataReceived += (sender, args) =>
                {
                    LineAdded(args.Data);
                    LogLines.AppendLine(args.Data);
                };
            process.EnableRaisingEvents = true;

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();
            var lines = LogLines.ToString();

            File.Delete(scriptPath);

            return lines;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace CoreTechs.Common
{
    public static class ProcessExtensions
    {
        /// <summary>
        /// Starts a process with settings for performing a headless, automated task.
        /// </summary>
        /// <param name="process">The process object that will perform the task.</param>
        /// <param name="onOutput">An action that will handle standard output and errors returned by the process.</param>
        /// <param name="synchronizeOutputEvents">
        /// If true, the invocations of the onOutput action will be serialized by wrapping the action in a lock.
        /// The output and error events are handled by different threads.
        /// To handle synchronization yourself, set this to false.</param>
        /// <returns></returns>
        public static bool StartAutomated(this Process process, Action<ProcessOutput> onOutput = null, bool synchronizeOutputEvents = true)
        {
            var info = process.StartInfo;
            info.UseShellExecute = false;
            info.CreateNoWindow = true;
            info.ErrorDialog = false;

            if (onOutput == null)
                return process.Start();

            if (synchronizeOutputEvents)
            {
                var mutex = new Object();
                var wrapped = onOutput;
                onOutput = o => { lock (mutex) wrapped(o); };
            }

            info.RedirectStandardOutput = true;
            info.RedirectStandardError = true;
            process.OutputDataReceived += (s, e) => { if (e.Data != null) onOutput(new ProcessOutput(e.Data, false)); };
            process.ErrorDataReceived += (s, e) => { if (e.Data != null) onOutput(new ProcessOutput(e.Data, true)); };

            var started = process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            return started;
        }

        /// <summary>
        /// Conveniently set the FileName and Arguments properties.
        /// </summary>
        public static void SetCommand(this ProcessStartInfo startInfo, string fileName, params string[] arguments)
        {
            startInfo.FileName = fileName;
            startInfo.Arguments = string.Join(" ", arguments);
        }

        /// <summary>
        /// Conveniently set the FileName and Arguments properties.
        /// </summary>
        public static void SetCommand(this ProcessStartInfo startInfo, string fileName, IEnumerable<string> arguments)
        {
            startInfo.FileName = fileName;
            startInfo.Arguments = string.Join(" ", arguments);
        }
    }

    public class ProcessOutput
    {
        /// <summary>
        /// The instant that the output was received.
        /// </summary>
        public DateTimeOffset Time { get; private set; }

        /// <summary>
        /// The data sent by the process.
        /// </summary>
        public string Data { get; private set; }

        /// <summary>
        /// True if the output was received on the StandardError stream.
        /// False if the output was received on the StandardOutput stream.
        /// </summary>
        public bool IsError { get; private set; }

        public ProcessOutput(string data, bool isError)
        {
            Time = DateTimeOffset.Now;
            Data = data;
            IsError = isError;
        }
    }
}
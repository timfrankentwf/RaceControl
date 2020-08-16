﻿using NLog;
using RaceControl.Common;
using RaceControl.Common.Utils;
using System.Diagnostics;

namespace RaceControl.Streamlink
{
    public class StreamlinkLauncher : IStreamlinkLauncher
    {
        private readonly ILogger _logger;
        private readonly IChildProcessTracker _childProcessTracker;

        public StreamlinkLauncher(ILogger logger, IChildProcessTracker childProcessTracker)
        {
            _logger = logger;
            _childProcessTracker = childProcessTracker;
        }

        public Process StartStreamlinkExternal(string streamUrl, out string streamlinkUrl, bool lowQualityMode, bool useAlternativeStream)
        {
            var port = SocketUtils.GetFreePort();
            var stream = GetStreamIdentifier(lowQualityMode, useAlternativeStream);
            _logger.Info($"Starting external Streamlink-instance for stream-URL '{streamUrl}' on port '{port}' with identifier '{stream}'.");

            var process = ProcessUtils.StartProcess(
                @".\streamlink\streamlink.bat",
                $"--player-external-http --player-external-http-port {port} --hls-audio-select * \"{streamUrl}\" {stream}",
                false,
                true);

            _childProcessTracker.AddProcess(process);
            streamlinkUrl = $"http://127.0.0.1:{port}";

            return process;
        }

        public Process StartStreamlinkVLC(string vlcExeLocation, string streamUrl, bool lowQualityMode, bool useAlternativeStream)
        {
            var stream = GetStreamIdentifier(lowQualityMode, useAlternativeStream);
            _logger.Info($"Starting VLC Streamlink-instance for stream-URL '{streamUrl}' with identifier '{stream}'.");

            return ProcessUtils.StartProcess(
                @".\streamlink\streamlink.bat",
                $"--player \"{vlcExeLocation} --file-caching=2000 --network-caching=4000\" --hls-audio-select * \"{streamUrl}\" {stream}",
                false,
                true);
        }

        private string GetStreamIdentifier(bool lowQualityMode, bool useAlternativeStream)
        {
            if (!lowQualityMode)
            {
                if (!useAlternativeStream)
                {
                    return "best";
                }
                else
                {
                    return "1080p_alt";
                }
            }
            else
            {
                if (!useAlternativeStream)
                {
                    return "576p";
                }
                else
                {
                    return "576p_alt";
                }
            }
        }
    }
}
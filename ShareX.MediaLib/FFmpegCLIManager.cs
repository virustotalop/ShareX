﻿#region License Information (GPL v3)

/*
    ShareX - A program that allows you to take screenshots and share any file type
    Copyright © 2007-2015 ShareX Developers

    This program is free software; you can redistribute it and/or
    modify it under the terms of the GNU General Public License
    as published by the Free Software Foundation; either version 2
    of the License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program; if not, write to the Free Software
    Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

    Optionally you can also view the license at <http://www.gnu.org/licenses/>.
*/

#endregion License Information (GPL v3)

using ShareX.HelpersLib;
using System;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace ShareX.MediaLib
{
    public class FFmpegCLIManager : ExternalCLIManager
    {
        public string FFmpegPath { get; private set; }
        public StringBuilder Output { get; private set; }

        public FFmpegCLIManager(string ffmpegPath)
        {
            FFmpegPath = ffmpegPath;
            Output = new StringBuilder();
            OutputDataReceived += FFmpeg_DataReceived;
            ErrorDataReceived += FFmpeg_DataReceived;
            Helpers.CreateDirectoryIfNotExist(FFmpegPath);
        }

        private void FFmpeg_DataReceived(object sender, DataReceivedEventArgs e)
        {
            lock (this)
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    Output.AppendLine(e.Data);
                }
            }
        }

        public VideoInfo GetVideoInfo(string videoPath)
        {
            Open(FFmpegPath, string.Format("-i \"{0}\"", videoPath));
            string output = Output.ToString();
            Match match = Regex.Match(output, @"Duration: (?<Duration>\d{2}:\d{2}:\d{2}\.\d{2}),.+?start: (?<Start>\d+\.\d+),.+?bitrate: (?<Bitrate>\d+) kb/s", RegexOptions.CultureInvariant);

            if (match.Success)
            {
                VideoInfo videoInfo = new VideoInfo();
                videoInfo.FilePath = videoPath;
                videoInfo.Duration = TimeSpan.Parse(match.Groups["Duration"].Value);
                //videoInfo.Start = TimeSpan.Parse(match.Groups["Start"].Value);
                videoInfo.Bitrate = int.Parse(match.Groups["Bitrate"].Value);

                return videoInfo;
            }

            return null;
        }
    }
}
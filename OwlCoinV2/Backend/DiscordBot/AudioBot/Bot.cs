using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using Discord.Audio;
using Discord;
using System.Collections.Concurrent;

namespace OwlCoinV2.Backend.DiscordBot.AudioBot
{
    public static class Bot
    {
        public static ConcurrentDictionary<ulong, IAudioClient> AudioInstances = new ConcurrentDictionary<ulong, IAudioClient>();
        public static async Task JoinVoice(SocketMessage Message)
        {
            IVoiceChannel Channel = (Message.Author as IVoiceState).VoiceChannel;
            IAudioClient Client = await Channel.ConnectAsync();
            if (AudioInstances.TryAdd(Channel.Id, Client)) { Console.WriteLine("yeet"); }
        }

        public static async Task PlaySong(SocketMessage Message)
        {
            await JoinVoice(Message);
            IAudioClient Client = AudioInstances[(Message.Author as IVoiceState).VoiceChannel.Id];
            using (var ffmpeg = CreateProcess("https://www.youtube.com/watch?v=1jjcxFGEysE"))
            using (var stream = Client.CreatePCMStream(AudioApplication.Music))
            {
                try { await ffmpeg.StandardOutput.BaseStream.CopyToAsync(stream); }
                finally { await stream.FlushAsync(); }
            }
        }

        private static Process CreateProcess(string path)
        {
            return Process.Start(new ProcessStartInfo
            {
                FileName = "ffmpeg.exe",
                Arguments = $"-hide_banner -loglevel panic -i \"{path}\" -ac 2 -f s16le -ar 48000 pipe:1",
                UseShellExecute = false,
                RedirectStandardOutput = true
            });
        }
    }
}

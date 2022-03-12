using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using Discord;
using Discord.WebSocket;
using System.Threading;

namespace DuelBot
{
    public class TimeOutLock : IDisposable
    {
        private Object _lockObject = null;
        private int _timeout = 20000;

        public TimeOutLock(Object obj)
        {
            _lockObject = obj;
        }

        public TimeOutLock Lock()
        {
            if (System.Threading.Monitor.TryEnter(_lockObject, _timeout))
                return new TimeOutLock(_lockObject);
            else
                throw new System.TimeoutException("failed to acquire the lock");
        }

        public void Dispose()
        {
            _lockObject = null;
        }
    }

    public class DiscordClient
    {
        private DiscordSocketClient _socketClient = null;
        private RequestOptions _requestOption = null;

        private TimeOutLock _lock = new TimeOutLock(new object());

        private string _token = null;
        private List<SocketGuild> _guilds = null;
        private ConcurrentBag<IDiscordMessageListener> _listeners = null;
        
        public DiscordClient()
        {
            XmlNodeList xmlNodes = null;
            using (var text = File.OpenText(Define.c_config_path))
            {
                if(text == null)
                {
                    return;
                }

                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(text.ReadToEnd());
                xmlNodes = xmlDoc.GetElementsByTagName("config");
            }

            var e = xmlNodes[0].SelectSingleNode("app_info");
            _token = e.Attributes["token"].Value;
            if (string.IsNullOrEmpty(_token))
            {
                return;
            }

            _socketClient = new DiscordSocketClient();
            _guilds = new List<SocketGuild>();
            _listeners = new ConcurrentBag<IDiscordMessageListener>();

            _socketClient.Connected += connected;
            _socketClient.JoinedGuild += joinedGuild;  // 채널 입장
            _socketClient.LeftGuild += leftGuild;    // 채널 퇴장
            _socketClient.MessageReceived += messageReceived;    // 메시지 받음
            _socketClient.Log += onLog;  // 시스템 로그

            _requestOption = RequestOptions.Default.Clone();
            _requestOption.Timeout = 20 * 1000;    // 20초
            _requestOption.UseSystemClock = true;
        }

        public async Task Start()
        {
            await _socketClient.LoginAsync(TokenType.Bot, _token);
            await _socketClient.StartAsync();
        }
        
        private Task connected()
        {
            _guilds.Clear();

            if (_socketClient.Guilds != null)
            {
                var itor = _socketClient.Guilds.GetEnumerator();
                while (itor.MoveNext())
                {
                    var guild = itor.Current;
                    _guilds.Add(guild);
                }
            }

            return Task.CompletedTask;
        }

        // 채널 입장
        private Task joinedGuild(SocketGuild socketChannel)
        {
            //using (_lock)
            {
                int idx = _guilds.FindIndex(item => item.Id == socketChannel.Id);
                if (idx >= 0)
                {
                    _guilds.RemoveAt(idx);
                }
                _guilds.Add(socketChannel); 
            }

            return Task.CompletedTask;
        }

        private Task leftGuild(SocketGuild socketChannel)
        {
            //using (_lock)
            {
                int idx = _guilds.FindIndex(item => item.Id == socketChannel.Id);
                if (idx >= 0)
                {
                    _guilds.RemoveAt(idx);
                }
            }
            return Task.CompletedTask;
        }
        
        // 메시지 받음
        private Task messageReceived(SocketMessage socketMessage)
        {
            onMessage(socketMessage);
            return Task.CompletedTask;
        }
        
        private Task onLog(LogMessage logMessage)
        {
            Console.WriteLine(logMessage);
            return Task.CompletedTask;
        }

        public void SendMessage(string msg)
        {
            using (_lock)
            {
                foreach (var guild in _guilds)
                {
                    var task = guild.DefaultChannel.SendMessageAsync($"```{msg}```", false, null, _requestOption);
                    task.Start();
                }
            }
        }

        public void SendMessage(IMessageChannel channel, string msg)
        {
            var task = channel.SendMessageAsync($"```{msg}```", false, null, _requestOption);
            task.Start();
        }

        public void AddListenr(IDiscordMessageListener listener)
        {
            _listeners.Add(listener);
        }

        private void onMessage(SocketMessage socketMessage)
        {
            foreach(var entitiy in _listeners)
            {
                entitiy.RecvMessage(socketMessage);
            }
        }

        public async Task Tick()
        {
            while (true)
            {
                double now = (TimeZoneInfo.ConvertTimeToUtc(DateTime.Now) - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;

                foreach (var entitiy in _listeners)
                {
                    entitiy.Tick(now);
                }

                await Task.Delay(10);
            }
        }
    }
}

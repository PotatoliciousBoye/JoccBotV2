using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Victoria;

namespace JoccBotV2
{
    /// <summary>
    /// Defines the <see cref="Program" />.
    /// </summary>
    internal class Program
    {
        #region Fields

        /// <summary>
        /// Defines the _configuration.
        /// </summary>
        public static IConfigurationRoot _configuration;

        /// <summary>
        /// Defines the _client.
        /// </summary>
        private DiscordSocketClient _client;

        /// <summary>
        /// Defines the _commands.
        /// </summary>
        private CommandService _commands;

        /// <summary>
        /// Defines the _services.
        /// </summary>
        private IServiceProvider _services;

        private LavaNode _lavaNode;

        private LavaConfig _lavaConfig;

        #endregion

        #region Methods

        /// <summary>
        /// The _client_Log.
        /// </summary>
        /// <param name="arg">The arg<see cref="LogMessage"/>.</param>
        /// <returns>The <see cref="Task"/>.</returns>
        private Task ClientLog(LogMessage arg)
        {
            Console.WriteLine(arg);
            return Task.CompletedTask;
        }

        /// <summary>
        /// The HandleCommandAsync.
        /// </summary>
        /// <param name="arg">The arg<see cref="SocketMessage"/>.</param>
        /// <returns>The <see cref="Task"/>.</returns>
        private async Task HandleCommandAsync(SocketMessage arg)
        {
            var message = arg as SocketUserMessage;
            var context = new SocketCommandContext(_client, message);
            if (message.Author.IsBot) return;

            int argPos = 0;
            if (message.HasStringPrefix("!", ref argPos))
            {
                var result = await _commands.ExecuteAsync(context, argPos, _services);
                if (!result.IsSuccess) Console.WriteLine(result.ErrorReason);
            }
        }

        /// <summary>
        /// The Main.
        /// </summary>
        /// <param name="args">The args<see cref="string[]"/>.</param>
        internal static void Main(string[] args) => new Program().RunBotAsync().GetAwaiter().GetResult();

        /// <summary>
        /// The RegisterCommandsAsync.
        /// </summary>
        /// <returns>The <see cref="Task"/>.</returns>
        public async Task RegisterCommandsAsync()
        {
            _client.MessageReceived += HandleCommandAsync;
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }

        /// <summary>
        /// The RunBotAsync.
        /// </summary>
        /// <returns>The <see cref="Task"/>.</returns>
        public async Task RunBotAsync()
        {
            _client = new DiscordSocketClient();
            _commands = new CommandService();
            _lavaConfig = new LavaConfig(){ SelfDeaf = true };
            _lavaNode = new LavaNode(_client, _lavaConfig);

            _services = new ServiceCollection()
                .AddSingleton(_client)
                .AddSingleton(_commands)
                .AddSingleton(_lavaConfig)
                .AddSingleton(_lavaNode)
                .BuildServiceProvider();

            _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetParent(AppContext.BaseDirectory).FullName)
                .AddJsonFile("appsettings.json", false)
                .Build();

            string token = _configuration.GetSection("BotToken").Value;

            _client.Log += ClientLog;

            _client.Ready += OnReadyAsync;

            await RegisterCommandsAsync();

            await _client.LoginAsync(TokenType.Bot, token);

            await _client.StartAsync();

            await Task.Delay(-1);
        }

        private async Task OnReadyAsync()
        {
            if (!_lavaNode.IsConnected)
            {
                await _lavaNode.ConnectAsync();
            }
        }

        #endregion
    }
}

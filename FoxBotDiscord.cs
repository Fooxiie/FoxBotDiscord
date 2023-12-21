using System;
using System.IO;
using System.Net.Http;
using System.Net.WebSockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Life;
using Life.DB;
using Life.Network;
using Mirror;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace FoxBotDiscord
{
    public class FoxBotDiscord : Plugin
    {
        private Config _config;
        private DiscordSocketClient _client;

        public FoxBotDiscord(IGameAPI api) : base(api)
        {
        }

        public override async void OnPluginInit()
        {
            base.OnPluginInit();

            var configFilePath = Path.Combine(pluginsPath, "FoxBotDiscord/config.json");

            if (!Directory.Exists(Path.GetDirectoryName(configFilePath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(configFilePath) ??
                                          Path.Combine(pluginsPath, "FoxBotDiscord"));
            }

            if (!File.Exists(configFilePath))
            {
                var defaultConfig = new Config
                {
                    DiscordBotToken = ""
                };
                File.WriteAllText(configFilePath, JsonConvert.SerializeObject(defaultConfig));
                Debug.Log("Le token discord n'est pas configuré !");
                Debug.Log("Le token discord n'est pas configuré !");
                Debug.Log("Le token discord n'est pas configuré !");
            }
            else
            {
                var jsonString = File.ReadAllText(configFilePath);

                _config = JsonConvert.DeserializeObject<Config>(jsonString);
            }

            _client = new DiscordSocketClient();

            _client.Ready += ReadyAsync;

            await _client.LoginAsync(TokenType.Bot, _config.DiscordBotToken);
            await _client.StartAsync();
            
            await Task.Delay(-1);
        }

        private async Task ReadyAsync()
        {
            Debug.Log("Discord bot is connected !");
            await _client.SetCustomStatusAsync($"Démarrage du serveur, connectez-vous !");
            
            CreateSlashCommand();
        }

        private async void CreateSlashCommand()
        {
            var slashCommandBuilder = new SlashCommandBuilder();
            slashCommandBuilder.WithName("nombre-joueur");
            slashCommandBuilder.WithDescription("Récupérer le nombre de joueur sur le serveur");

            _client.SlashCommandExecuted += _client_SlashCommandExecuted;

            try
            {
                await _client.CreateGlobalApplicationCommandAsync(slashCommandBuilder.Build());
            }
            catch (Exception exception)
            {
                Console.WriteLine("DiscordBot " + exception.Message);
            }
        }

        private static async Task _client_SlashCommandExecuted(SocketSlashCommand arg)
        {
            if (arg.CommandName == "nombre-joueur")
            {
                await arg.RespondAsync(
                    $"Le nombre de joueur actuellement sur le serveur est de {Nova.server.GetAllInGamePlayers().Count}/{Nova.serverInfo.serverSlot}");
            }
        }

        private async Task UpdatePlayers()
        {
            await _client.SetCustomStatusAsync($"{Nova.serverInfo.serverName} : {Nova.server.GetAllInGamePlayers().Count}/{Nova.serverInfo.serverSlot}");
        }

        public override void OnPlayerDisconnect(NetworkConnection conn)
        {
            base.OnPlayerDisconnect(conn);
            UpdatePlayers();
        }

        public override void OnPlayerSpawnCharacter(Player player, NetworkConnection conn, Characters character)
        {
            base.OnPlayerSpawnCharacter(player, conn, character);
            UpdatePlayers();
        }
    }
}
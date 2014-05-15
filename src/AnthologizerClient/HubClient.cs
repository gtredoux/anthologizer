using Microsoft.AspNet.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnthologizerClient
{
    public class HubClient
    {
        private IDisposable hubReg = null;
        private IHubProxy anthHubProxy = null;
        private string myPlayerName = "Windows .NET Client";
        private Task connectToHubTask;

        public void Start()
        {
            connectToHubTask = connectToHubAsync();
        }

        private async Task connectToHubAsync()
        {
            var hubConnection = new HubConnection("http://localhost:34644");
            anthHubProxy = hubConnection.CreateHubProxy("AnthologizerHub");

            string playMessage = "play";
            hubReg = anthHubProxy.On<String, String, String>(playMessage, (player, name, path) => playItem(player, name, path));

            await hubConnection.Start();

            registerPlayer(myPlayerName);
        }

        private void playItem(string player, string host, string path)
        {

        }

        private void sendPlayItemMessage(string player, string host, string path)
        {
            anthHubProxy.Invoke("Play", player, host, path);
        }

        private void registerPlayer(string player)
        {
            anthHubProxy.Invoke("Register", player);
        }

        private List<string> players = new List<string>();

        private async Task getAllPlayers()
        {
            players = await anthHubProxy.Invoke<List<String>>("getPlayers");
        } 
    }
}

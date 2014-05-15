using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using System.Threading.Tasks;

namespace com.renoster.Anthologizer
{
    public class AnthologizerHub : Hub
    {
        private List<String> players = new List<string>();

        public override Task OnConnected()
        {
            var name = Context.QueryString["playerName"];
            if (!players.Contains(name))
                players.Add(name);
            return base.OnConnected();
        }

        public void Register(string playerName)
        {
            if (!players.Contains(playerName))
                players.Add(playerName); 
        }

        public void Play(string player, string host, string path)
        {
            Clients.All.Play(player, host, path);
        }

        public List<String> getPlayers()
        {
            return players;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnthologizerClient
{
    public abstract class MediaPlayerTask
    {
        public MediaPlayerTask(MediaPlayer player)
        {
            this.player = player;
        }

        private MediaPlayer player;

        public MediaPlayer Player
        {
            get { return player; }
            set { player = value; }
        }
    }

    public class StopMediaTask : MediaPlayerTask
    {
        public StopMediaTask(MediaPlayer player): base(player)
        {
        }
    }

    public class PlayMediaTask : MediaPlayerTask
    {
        private string mimetype;
        private string path;
        private string host;

        public PlayMediaTask(MediaPlayer player, string pmimetype, string phost, string ppath): base(player)
        {
            this.mimetype = pmimetype;
            this.host = phost;
            this.path = ppath;
        }

        public string Mimetype
        {
            get { return mimetype; }
            set { mimetype = value; }
        }

        public string Path
        {
            get { return path; }
            set { path = value; }
        }

        public string Host
        {
            get { return host; }
            set { host = value; }
        }
    }
}

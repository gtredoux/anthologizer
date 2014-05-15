using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;
using zPlayerLib;
using System.IO;
using System.Collections.Concurrent;

namespace AnthologizerClient
{
    public class MediaPlayer
    {
        private BlockingCollection<MediaPlayerTask> myTasks = new BlockingCollection<MediaPlayerTask>();

        private enum StateEnum {Stopping, Stopped, Started};

        private StateEnum state = StateEnum.Stopped;

        public delegate void EndOfMedia(string uri);

        public event EndOfMedia OnEndOfMedia;

        private WMPLib.WindowsMediaPlayer wmplayer;
        private zPlayerController zPlayerController;
        private string current=null;

        public MediaPlayer()
        {
        }

        public void Close()
        {
            myTasks.CompleteAdding();
        }

        public void Start()
        {
            // service the task queue
            Task.Run(() =>
            {
                // apparently these must be on the same thread
                wmplayer = new WMPLib.WindowsMediaPlayer();
                zPlayerController = new zPlayerController();
                wmplayer.PlayStateChange += new WMPLib._WMPOCXEvents_PlayStateChangeEventHandler(wmplayerStateChange);
                zPlayerController.OnEndOfMedia += zPlayerController_OnEndOfMedia;

                while (!myTasks.IsCompleted)
                {
                    MediaPlayerTask task = null;

                    // Blocks if number.Count == 0 
     
                    try
                    {
                        task = myTasks.Take();
                    }
                    catch (InvalidOperationException)
                    {
                        if (myTasks.IsCompleted)
                            return;
                        throw; // some other problem, raise it
                    }

                    PlayMediaTask playTask = task as PlayMediaTask;

                    if (playTask != null)
                    {
                        doPlay(playTask.Mimetype, playTask.Host, playTask.Path);
                    }

                    StopMediaTask stopTask = task as StopMediaTask;

                    if (stopTask != null)
                        doStop();

                    // otherwise ignore it
                }
            });
        }

        void zPlayerController_OnEndOfMedia(string uri)
        {
            if (this.OnEndOfMedia != null)
                OnEndOfMedia(current);
        }

        private void wmplayerStateChange(int newState)
        {
            switch (newState)
            {
                case 0: // Undefined
                    //currentStateLabel.Text = "Undefined";
                    break;

                case 1: // Stopped
                    //currentStateLabel.Text = "Stopped";
                    if (state == StateEnum.Started)
                    {
                        if (OnEndOfMedia != null)
                            OnEndOfMedia(current);
                    }
                    break;

                case 2: // Paused
                    //currentStateLabel.Text = "Paused";
                    break;

                case 3: // Playing
                    //currentStateLabel.Text = "Playing";
                    break;

                case 4: // ScanForward
                    //currentStateLabel.Text = "ScanForward";
                    break;

                case 5: // ScanReverse
                    //currentStateLabel.Text = "ScanReverse";
                    break;

                case 6: // Buffering
                    //currentStateLabel.Text = "Buffering";
                    break;

                case 7: // Waiting
                    //currentStateLabel.Text = "Waiting";
                    break;

                case 8: // MediaEnded
                    //currentStateLabel.Text = "MediaEnded";
                    break;

                case 9: // Transitioning
                    //currentStateLabel.Text = "Transitioning";
                    break;

                case 10: // Ready
                    //currentStateLabel.Text = "Ready";
                    break;

                case 11: // Reconnecting
                    //currentStateLabel.Text = "Reconnecting";
                    break;

                case 12: // Last
                    //currentStateLabel.Text = "Last";
                    break;

                default:
                    //currentStateLabel.Text = ("Unknown State: " + e.newState.ToString());
                    break;
            }
        }

        private string PlayWMP(string mimetype, string uri)
        {
            wmplayer.controls.stop();
            wmplayer.URL = uri;
            wmplayer.controls.play();
            return uri;
        }

        public void Play(string mimetype, string url, string path)
        {
            myTasks.Add(new PlayMediaTask(this,mimetype, url, path));
        }

         private void doPlay(string mimetype, string host, string path)
        {
            if (mimetype == "audio/flac")
            {
                current = PlayFLAC(host, path);
            }
            else
            {
                string absuri = MediaClientREST.GetAtomicURI(host, path);
                current = PlayWMP(mimetype, absuri);
            }

            state = StateEnum.Started;
        }

        private string PlayFLAC(string host, string path)
        {
            zPlayerController.Close();

            long contentLength;
            MediaClientREST restClient = new MediaClientREST(host);
            Stream stream = restClient.GetAtomic(path, out contentLength);
            PlayFLAC(contentLength, stream);
            return MediaClientREST.GetAtomicURI(host,path);
        }

        private void PlayFLAC(long contentLength, Stream stream)
        {
            zPlayerController.Open(contentLength, TStreamFormat.sfFLAC, stream);
        }

        private void doStop()
        {
            // not sure which is playing so stop them both
            state = StateEnum.Stopping;
            StopPlayers();
            state = StateEnum.Stopped;
        }

        public void Stop()
        {
            myTasks.Add(new StopMediaTask(this));
        }

        private void StopPlayers()
        {
            wmplayer.controls.stop();
            zPlayerController.Close();
        }

        internal void Skip()
        {
            // keep playing if possible
            StopPlayers();   
        }
    }

  
}

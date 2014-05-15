using System.Collections.ObjectModel;
using com.renoster.Anthologizer.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client;

namespace AnthologizerClient
{
    public class Anthologizer
    {
        private string url;
        private Anthology anthology;
        private MediaMgr mediaMgr;
        private MediaPlayer mediaPlayer;
        private Stack<Record> stack = new Stack<Record>();
        private List<ItemSelector> playList = null;

        private Record current = null;

        private Item currentlyPlayingItem;
        private int currentlyPlaying = -1;

        public delegate void NewItemsEvent(Anthologizer a, Record newItems);
        public delegate void ConnectedEvent(Anthologizer a, string url);
        public delegate void ErrorEvent(Anthologizer a, string error, Exception ex);
        public delegate void MediaStoppedEvent(Anthologizer a, int index, Item item);
        public delegate void MediaStartedEvent(Anthologizer a, int index, Item item);

        public event NewItemsEvent EventNewItems;
        public event ConnectedEvent EventConnected;
        public event ErrorEvent EventError;
        public event MediaStartedEvent EventMediaStarted;
        public event MediaStoppedEvent EventMediaStopped;

        HubClient hubClient = new HubClient();

        public Anthologizer()
        {
            mediaPlayer = new MediaPlayer();
            mediaPlayer.OnEndOfMedia += mediaPlayer_OnEndOfMedia;
        }

        public Anthology Anthology
        {
            get { return anthology; }
            set
            {
                anthology = value;
                anthology.OnError += anthology_OnError;
            }
        }

        void anthology_OnError(Anthology a, string error, Exception ex)
        {
            if (this.EventError != null)
                EventError(this, error, ex);
        }

        private void SetCurrentlyPlaying(int index)
        {
            currentlyPlaying = index;
            currentlyPlayingItem =  (currentlyPlaying == -1) ? null : current.Contents[currentlyPlaying].GetItem();
        }

        void mediaPlayer_OnEndOfMedia(string uri)
        {
            if (PlayNext())
                return;

            NotifyMediaStopped();
        }

        private void NotifyMediaStopped()
        {
            if (EventMediaStopped != null)
            {
                Item last = currentlyPlayingItem;
                int lastindex = currentlyPlaying;
                SetCurrentlyPlaying(-1);

                if (last != null)
                    EventMediaStopped(this, lastindex, last);
            }
        }

        public bool PlayNext()
        {
            int next = currentlyPlaying + 1;
            while (next < current.Contents.Count)
            {
                Item item = playList[next].GetItem();
                if (playList[next].ItemType == ItemTypeEnum.atomic && IsPlayable(item))
                {
                    PlayMedia(next, item.Mimetype, item.Id);
                    return true;
                }
                next++;
            }
            return false;
        }

        public void Close()
        {
            if (mediaPlayer != null)
            {
                StopPlaying();
                mediaPlayer.Close();
                mediaPlayer = null;
            }

            if (mediaMgr != null)
            {
                mediaMgr.Stop();
                mediaMgr = null;
            }
        }

        public void Start()
        {
            Anthology = new Anthology(null);
            mediaPlayer.Start();

            hubClient.Start();
        }

        public void Connect(string url)
        {
            this.url = url;

            stack.Clear();
                
            mediaMgr = new MediaMgr(MediaClientFactory.Create(url));
            Anthology.MediaMgr = mediaMgr;
            mediaMgr.Start();

            Anthology.Refresh();
 
            GetCollection("/");

            if (EventConnected != null)
                EventConnected(this, url);
        }

        public void Back()
        {
            if (stack.Count == 0)
                return;

            Record up = stack.Pop();
            Update(up);
        }

        public void StopPlaying()
        {
            mediaPlayer.Stop();
            NotifyMediaStopped();
        }

        private void Update(Record r)
        {
            current = r;
            playList = current.Contents;
            if (EventNewItems != null)
                EventNewItems(this, current);
        }

        private object getLock = new object();
        private GetCompositeTask outstandingGetComposite = null;

        public GetCompositeTask GetCollection(string path)
        {
            lock (getLock)
            {
                if (outstandingGetComposite != null)
                    outstandingGetComposite.Cancel(); 
                outstandingGetComposite = mediaMgr.GetCompositeAsync(path, NotifyGetCollectionResult);
                return outstandingGetComposite;
            }
        }

        private void NotifyGetCollectionResult(MediaMgrTask task)
        {
            lock (getLock)
            {
                outstandingGetComposite = null;
            }

            if (task.ErrorException != null)
            {
                if (EventError != null)
                    EventError(this, task.ErrorException.Message, null);
                return;
            }

            GetCompositeTask getcTask = task as GetCompositeTask;
            if (getcTask == null || getcTask.IsCancelled)
                return;

            if (getcTask.Results == null)
            {
                if (EventError != null)
                    EventError(this, "Could not get " + getcTask.Path, null);
                return;
            }

            Update(getcTask.Path, getcTask.Results);
           
        }

        public void PlayMedia(int index, Item item)
        {
            if (item.ItemType == ItemTypeEnum.atomic)
            {
                if (IsPlayable(item))
                    PlayMedia(index, item.Mimetype, item.Location);
            }
            else
            {
                Record old = current;
                GetCollection(item.Id);
                stack.Push(old);
            }
        }

        public void PlayMedia(int index, string mimetype, string path)
        {
            mediaPlayer.Stop();

            mediaPlayer.Play(mimetype, url, path);
            SetCurrentlyPlaying(index);

            if (EventMediaStarted != null)
                EventMediaStarted(this, currentlyPlaying, currentlyPlayingItem);
        }

        private static bool IsPlayable(Item item)
        {
            return item.Mimetype.StartsWith(@"audio/");
        }

        private bool Filter(Item item)
        {
            return (item.ItemType == ItemTypeEnum.composite) || IsPlayable(item);
        }

        private List<ItemSelector> Filter(Item[] items)
        {
            List<ItemSelector> filtered = new List<ItemSelector>();
            foreach (Item item in items)
            {
                if (Filter(item))
                {
                    ItemSelector itemUI = new ItemSelector(item);
                    itemUI.Add = anthology.Contains(item);
                    itemUI.OnAdd += itemUI_OnAdd;
                    filtered.Add(itemUI);
                }
            }
            return filtered;
        }

        private void Update(string path, Item[] results)
        {
            var filtered = Filter(results);
            Update(new Record(path, filtered));
        }

        void itemUI_OnAdd(ItemSelector itemUI)
        {
            if (itemUI.ItemType == ItemTypeEnum.composite)
                return; // in the future add all 

            if (itemUI.Add == true)
            {
                Anthology.AddAsync(itemUI.GetItem());
            }
            else
            {
                Anthology.RemoveAsync(itemUI.GetItem());
            }
        }
    }

    public class Record
    {
        private string path;
        private List<ItemSelector> contents;

        public Record(string ppath, List<ItemSelector> pcontents)
        {
            path = ppath;
            contents = pcontents;
        }

        public string Path
        {
            get { return path; }
            set { path = value; }
        }

        public List<ItemSelector> Contents
        {
            get { return contents; }
            set { contents = value; }
        }
    }
}

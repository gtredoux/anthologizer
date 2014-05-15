using com.renoster.Anthologizer.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnthologizerClient
{
    public class Anthology
    {
        private Dictionary<string,Item> contents = new Dictionary<string, Item>();
        private Dictionary<string, Item> contentsByDigest = new Dictionary<string, Item>(); 

        private MediaMgr mediaMgr;
        private string name;

        public delegate void UpdatedEvent(Anthology thisAnthology);
        public delegate void ErrorEvent(Anthology a, string error, Exception ex);

        public event UpdatedEvent OnUpdate;
        public event ErrorEvent OnError;

        private string section;

        public Anthology(string pname)
        {
            this.name = pname;
        }

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public string Section
        {
            get { return section; }
            set { section = value; }
        }

        public MediaMgr MediaMgr
        {
            get { return mediaMgr; }
            set { mediaMgr = value; }
        }

        public int Count
        {
            get { return contents.Count; }
            //set { count = value; }
        }

        public int Add(Item item)
        {
            int newCount = mediaMgr.Anthologize(Section, Name, item.Id);

            if (!contents.ContainsKey(item.Id))
            {
                contents.Add(item.Id, item);
                if (!String.IsNullOrEmpty(item.Digest))
                    contentsByDigest.Add(item.Digest, item);
            }

            if (OnUpdate != null)
                OnUpdate(this);

            return Count;
        }

        public AnthologizeTask AddAsync(Item item)
        {
            return mediaMgr.AnthologizeAsync(item, Section, Name, item.Id, new MediaMgrTask.AsyncTaskCompletedEvent(this.AsyncTaskCompleted));
        }

        private void AsyncTaskCompleted(MediaMgrTask task)
        {
            AnthologizeTask aTask = task as AnthologizeTask;
            if (aTask == null || aTask.IsCancelled)
                return; // not interested in other tasks

            int newCount = aTask.Count;

            Item item = aTask.Item;
            if (aTask.ErrorException == null)
            {
                if (aTask.Action == AnthologizeTask.ActionEnum.Add)
                {
                    if (!contents.ContainsKey(item.Id))
                        contents.Add(item.Id, item);
                    if (!String.IsNullOrEmpty(item.Digest) && !contentsByDigest.ContainsKey(item.Digest))
                        contentsByDigest.Add(item.Digest, item);
                }
                else
                {
                    if (contents.ContainsKey(item.Id))
                        contents.Remove(item.Id);
                    if (!String.IsNullOrEmpty(item.Digest) && contentsByDigest.ContainsKey(item.Digest))
                        contentsByDigest.Remove(item.Digest);
                }
            }

            if (OnUpdate != null)
                OnUpdate(this);
        }

        public int Remove(Item item)
        {
            int newCount = mediaMgr.UnAnthologize(Section, Name, item.Id);

            if (contents.ContainsKey(item.Id))
                contents.Remove(item.Id);

            if (OnUpdate != null)
                OnUpdate(this);
            
            return Count;
        }

        public AnthologizeTask RemoveAsync(Item item)
        {
            return mediaMgr.UnAnthologizeAsync(item, Section, Name, item.Id, new MediaMgrTask.AsyncTaskCompletedEvent(this.AsyncTaskCompleted));
        }

        public void Refresh()
        {
            mediaMgr.ListAnthologyAsync(Section, Name, NotifyListAnthologyResult);
        }

        private void NotifyListAnthologyResult(MediaMgrTask task)
        {
            if (task.ErrorException != null)
            {
                if (OnError != null)
                    OnError(this, "Could not list anthology", task.ErrorException);
                return;
            }

            ListAnthologyTask lat = task as ListAnthologyTask;
            if (lat == null)
                return;
            if (lat.Results != null)
            {
                contents.Clear();
                foreach (Item item in lat.Results)
                {
                    contents.Add(item.Id, item);
                    if (!String.IsNullOrEmpty(item.Digest) && !contentsByDigest.ContainsKey(item.Digest))
                        contentsByDigest.Add(item.Digest,item);
                }
                
                if (OnUpdate != null)
                    OnUpdate(this);
            }
        }

        public bool Contains(Item item)
        {
            return (!String.IsNullOrEmpty(item.Digest) && contentsByDigest.ContainsKey(item.Digest)) ||
                   contents.ContainsKey(item.Id);
        }
    }
}

using com.renoster.Anthologizer.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AnthologizerClient
{
    public abstract class MediaMgrTask
    {
        public delegate void AsyncTaskCompletedEvent(MediaMgrTask task);
        public AsyncTaskCompletedEvent EventAsyncTaskCompleted;

        public delegate void AsyncTaskCancelledEvent(MediaMgrTask task);
        public AsyncTaskCancelledEvent EventAsyncTaskCancelled;

        private Exception errorException;
        private bool cancelled = false;
        private MediaMgr mediaMgr;

        public MediaMgrTask(MediaMgr mgr)
        {
            this.mediaMgr = mgr;
        }

        public MediaMgrTask(MediaMgr mgr, AsyncTaskCompletedEvent notify)
        {
            this.mediaMgr = mgr;
            if (notify != null)
                EventAsyncTaskCompleted += notify;
        }

        public virtual void Cancel()
        {
            cancelled = true;
            if (EventAsyncTaskCancelled != null)
                EventAsyncTaskCancelled(this);
        }

        public bool IsCancelled {
            get { return cancelled; }
        }

        public Exception ErrorException
        {
            get { return errorException; }
            set { errorException = value; }
        }
    }

    public class GetCompositeTask: MediaMgrTask
    {
        private CancellationTokenSource cts = new CancellationTokenSource();

        private string path;
        private Item[] results;

        public GetCompositeTask(MediaMgr mediaMgr, string path, AsyncTaskCompletedEvent notify)
            : base(mediaMgr,notify)
        {
            this.path = path;
        }

        public override void Cancel()
        {
            CancellationTokenSource.Cancel();
        }

        public string Path
        {
            get { return path; }
            set { path = value; }
        }

        public Item[] Results
        {
            get { return results; }
            set { results = value; }
        }

        public CancellationTokenSource CancellationTokenSource
        {
            get { return cts; }
            set { cts = value; }
        }
    }

    public class ListAnthologyTask : MediaMgrTask
    {
        private string section;
        private string name;
        private Item[] results;

        public ListAnthologyTask(MediaMgr mediaMgr, string section, string name, AsyncTaskCompletedEvent notify)
            : base(mediaMgr, notify)
        {
            this.Section = section;
            this.Name = name;
        }
  
        public Item[] Results
        {
            get { return results; }
            set { results = value; }
        }

        public string Section
        {
            get { return section; }
            set { section = value; }
        }

        public string Name
        {
            get { return name; }
            set { name = value; }
        }
    }
}

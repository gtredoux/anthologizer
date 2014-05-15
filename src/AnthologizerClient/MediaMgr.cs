using System.Runtime.Remoting.Services;
using com.renoster.Anthologizer.Media;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnthologizerClient
{
    public class MediaMgr
    {
        private BlockingCollection<MediaMgrTask> myTasks = new BlockingCollection<MediaMgrTask>();
        private IMediaClient client;

        public MediaMgr(IMediaClient pclient)
        {
            client = pclient;
        }

        public void Cancel()
        {
            Stop();
            return; 
        }

        public void Start()
        {
            // service the task queue
            Task.Run(() =>
            {
                while (!myTasks.IsCompleted)
                {
                    MediaMgrTask task = null;

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

                    task.EventAsyncTaskCancelled += (mgrTask) => client.Cancel(); 

                    AnthologizeTask aTask = task as AnthologizeTask;

                    try
                    {
                        if (aTask != null)
                        {
                            if (aTask.Action == AnthologizeTask.ActionEnum.Add)
                                aTask.Count = Anthologize(aTask.Section, aTask.Name, aTask.Path);
                            else
                                aTask.Count = UnAnthologize(aTask.Section, aTask.Name, aTask.Path);
                            continue;
                        }

                        GetCompositeTask getcTask = task as GetCompositeTask;

                        if (getcTask != null && !getcTask.IsCancelled)
                        {
                            getcTask.Results = GetComposite(getcTask.Path);
                            continue;
                        }

                        ListAnthologyTask laTask = task as ListAnthologyTask;

                        if (laTask != null && !laTask.IsCancelled)
                        {
                            laTask.Results = ListAnthology(laTask.Section, laTask.Name);
                            continue;
                        }
                        // otherwise ignore it
                    }
                    catch (Exception ex)
                    {
                        if (!task.IsCancelled) // ignore aborted exception
                            task.ErrorException = ex;
                    }
                    finally
                    {
                        if (task.EventAsyncTaskCompleted != null)
                            task.EventAsyncTaskCompleted(task);
                    }
                }
            });
        }

        public void Stop()
        {
            myTasks.CompleteAdding();
        }

        public string Download(string path)
        {
            string tmpfname = Path.GetTempFileName();
            using (FileStream tmpf = File.Open(tmpfname, FileMode.Create, FileAccess.Write))
            {
                using (Stream s = client.GetAtomic(path))
                {
                    s.CopyTo(tmpf);
                    s.Close();
                }
                tmpf.Close();
            }
            return tmpfname;
        }

        public System.IO.Stream GetAtomic(string path)
        {
            return client.GetAtomic(path);
        }

        public Item[] GetComposite(string path)
        {
            Item[] result = client.GetComposite(path);
            return result;
        }

        public GetCompositeTask GetCompositeAsync(string path, MediaMgrTask.AsyncTaskCompletedEvent notify)
        {
            GetCompositeTask task = new GetCompositeTask(this, path, notify);
            client.GetCompositeAsync(task, notify);
            return task;
        }

        public int Anthologize(string anthologySection, string anthologyName, string path)
        {
            return client.Anthologize(anthologySection, anthologyName, path);
        }

        public AnthologizeTask AnthologizeAsync(Item item, string section, string name, string path, MediaMgrTask.AsyncTaskCompletedEvent notify)
        {
            AnthologizeTask atask = new AnthologizeTask(item, AnthologizeTask.ActionEnum.Add, this, section, name, path, notify);
            myTasks.Add(atask);
            return atask;
        }

        public int UnAnthologize(string anthologySection, string anthologyName, string path)
        {
            return client.UnAnthologize(anthologySection, anthologyName, path);
        }

        public AnthologizeTask UnAnthologizeAsync(Item item, string section, string name, string path, MediaMgrTask.AsyncTaskCompletedEvent notify)
        {
            AnthologizeTask atask = new AnthologizeTask(item, AnthologizeTask.ActionEnum.Remove, this, section, name, path, notify);
            myTasks.Add(atask);
            return atask;
        }

        public Item[] ListAnthology(string section, string name)
        {
            Item[] result = client.ListAnthology(section,name);
            return result;
        }

        public ListAnthologyTask ListAnthologyAsync(string section, string name, MediaMgrTask.AsyncTaskCompletedEvent notify)
        {
            ListAnthologyTask atask = new ListAnthologyTask(this, section, name, notify);
            myTasks.Add(atask);
            return atask;
        }
    }
}

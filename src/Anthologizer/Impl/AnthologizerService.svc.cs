using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Contexts;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Web.UI.WebControls;
using com.renoster.Anthologizer.Contract;
using com.renoster.Anthologizer.Impl.ExtensionMethods;
using com.renoster.Anthologizer.Properties;
using com.renoster.Anthologizer.Media;
using System.ServiceModel.Web;
using com.renoster.AnthologizerIndexerLib;
using com.renoster.LRUCache;
using System.Diagnostics;
using com.renoster.FileCacheLib;

namespace com.renoster.Anthologizer.Impl
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "AnthologizerService" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select AnthologizerService.svc or AnthologizerService.svc.cs at the Solution Explorer and start debugging.
    
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class AnthologizerService : Contract.IAnthologizerService
    {
        //private string restURL = "http://localhost:34644/Impl/AnthologizerService.svc/REST/GetComposite?path=%5c";

        private Indexer indexer;
        FileCache<Item> cache = new FileCache<Item>( (item,id) => (item.Id == id), Root, 5000); 

        private FileSystemWatcher fw = new FileSystemWatcher();
        private EventLog eventLog;
        private object indexLock = new object();

        public AnthologizerService()
        {
            var anthinfo = new DirectoryInfo(AnthologiesDir);
            indexer = new Indexer(anthinfo.Parent.FullName, anthinfo.Name);
            fw.IncludeSubdirectories = true;
            fw.Path = Root;
            //fw.Changed += fw_Changed;
            fw.Created += FwOnCreated;
            fw.Deleted += FwOnDeleted;
            fw.EnableRaisingEvents = true;

            eventLog = Indexer.GetEventLog();
            LogInfo("Anthologizer Service Starting ...");
        }

        private void FwOnDeleted(object sender, FileSystemEventArgs fileSystemEventArgs)
        {
            lock (cache)
            {
                cache.DeleteItemFromParent(fileSystemEventArgs.FullPath);
            }
        }

        private void FwOnCreated(object sender, FileSystemEventArgs fileSystemEventArgs)
        {
            try
            {
                lock (indexLock)
                {
                    String path = fileSystemEventArgs.FullPath;
                    FileInfo f = new FileInfo(path);
                    Item item = GetFileInfo(cache.getRelativePath(path), f, false, false);
                    cache.AddItemToParent(path, item);
                }
            }
            catch (Exception ex)
            {
                eventLog.WriteEntry("Cannot process newly created file or folder " + ex.Message);
            }
        }

        private static string Root {
            get { return Properties.Settings.Default.root; }
        }

        private string AtomVdir
        {
            get { return Properties.Settings.Default.atomVdir; }
        }

        private string AnthologiesDir
        {
            get { return Properties.Settings.Default.anthologies; }
        }

        public void Normalize(NormalizeEnum action, string encpath)
        {
            string path = Uri.UnescapeDataString(encpath);
            string absPath = cache.Absolute(Root, path);
            if (Directory.Exists(path))
            {
                DirectoryInfo dinfo = new DirectoryInfo(path);
                NormalizeDir(action, dinfo);
            }
            if (File.Exists(path))
            {
                FileInfo finfo = new FileInfo(path);
                NormalizeFile(action, finfo);
            }
        }

        private void NormalizeFile(NormalizeEnum action, FileInfo finfo)
        {
            throw new NotImplementedException();
        }

        private void NormalizeDir(NormalizeEnum action, DirectoryInfo dinfo)
        {
            switch (action)
            {
                case NormalizeEnum.Deepen:
                    String[] parts = dinfo.Name.Split('-');
                    if (parts.Length < 2)
                        throw new Exception("Cannot deepen this path");

                    string newparent = dinfo.Parent.FullName + Path.DirectorySeparatorChar + parts[0].Trim();
                    if (!Directory.Exists(newparent))
                        Directory.CreateDirectory(newparent);

                    string newchild =
                         newparent + Path.DirectorySeparatorChar + parts[1].Trim();
                    
                    Directory.Move(dinfo.FullName, newchild);
                    break;
                case NormalizeEnum.Flatten:
                    throw new NotImplementedException();
            }
        }

        public List<Item> GetComposite(string path)
        {
            try
            {
                List<Item> result = null;
                if (Properties.Settings.Default.cacheResults)
                    result = cache.GetFromCache(path);

                if (result == null)
                {
                    result = tryGetComposite(path);
                    if (Properties.Settings.Default.cacheResults)
                        cache.PutInCache(path, result);
                }
                return result;
            }
            catch (Exception e)
            {
                LogError(Resources.AnthologizerService_GetComposite_Could_not_get_composite + " < " + path + " >", e);
                throw;
            }
        }

        private Dictionary<string,Dictionary<string,bool>> contexts = 
            new Dictionary<string, Dictionary<string, bool>>(); 
 

        public List<Item> GetRandom(string contextKey, int maxcount, string encrelpath)
        {
            return GetRandom(Root, contextKey, maxcount, encrelpath);
        }

        public List<Item> GetRandom(string root, string contextKey, int maxcount, string encrelpath)
        {
            if (!contexts.ContainsKey(contextKey))
                contexts.Add(contextKey, new Dictionary<string, bool>());
            Dictionary<string, bool> context = contexts[contextKey];

            string path = Uri.UnescapeDataString(encrelpath);
            string absPath = cache.Absolute(root, path);
            if (!Directory.Exists(absPath))
                throw new Exception(Resources.AnthologizerService_tryGetComposite_No_such_path + path);

            DirectoryInfo dinfo = new DirectoryInfo(absPath);
            List<Item> results = new List<Item>();
            int count = 0;

            while (count < maxcount)
            {
                FileInfo f = GetRandom(context, dinfo);
                if (f == null)
                    break;
                GetFileInfo(f.FullName.Replace(root, ""), f, results, false);
                ++count;
            }

            return results;
        }

        private static Random random = new Random();

        private FileInfo GetRandom(Dictionary<string, bool> context, DirectoryInfo dinfo)
        {
            if (context.ContainsKey(dinfo.FullName) || !dinfo.Exists)
                return null;

             // flip a coin whether to go deep or shallow
            int x = random.Next(2);

            FileInfo result = GetRandomFromX(context, dinfo, x);
                
            if (result != null)
                return result;

            return GetRandomFromX(context, dinfo, (x + 1) % 2);
        }

        private FileInfo GetRandomFromX(Dictionary<string, bool> context, DirectoryInfo dinfo, int x)
        {
            if (x == 0)
                return GetRandomFromFiles(context, dinfo);
            return GetRandomFromDirs(context, dinfo);
        }

        private FileInfo GetRandomFromDirs(Dictionary<string, bool> context, DirectoryInfo dinfo)
        {
            DirectoryInfo[] dirs = dinfo.GetDirectories();
            if (dirs == null || dirs.Length == 0)
                return null;

            FileInfo result = GetRandomFromDirs(context, dirs);

            // mark this one as barren
            if (result == null)
                context.Add(dinfo.FullName, true);

            return result;
        }

        private FileInfo GetRandomFromDirs(Dictionary<string, bool> context, DirectoryInfo[] dirs)
        {
            List<DirectoryInfo> dcandidates = new List<DirectoryInfo>();
            foreach (DirectoryInfo d in dirs)
            {
                if (!context.ContainsKey(d.FullName))
                    dcandidates.Add(d);
            }

            while (dcandidates.Count > 0)
            {
                int randomNumber = random.Next(dcandidates.Count);
                FileInfo f = GetRandom(context, dcandidates[randomNumber]);
                if (f != null)
                    return f;
                dcandidates.RemoveAt(randomNumber);
            }

            return null;
        }

        private static FileInfo GetRandomFromFiles(Dictionary<string, bool> context, DirectoryInfo dinfo)
        {
            FileInfo[] files = dinfo.GetFiles();

            if (files != null & files.Count() > 0)
            {
                List<FileInfo> fcandidates = new List<FileInfo>();
                foreach (FileInfo f in files)
                {
                    if (!context.ContainsKey(f.FullName))
                        fcandidates.Add(f);
                }

                while (fcandidates.Count > 0)
                {
                    int randomNumber = random.Next(fcandidates.Count);
                    if (isAudio(fcandidates[randomNumber].FullName) &&
                        !context.ContainsKey(fcandidates[randomNumber].FullName))
                    {
                        context.Add(fcandidates[randomNumber].FullName, true);
                        {
                            return fcandidates[randomNumber];
                        }
                    }
                    fcandidates.RemoveAt(randomNumber);
                }
            }
            return null;
        }

        private static bool isAudio(string name)
        {
            return Mime.GetMIMEType(name).StartsWith(("audio/"));
        }


        private void LogInfo(string msg)
        {
            eventLog.WriteEntry(msg, EventLogEntryType.Information);
        }

        private void LogError(string msg, Exception e)
        {
            string estr = (e == null) ? "" : " " + e.Message + " " + e.StackTrace;
            eventLog.WriteEntry(msg + estr, EventLogEntryType.Error);
        }

        private List<Item> tryGetComposite(string encrelpath)
        {
            //Thread.Sleep(3*1000); // for debugging

            string path = Uri.UnescapeDataString(encrelpath);

            string absPath = cache.Absolute(Root, path);
            if (!Directory.Exists(absPath))
                throw new Exception(Resources.AnthologizerService_tryGetComposite_No_such_path + path);

            DirectoryInfo dinfo = new DirectoryInfo(absPath);

            // double check for skull-duggery in the path
            if (dinfo.FullName != absPath)
                throw new Exception(Resources.AnthologizerService_tryGetComposite_Invalid_path + " " + path);

            List<Item> result = new List<Item>();
            foreach (FileInfo f in dinfo.EnumerateFiles())
            {
                string mimeType = Mime.GetMIMEType(f.Name); 
                if ((!mimeType.StartsWith("audio/")) || mimeType.StartsWith("audio/x-mpegurl"))
                    continue;
                GetFileInfo(Combine(path,f.Name), f, result, false);
            }

            foreach (DirectoryInfo d in dinfo.EnumerateDirectories())
            {
                GetDirInfo(path, d, result);
            }

            return result;
        }

        private static void GetDirInfo(string path, DirectoryInfo d, List<Item> result)
        {
            Item item = new Item();
            item.Id = path.TrimEnd(Path.DirectorySeparatorChar, '/') + Path.DirectorySeparatorChar + d.Name;
            item.ItemType = Media.ItemTypeEnum.composite;
            item.Name = d.Name;
            item.Mimetype = Media.Mime.FolderMimeType;
            item.Size = 0;
            item.LastModified = d.LastWriteTime;
            result.Add(item);
        }

        private Item GetFileInfo(string relpath, FileInfo f, List<Item> result, bool knownToBeAnthologized)
        {
            var item = GetFileInfo(relpath, f, isAnthologyTrack(relpath), knownToBeAnthologized);
            result.Add(item);
            return item;
        }

        private Item GetFileInfo(string relpath, bool knownToBeAnthologized)
        {
            FileInfo f = new FileInfo( cache.Absolute(Root,relpath) );
            var anAnthologyTrack = isAnthologyTrack(relpath);

            var item = GetFileInfo(relpath, f, anAnthologyTrack, knownToBeAnthologized);
            return item;
        }

        private bool isAnthologyTrack(string relpath)
        {
            string anthSection;
            string anthName;
            bool anAnthologyTrack = indexer.IsAnthology(relpath, out anthSection, out anthName);
            return anAnthologyTrack;
        }

        private Item GetFileInfo(string relpath, FileInfo f, bool anAnthologyTrack, bool knownToBeAnthologized)
        {
            Item item = new Item();
            item.ItemType = Media.ItemTypeEnum.atomic;
            item.Id = relpath;
 
            // try the database
            MediaData.Item dbitem = indexer.GetFile(item.Id, anAnthologyTrack, knownToBeAnthologized);
            if (dbitem != null)
            {
                item.Digest = dbitem.digest;
                item.LastModified = new DateTime(dbitem.lastmodified);
                item.Mimetype = dbitem.mimetype;
                item.Name = dbitem.name;
                item.Size = dbitem.size;
                item.Anthologized = (dbitem.anthologized == 1);
            }
            else
            {
                item.Name = f.Name;
                item.Mimetype = Mime.GetMIMEType(f.Name);
                item.Size = f.Length;
                item.LastModified = f.LastWriteTime;
            }

            if ((item.ItemType == ItemTypeEnum.atomic) && !String.IsNullOrWhiteSpace(AtomVdir))
            {
                if (WebOperationContext.Current != null)
                {
                    UriBuilder locationUri =
                        new UriBuilder(WebOperationContext.Current.IncomingRequest.UriTemplateMatch.BaseUri);
                    locationUri.Path = AtomVdir + relpath;
                    item.Location = locationUri.Uri.AbsoluteUri;
                }
            }
            else
                item.Location = relpath;

            return item;
        }

        private static string Combine(string path, string name)
        {
            return path.TrimEnd(Path.DirectorySeparatorChar, '/') + Path.DirectorySeparatorChar + name.TrimStart(Path.DirectorySeparatorChar, '/');
        }

        public System.IO.Stream GetAtomic(string encpath)
        {
            string path = Uri.UnescapeDataString(encpath);
            try
            {
                return tryGetAtomic(path);
            }
            catch (Exception e)
            {
                LogError(Resources.AnthologizerService_GetAtomic_Could_not_get_atom + " < " + path + " >", e);
                throw;
            }
        }

        private Stream tryGetAtomic(string relpath)
        {
            string absPath = cache.Absolute(Root,relpath);

            FileInfo finfo = new FileInfo(absPath);

            WebOperationContext ctx = WebOperationContext.Current;
            ctx.OutgoingResponse.StatusCode = System.Net.HttpStatusCode.OK;
            Item item = GetFileInfo(relpath, false);
            ctx.OutgoingResponse.ContentType = item.Mimetype;
            ctx.OutgoingResponse.ContentLength = finfo.Length;
            
            if (!finfo.Exists)
                throw new Exception(Resources.AnthologizerService_tryGetAtomic_No_such_atom + " < " + relpath + " >");

            if (finfo.FullName != absPath)
                throw new Exception(Resources.AnthologizerService_tryGetAtomic_Invalid_path + " < " + relpath + " >");

           
            return new FileStream(absPath, FileMode.Open, FileAccess.Read,FileShare.Read);
        }

        public int Anthologize(string anthologySection, string anthologyName, string path)
        {
            lock (indexLock)
            {
                try
                {
                    return tryAnthologize(anthologySection, anthologyName, path);
                }
                catch (Exception e)
                {
                    LogError(Resources.AnthologizerService_Anthologize_Could_not_anthologize + " < " + path + " >", e);
                    throw;
                }
            }
        }

        private object createDirLock = new object();
        private Dictionary<string,object> destFileLocks = new Dictionary<string, object>();

        private object GetLockObj(string destFile)
        {
            lock (destFileLocks)
            {
                if (!destFileLocks.ContainsKey(destFile))
                {
                    object lockObj = new object();
                    destFileLocks.Add(destFile, lockObj);
                    return lockObj;
                }
                return destFileLocks[destFile];
            }
        }

        public int tryAnthologize(string anthologySection, string anthologyName, string path)
        {
            string anthologiesAbsPath;

            lock (createDirLock)
            {
                anthologiesAbsPath = AnthologiesAbsPath(anthologySection, anthologyName);
                if (!Directory.Exists(anthologiesAbsPath))
                    Directory.CreateDirectory(anthologiesAbsPath);
            }

            string atomAbsPath = cache.Absolute(Root, path); // will check path for illegal chars

            if (!File.Exists(atomAbsPath))
                throw new Exception("Path " + path + " does not exist");

            var destPath = GetAnthologyItemDestPath(path, anthologiesAbsPath);

            // do our own locking here to prevent the file system locking from barfing on us
            lock (GetLockObj(destPath))
            {
                File.Copy(atomAbsPath, destPath, overwrite: true);
            }

            indexer.SetAnthologized(path);

            // next index the file in its copied form in the anthologies folder
            String anthItemRelPath = GetAnthologyItemRelPath(anthologySection, anthologyName, path);
            indexer.IndexItem(anthologySection, anthologyName, anthItemRelPath );

            return Directory.GetFiles(anthologiesAbsPath).Length;

        }

        private string AnthologiesAbsPath(string anthologySection, string anthologyName)
        {
            return cache.Absolute(AnthologiesDir, anthologySection + Path.DirectorySeparatorChar + anthologyName);
        }

        private static string GetAnthologyItemDestPath(string path, string anthologiesAbsPath)
        {
            var fname = GetAnthologyFilename(path);

            string destPath = anthologiesAbsPath.TrimEnd(Path.DirectorySeparatorChar, '/') +
                              Path.DirectorySeparatorChar + fname.TrimStart(Path.DirectorySeparatorChar, '/');
            return destPath;
        }

        private string GetAnthologyItemRelPath(string anthologySection, string anthologyName, string path)
        {
            var fname = GetAnthologyFilename(path);

            string destPath =
                new DirectoryInfo(AnthologiesDir).Name + 
                Path.DirectorySeparatorChar + anthologySection + 
                Path.DirectorySeparatorChar + anthologyName + 
                Path.DirectorySeparatorChar + fname.TrimStart(Path.DirectorySeparatorChar, '/');
            return destPath;
        }

        private static string GetAnthologyFilename(string path)
        {
            var pathInfo = new FileInfo(path);
            string lname = pathInfo.Name.ToLower();
            DirectoryInfo parent = pathInfo.Directory;
            
            DirectoryInfo gparent = (parent == null) ? null : parent.Parent;

            StringBuilder result = new StringBuilder();

            if (gparent != null && 
                gparent.Parent != null &&  
                !parent.Name.ToLower().Contains(gparent.Name.ToLower()) && 
                !lname.Contains(gparent.Name.ToLower()))
            {
                result.Append(gparent.Name);
                result.Append("-");
            }

            if (parent.Parent != null && parent != null && !lname.Contains(parent.Name.ToLower()))
            {
                result.Append(parent.Name);
                result.Append("-");
            }

            result.Append(pathInfo.Name);

            return result.ToString();
        }

        public int UnAnthologize(string anthologySection, string anthologyName, string path)
        {
            try
            {
                return tryUnAnthologize(anthologySection, anthologyName, path);
            }
            catch (Exception e)
            {
                LogError(Resources.AnthologizerService_UnAnthologize_Could_not_unanthologize_ + " < " + path + " >", e);
                throw;
            }
        }

        private int tryUnAnthologize(string anthologySection, string anthologyName, string path)
        {
            string anthologiesAbsPath;

            anthologiesAbsPath = AnthologiesAbsPath(anthologySection, anthologyName);
            var destPath = GetAnthologyItemDestPath(path, anthologiesAbsPath);

            lock (GetLockObj(destPath))
            {
                if (File.Exists(destPath))
                    File.Delete(destPath);
            }
            return Directory.GetFiles(anthologiesAbsPath).Length;
        }

        public List<Item> ListAnthology(string anthologySection, string anthologyName)
        {
            try
            {
                string anthologiesAbsPath;
                string anthFolder = new DirectoryInfo(AnthologiesDir).Name;
                anthologiesAbsPath = AnthologiesAbsPath(anthologySection, anthologyName);

                List<Item> result = new List<Item>();

                DirectoryInfo dinfo = new DirectoryInfo(anthologiesAbsPath);
                if (dinfo.Exists)
                {
                    foreach (FileInfo f in dinfo.EnumerateFiles())
                    {
                        GetFileInfo(Path.DirectorySeparatorChar + anthFolder + Path.DirectorySeparatorChar + 
                                    anthologySection + Path.DirectorySeparatorChar + anthologyName + 
                                    Path.DirectorySeparatorChar + f.Name, f, result, true);
                    }
                }
                return result;
            }
            catch (Exception e)
            {
                LogError("Could not get anthology",e);
                throw;
            }
        }

    }

    namespace ExtensionMethods
    {
        public static class MyExtensions
        {
            /// <summary>
            /// We do not allow .. or other funny chars
            /// </summary>
            /// <param name="path"></param>
            /// <returns></returns>
            public static bool ContainsBadChars(this String path)
            {
                foreach (char c in Path.GetInvalidPathChars())
                    if (path.Contains(c))
                        return true;

                return path.Contains("../") || path.Contains(@"..\");
            }
        }
    }
}

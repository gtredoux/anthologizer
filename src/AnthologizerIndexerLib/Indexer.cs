using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using com.renoster.Anthologizer.Media;
using MediaData;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Item = MediaData.Item;
using System.Diagnostics;
using HundredMilesSoftware.UltraID3Lib;

namespace com.renoster.AnthologizerIndexerLib
{
    public class Indexer
    {
        private bool shouldStop = false;

        private Dictionary<string, bool> anthologizedCache = null;
   
        public delegate void FileIndexed(FileInfo f);
        public delegate void FolderIndexing(DirectoryInfo d);
        public delegate void FolderIndexed(DirectoryInfo d);
        public delegate void FileIndexingSkipped(FileInfo f);
        public delegate void ErrorIndexing(string name, Exception ex);

        public event FileIndexed OnFileIndexed;
        public event FileIndexingSkipped OnFileIndexingSkipped;

        public event FolderIndexed OnFolderIndexed; 
        public event FolderIndexing OnFolderIndexing;
        public event ErrorIndexing OnErrorIndexing;

        private string root;
        private string anthologiesDir;

        private MediaDataClassesDataContext db;

        private Regex anthRegex;
        private Regex trackTitleRegex;

        public const string EVENT_LOG_SOURCE = "Anthologizer";
        public const string EVENT_LOG_NAME = "Main";

        public static void CreateEventLog()
        {
            if (!EventLog.SourceExists(EVENT_LOG_SOURCE))
            {
                //An event log source should not be created and immediately used. 
                //There is a latency time to enable the source, it should be created 
                //prior to executing the application that uses the source. 
                //Execute this sample a second time to use the new source.
                EventLog.CreateEventSource(EVENT_LOG_SOURCE, EVENT_LOG_NAME);
            }
        }

        public static EventLog GetEventLog() {
            EventLog log = new EventLog();
            log.Source = EVENT_LOG_SOURCE;
            return log;

        }

        private EventLog log;

        public Indexer(string root, string panthologiesDir)
        {
            log =  GetEventLog();

            this.root = root;
            this.anthologiesDir = panthologiesDir;
            string anthDirName = new DirectoryInfo(anthologiesDir).Name;

            string sep = "\\\\";
            string anthologyE = anthDirName;
            const string sectionE = @"(?<section>[^\\]+)";
            const string nameE = @"(?<name>[^\\]+)";
            const string restE = @"(.*)";
            string anthRegexStr = @"^" + sep + anthologyE + sep + sectionE + sep + nameE + restE + @"$";
            anthRegex = new Regex(anthRegexStr, RegexOptions.Compiled);

            const string prefix = @"([^\d])?";
            const string suffix = @"([^\d])?";
            const string tracknum = @"(?<tracknum>\d+)";
            const string trackpart = @"(" + prefix + tracknum + suffix + ")?";
            const string name = @"(?<title>[^\.]+)";
            const string ext = @"(\.\w+)?";
            string trackTitleRegexStr = @"^" + trackpart + name + ext + @"$";
            trackTitleRegex = new Regex(trackTitleRegexStr, RegexOptions.Compiled);

            ConnectDB();

        }

        public void Index()
        {
            Dictionary<string, DirectoryInfo> exclude = new Dictionary<string, DirectoryInfo>();

            DirectoryInfo anthdirinfo = new DirectoryInfo(anthologiesDir);
            IndexFolder(@"\", anthdirinfo, exclude);

            rebuildAnthologizedCache();

            exclude.Add(anthdirinfo.FullName,anthdirinfo);

            if (Directory.Exists(root))
                IndexFolder(null, new DirectoryInfo(root), exclude);
            else if (File.Exists(root))
                IndexFile(null, new FileInfo(root), false, false);           
        }

 
        public void Stop()
        {
            shouldStop = true;
        }

        private void ConnectDB()
        {
            db = new MediaDataClassesDataContext();
            if (!db.DatabaseExists())
                db.CreateDatabase();
        }

        public void IndexFolder(string parentPath, DirectoryInfo dir, Dictionary<string, DirectoryInfo> exclude)
        {
            if (!dir.Exists || shouldStop)
                return;

            if (OnFolderIndexing != null)
                OnFolderIndexing(dir);

            try
            {
                var thisPath = GrowPath(parentPath, dir.Name);

                MediaData.Anthology anthology=null;
                string anthSection;
                string anthName;
                if (IsAnthology(thisPath, out anthSection, out anthName))
                {
                    anthology = RecordAnthology(thisPath, anthSection, anthName);
                } 
                    
                RecordFolder(dir, thisPath);  // also record this as an item 

                foreach (FileInfo f in dir.GetFiles())
                {
                    if (shouldStop)
                        return;

                    if (exclude.ContainsKey(f.FullName))
                        continue;

                    IndexItem(thisPath, f, anthology);
                }

                foreach (DirectoryInfo d in dir.GetDirectories())
                {
                    if (shouldStop)
                        return;

                    if (exclude.ContainsKey(d.FullName))
                        continue;

                    IndexFolder(thisPath, d,  exclude);
                }

                if (OnFolderIndexed != null)
                    OnFolderIndexed(dir);
            }
            catch (Exception ex)
            {
                ErrorIndexingFolder(dir, ex);
            }
        }

        public void IndexItem(string anthSection, string anthName, string thisPath )
        {
            FileInfo f = new FileInfo(root + Path.DirectorySeparatorChar + thisPath.TrimStart(Path.DirectorySeparatorChar));
            Anthology anthology = RecordAnthology(thisPath, anthSection, anthName);
            IndexItem(thisPath,f,anthology);
        }

        public void IndexItem(string thisPath, FileInfo f, Anthology anthology)
        {
            MediaData.Item item = IndexFile(thisPath, f, anthology != null, anthology != null);
            if (item != null && anthology != null)
            {
                AddItemToAnthology(anthology, item);
            }
        }

        private void AddItemToAnthology(Anthology anthology, Item item)
        {
            MediaData.AnthologyItem record =
                (from o in db.AnthologyItems
                 where (o.anthology_id == anthology.Id) && (o.item_id == item.Id)
                 select o).SingleOrDefault();

            if (record == default(MediaData.AnthologyItem))
            {
                record = new AnthologyItem();
                record.anthology_id = anthology.Id;
                record.item_id = item.Id;
                db.AnthologyItems.InsertOnSubmit(record);
            }

            item.anthologized = 1;
            db.SubmitChanges();

            if (anthologizedCache != null && !anthologizedCache.ContainsKey(item.digest))
                anthologizedCache.Add(item.digest,true);
        }

        private MediaData.Anthology RecordAnthology(string thisPath, string anthSection, string anthName)
        {
            MediaData.AnthologySection section = 
                (from o in db.AnthologySections
                 where o.name == anthSection
                 select o).SingleOrDefault();

            if (section == default(MediaData.AnthologySection))
            {
                section = new MediaData.AnthologySection();
                section.name = anthSection;
                db.AnthologySections.InsertOnSubmit(section);
            }
            // otherwise it already exists and nothing can be updated

            MediaData.Anthology a = 
                (from o in db.Anthologies
                 where o.name == anthName
                 select o).SingleOrDefault();

            if (a == default(MediaData.Anthology))
            {
                a = new MediaData.Anthology();
                a.name = anthName;
                a.sectionid = section.Id;
                db.Anthologies.InsertOnSubmit(a);
            } else
                a.sectionid = section.Id; // maybe that changed

            db.SubmitChanges();

            return a;
        }

        public bool IsAnthology(string path, out string section, out string name)
        {
            section = null;
            name = null;

            Match m = anthRegex.Match(path);
            if (m != null && m.Success)
            {
                section = m.Groups["section"].Value;
                name = m.Groups["name"].Value;
                return true;
            }

            return false;
        }

        private void RecordFolder(DirectoryInfo dir, string thisPath)
        {
            bool newitem;

            var item = GetItem(thisPath, out newitem);

            item.name = dir.Name;
            item.lastmodified = dir.LastWriteTime.Ticks;
            item.digest = thisPath;
            item.mimetype = Mime.FolderMimeType;
            item.path = thisPath;

            if (newitem)
                db.Items.InsertOnSubmit(item);
            db.SubmitChanges();
        }

        private void ErrorIndexingFolder(DirectoryInfo dir, Exception ex)
        {
            if (OnErrorIndexing != null)
                OnErrorIndexing(dir.FullName, ex);
        }

        private void ErrorIndexingFile(FileInfo f, Exception ex)
        {
            if (OnErrorIndexing != null)
                OnErrorIndexing(f.FullName, ex);
        }

        private static string GrowPath(string parentPath, string name)
        {
            if (parentPath == null)
                return "" + Path.DirectorySeparatorChar;

            StringBuilder pathBuilder = new StringBuilder(parentPath.TrimEnd(Path.DirectorySeparatorChar));
            pathBuilder.Append(Path.DirectorySeparatorChar);
            pathBuilder.Append(name.TrimStart(Path.DirectorySeparatorChar));
            string thisPath = pathBuilder.ToString();
            return thisPath;
        }

        private MediaData.Item IndexFile(string parentPath, FileInfo f, bool knownToBeAnthologized, bool anAnthologyTrack)
        {
            if (!f.Exists || shouldStop)
                return null;

            var thisPath = GrowPath(parentPath, f.Name);

            string theMimeType = Mime.GetMIMEType(f.Name);
            if (!ShouldIndex(theMimeType))
                return null;

            try
            {
                bool skipped;
                MediaData.Item item = RecordFile(f, thisPath, theMimeType, true, knownToBeAnthologized, anAnthologyTrack, out skipped);

                if (!skipped && (OnFileIndexed != null))
                    OnFileIndexed(f);

                if (skipped && (OnFileIndexingSkipped != null))
                    OnFileIndexingSkipped(f);

                return item;
            }
            catch(Exception ex)
            {
                ErrorIndexingFile(f,ex);
                return null;
            }
        }

        /// <summary>
        /// Gets the item and creates it if necessary but does not update it if it already exists
        /// </summary>
        /// <param name="relpath"></param>
        /// <returns></returns>
        public MediaData.Item GetFile(string relpath, bool knownToBeAnthologized, bool anAnthologyTrack)
        {
            string abspath = Compose(root, relpath);
            FileInfo f = new FileInfo(abspath);
            bool skipped;
            MediaData.Item item = RecordFile(f, relpath, Mime.GetMIMEType(f.Name), false, knownToBeAnthologized, anAnthologyTrack, out skipped);
            return item;
        }

        private string Compose(string rootp, string relpath)
        {
            rootp = rootp.Replace('/', Path.DirectorySeparatorChar);
            relpath = relpath.Replace('/', Path.DirectorySeparatorChar);

            return rootp.TrimEnd(Path.DirectorySeparatorChar) +
                Path.DirectorySeparatorChar + 
                relpath.TrimStart(Path.DirectorySeparatorChar);
        }

        public void SetAnthologized(string thisPath)
        {
            bool newitem;
            var item = GetItem(thisPath, out newitem);
            item.anthologized = 1;
            if (newitem)
                db.Items.InsertOnSubmit(item);

            db.SubmitChanges();
        }

        private MediaData.Item RecordFile(
            FileInfo f, string thisPath, string theMimeType, bool update, 
            bool knownToBeAnthologized, bool anAnthologyTrack, out bool skipped)
        {
            bool newitem;
            
            var item = GetItem(thisPath, out newitem);

            string title = GetTitle(f, item, anAnthologyTrack);

            bool isAnthologized = knownToBeAnthologized || (item.anthologized == 1) || CheckIsAthologized(item);
            bool dbaffected = newitem || (update && NeedsUpdate(f, item, isAnthologized, title));

            if (dbaffected)
            {
                if (newitem || (f.LastWriteTime.Ticks > item.lastmodified))
                {
                    string fdigest = GetDigest(f);
                    item.digest = fdigest;
                }

                item.name = title;
                item.lastmodified = f.LastWriteTime.Ticks;
                
                item.mimetype = theMimeType;
                item.path = thisPath;
                item.size = f.Length;
                item.anthologized = isAnthologized ? 1 : 0;
            }
            else
            {
                item.anthologized = isAnthologized ? 1 : 0;
            }

            if (newitem)
                db.Items.InsertOnSubmit(item);

            if (dbaffected)
            {
                skipped = false;
                db.SubmitChanges();
            }
            else
            {
                skipped = true;
            }
            return item;
        }

        private const string trackTitleFmt = "{0,2}. {1}";
        private const string anthologyTrackTitleFmt = "{0} [{1}, {2}]";

        private string GetTitle(FileInfo f, Item item, bool inAnthology)
        {
            short? tracknum;
            string artist;
            string album;
            string title = item.name == null ? f.Name : item.name;

            try
            {
                //GetUltraID3Info(f, out title, out artist, out album, out tracknum);
                //if (title == item.name || String.IsNullOrWhiteSpace(title))
                GetSharpTagInfo(f, out title, out artist, out album, out tracknum);

                // prefer the tag title unless it is obviously bogus
                if (!String.IsNullOrWhiteSpace(title) && (!title.Contains("Track") || item.name.Contains("Track")))
                {
                    //if (!inAnthology)
                    //    title = String.Format(trackTitleFmt, tracknum, title);
                    //else
                        title = String.Format(anthologyTrackTitleFmt, title, artist, album);
                }
                else
                {
                    if (item.name == null)
                        item.name = f.Name;
                    Match m = trackTitleRegex.Match(item.name);
                    // pretty desperate
                    if (m != null && m.Success)
                    {
                        string tracknumstr = m.Groups["tracknum"].Value;
                        tracknum = (String.IsNullOrWhiteSpace(tracknumstr)) ? (short)-1: short.Parse(tracknumstr);
                        title = m.Groups["title"].Value;
                    }
                    else
                    {
                        title = item.name;
                    }
                }
            }
            catch (Exception)
            {
                log.WriteEntry("Could not get tag info for track " + f.FullName);
                title = item.name;
            }

            return title;
        }

        private static void GetUltraID3Info(FileInfo f, out string title, out string artist, out string album, out short? tracknum)
        {
            UltraID3 u = new UltraID3();
            u.Read(f.FullName);
            title = u.Title;
            artist = u.Artist;
            album = u.Album;
            tracknum = u.TrackNum;
        }

        private static void GetSharpTagInfo(FileInfo f, out string title, out string artist, out string album, out short? tracknum)
        {
            TagLib.File tagSet = TagLib.File.Create(f.FullName);
            title = tagSet.Tag.Title;
            artist = tagSet.Tag.JoinedPerformers == String.Empty ? tagSet.Tag.JoinedAlbumArtists : tagSet.Tag.JoinedPerformers;
            album = tagSet.Tag.Album;
            tracknum =(short) tagSet.Tag.Track;
        }

        private bool NeedsUpdate(FileInfo f, Item item, bool isAnthologized, string newtitle)
        {
            return 
                (f.LastWriteTime.Ticks > item.lastmodified) || 
                String.IsNullOrEmpty(item.digest) || 
                (f.Length != item.size) ||
                (item.name != newtitle) ||
                ((isAnthologized ? 1: 0) != item.anthologized);
        }

        private Item GetItem(string thisPath, out bool newitem)
        {
            newitem = false;
            //MediaData.Item item = (from item_o in db.Items
            //                       where item_o.path == thisPath
            //                       select item_o).SingleOrDefault();

            foreach (Item item in db.ExecuteQuery<MediaData.Item>
                (@"SELECT * from Item where pathCS=CHECKSUM({0})", thisPath))
            {
                if (item.path != thisPath)
                    continue;

                if (item == default(MediaData.Item))
                {
                    newitem = true;
                    return new MediaData.Item();
                } 
                return item;
            }

            newitem = true;
            return new MediaData.Item();
        }

        private SHA256 shaM = new SHA256Managed();

        private string GetDigest(FileInfo f)
        {
            byte[] digest;
            digest = shaM.ComputeHash(f.OpenRead());
            
            string digestStr = System.Convert.ToBase64String(digest);
            return digestStr;
        }

        private bool ShouldIndex(string mimeType)
        {
            return mimeType != null && mimeType.StartsWith("audio/");
        }

        public bool CheckIsAthologized(Item item)
        {
            if (anthologizedCache != null)
                return anthologizedCache.ContainsKey(item.digest);
            else
            {
                var query =
                    (from i in db.Items
                        join o in db.AnthologyItems on i.Id equals o.item_id
                        where i.digest == item.digest
                        select i.digest);

                foreach (var result in query)
                    return true;

                return false;
            }
        }

        private void rebuildAnthologizedCache()
        {
            anthologizedCache = new Dictionary<string, bool>();
            var query =
                (from i in db.Items
                 join o in db.AnthologyItems on i.Id equals o.item_id
                 select i.digest);

            foreach (var result in query)
                if (!anthologizedCache.ContainsKey(result))
                    anthologizedCache.Add(result,true);
        }
    }
}

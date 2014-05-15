using com.renoster.LRUCache;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.renoster.FileCacheLib
{
    public class FileCache<T>
    {
        private LRUCache<String, CacheEntry<List<T>>> cache;
        private string root;

        public delegate bool MatchesDelegate(T item, string path);
        private MatchesDelegate matches;

        public FileCache(MatchesDelegate matches, String root, int limit)
        {
            this.root = root;
            this.matches = matches;
            cache = new LRUCache<String, CacheEntry<List<T>>>(limit);
        }

        public string GetParent(string inAbsPath)
        {
            string abspath;
            if (File.Exists(inAbsPath))
                abspath = new FileInfo(inAbsPath).DirectoryName;
            else
                abspath = new DirectoryInfo(inAbsPath).Parent.FullName;
            return abspath;
        }

        public FileStream WaitForFile(string fullPath, FileAccess access, FileShare share, int maxtries)
        {
            int numTries = 0;
            while (true)
            {
                ++numTries;
                try
                {
                    // Attempt to open the file exclusively.
                    FileStream fs = new FileStream(fullPath, FileMode.Open, access, share);
                    return fs;
                }
                catch (Exception)
                {
                    if (numTries > maxtries)
                        throw new IOException("Could not open file " + fullPath + " after " + maxtries + " retries");
                    
                    // Wait for the lock to be released
                    System.Threading.Thread.Sleep(500);
                }
            }
        }

        public void DeleteItemFromParent(string abschildpath)
        {
            String abspath = GetParent(abschildpath);
            String ppath = getRelativePath(abspath);
            CacheEntry<List<T>> set = cache.get(ppath);
            if (set == null)
                return;
            var pos = getPos(getRelativePath(abschildpath), set.Contents);
            if (pos >= 0)
                set.Contents.RemoveAt(pos);
        }

        private int getPos(string path, List<T> set)
        {
            int pos;
            for (pos = 0; pos < set.Count; pos++)
                if (matches(set[pos], path))
                {
                    break;
                }
            return (pos >= set.Count) ? -1 : pos;
        }

        public void RemoveParentFromCache(String abschildpath)
        {
            String abspath = GetParent(abschildpath);
            String path = getRelativePath(abspath);
            lock (cache)
            {
                if (cache.contains(path))
                    cache.remove(path);
            }
        }

        public void AddItemToParent(string abschildpath, T item)
        {
            String abspath = GetParent(abschildpath);
            String parentPath = getRelativePath(abspath);
            lock (this)
            {
                CacheEntry<List<T>> entry = cache.get(parentPath);
                if (entry == null)
                    return;

                var pos = getPos(getRelativePath(abschildpath), entry.Contents);
                if (pos >= 0)
                    entry.Contents.RemoveAt(pos);

                entry.Contents.Insert(pos, item);
                entry.LastModified = GetLastWriteTime(abschildpath);
            }
        }

        public String getRelativePath(String abspath)
        {
            String path = abspath.Replace(root, "");
            return path;
        }

        public void PutInCache(string relpath, List<T> result)
        {
            string path = Absolute(root, relpath);
            var lastWriteTime = GetLastWriteTime(path);
            var entry = new CacheEntry<List<T>>(lastWriteTime, result);
            lock (cache)
            {
                if (cache.contains(path))
                    cache.remove(path);
                cache.add(path, entry);
            }   
        }

        public string Absolute(string rootPath, string path)
        {
            //if (path.ContainsBadChars())
            //    throw new Exception(Resources.AnthologizerService_Absolute_Bad_path_ + path);

            string root = rootPath.TrimEnd(Path.DirectorySeparatorChar);
            string npath = path.Replace('/', Path.DirectorySeparatorChar);
            string result = root + Path.DirectorySeparatorChar + npath.TrimStart(Path.DirectorySeparatorChar);
            return result.TrimEnd(Path.DirectorySeparatorChar);
        }

        private static long GetLastWriteTime(string path)
        {
            DirectoryInfo dinfo = new DirectoryInfo(path);
            if (dinfo.Exists)
                return dinfo.LastWriteTime.Ticks;

            FileInfo f = new FileInfo(path);
            if (f.Exists)
                return f.LastWriteTime.Ticks;

            throw new ArgumentException("The path " + path + " is neither a directory nor a file");
        }

        public List<T> GetFromCache(string relpath)
        {
            string path = Absolute(root, relpath);
            List<T> result = null;
 
            CacheEntry<List<T>> entry = cache.get(path);
            if (entry != null && GetLastWriteTime(path) <= entry.LastModified)
                return entry.Contents;
 
            return result;
        }
    }
}

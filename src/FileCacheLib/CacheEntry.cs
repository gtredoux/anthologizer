using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace com.renoster.FileCacheLib
{
    public class CacheEntry<T>
    {
        private long lastModified;
        private T contents;

        public CacheEntry(long lastModified, T contents)
        {
            this.lastModified = lastModified;
            this.contents = contents;
        }

        public long LastModified
        {
            get { return lastModified; }
            set { lastModified = value; }
        }

        public T Contents
        {
            get { return contents; }
            set { contents = value; }
        }
    }
}
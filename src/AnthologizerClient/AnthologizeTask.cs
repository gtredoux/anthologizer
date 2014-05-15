using com.renoster.Anthologizer.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;

namespace AnthologizerClient
{
    public class AnthologizeTask : MediaMgrTask
    {
        public enum ActionEnum
        {
            Add,
            Remove
        };

        private Item item;
        private ActionEnum action;
        private string section;
        private string name;
        private string path;
        private int count;

        public AnthologizeTask(Item item, ActionEnum action, MediaMgr mediaMgr, string section, string name, string path, AsyncTaskCompletedEvent notify)
            : base(mediaMgr,notify)
        {
            this.item = item;
            this.action = action;
            this.section = section;
            this.name = name;
            this.path = path;
        }

        public AnthologizeTask(Item item, ActionEnum action, MediaMgr mediaMgr, string section, string name, string path)
            : this(item,action, mediaMgr,section,name,path,null)
        {
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

        public string Path
        {
            get { return path; }
            set { path = value; }
        }

        public int Count {
            get { return count; }
            set { count=value; } 
        }

        public ActionEnum Action
        {
            get { return action; }
            set { action = value; }
        }

        public com.renoster.Anthologizer.Media.Item Item
        {
            get { return item; } 
            set { item = value; }
        }
    }
}

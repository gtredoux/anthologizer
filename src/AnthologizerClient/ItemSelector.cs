using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;
using com.renoster.Anthologizer.Media;

namespace AnthologizerClient
{
    public class ItemSelector
    {
        public delegate void AddEvent(ItemSelector itemUI);

        public event AddEvent OnAdd;
        
        private Item item;

        public ItemSelector(Item pitem)
        {
            item = pitem;
        }

        public ItemTypeEnum ItemType
        {
            get { return item.ItemType; }
        }

        public string Name
        {
            get { return item.Name;   }
        }

        private bool anthologize = false;

        public Boolean Add
        {
            get { return anthologize; }
            set
            {
                anthologize = value;
                if (OnAdd != null)
                    OnAdd(this);
            }
        }

        public Boolean Anthologized
        {
            get { return item.Anthologized; }
           
        }


        public Item GetItem()
        {
            return item;
        }
    }
}

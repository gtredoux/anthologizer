using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnthologizerClient
{
    public class MediaClientFactory
    {
        public static IMediaClient Create(string url)
        {
            // switch here
            //return new MediaClientSOAP(url);
            return new MediaClientREST(url);
        }
    }
}

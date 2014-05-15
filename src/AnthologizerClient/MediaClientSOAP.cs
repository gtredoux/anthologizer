using AnthologizerClient.AnthologizerService;
using com.renoster.Anthologizer.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace AnthologizerClient
{
    class MediaClientSOAP: IMediaClient
    {
        private string url;
        private AnthologizerService.AnthologizerServiceClient client;

        public MediaClientSOAP(string purl)
        {
            url = purl;
        }

        public AnthologizerServiceClient Client
        {
            get
            {
                lock (this)
                {
                    if (client == null)
                    {
                        string binding = "WSHttpBinding_IAnthologizerService";
                        EndpointAddress end = new EndpointAddress(url);
                        client = new AnthologizerServiceClient(binding, end);
                    }
                    return client;
                }
            }
        }

        public void Normalize(NormalizeEnum action, string path)
        {
            throw new NotImplementedException();            
        }

        public System.IO.Stream GetAtomic(string path)
        {
            return Client.GetAtomic(path);
        }

        public Item[] GetComposite(string path)
        {
            Item[] result = Client.GetComposite(path);
            return result;
        }

        public Task GetCompositeAsync(GetCompositeTask getctask, MediaMgrTask.AsyncTaskCompletedEvent notify)
        {
            throw new NotImplementedException();
        }

        public int Anthologize(string anthologySection, string anthologyName, string path)
        {
            throw new NotImplementedException();
        }

        public int UnAnthologize(string anthologySection, string anthologyName, string path)
        {
            throw new NotImplementedException();            
        }

        public Item[] ListAnthology(string anthologySection, string anthologyName)
        {
            throw new NotImplementedException();  
        }

        public void Cancel()
        {
            throw new NotImplementedException();  
        }
    }
}

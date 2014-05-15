using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using AnthologizerClient.AnthologizerService;
using com.renoster.Anthologizer.Media;
using System.Threading;
using System.Net.Http;

namespace AnthologizerClient
{
    public class MediaClientREST: IMediaClient
    {
        private string host;

        private object webRquestLock = new object();

        public void Cancel()
        {
            lock (webRquestLock)
            {
                if (webRequest != null)
                    webRequest.Abort();
            }
        }

        public MediaClientREST(string host)
        {
            this.host = host;

        }

        private HttpWebRequest webRequest=null;

        private const string normalizeUriFmt = "REST/Normalize?path={0}&action={1}";
        private const string atomUriFmt = "REST/GetAtom?path={0}";
        private const string compositeUriFmt = "REST/GetComposite?path={0}";
        private const string anthologizeUriFmt = "REST/Anthologize?anthologySection={0}&anthologyName={1}&path={2}";
        private const string unanthologizeUriFmt = "REST/UnAnthologize?anthologySection={0}&anthologyName={1}&path={2}";
        private const string listAnthologyUriFmt = "REST/ListAnthology?anthologySection={0}&anthologyName={1}";

        public void Normalize(NormalizeEnum action, string path)
        {
            string escpath = Uri.EscapeUriString(path);

            StringBuilder uri = new StringBuilder(host);
            uri.Append("/");
            string encpath = Uri.EscapeDataString(path);
            string encaction = Uri.EscapeDataString(action.ToString());
            uri.Append(String.Format(normalizeUriFmt, encpath, encaction));

            var webRequest = (HttpWebRequest)WebRequest.Create(uri.ToString());
            webRequest.Method = "GET";
            var webResponse = (HttpWebResponse)webRequest.GetResponse();

            lock (webRquestLock)
            {
                webRequest = null;
            }
        }

        public System.IO.Stream GetAtomic(string path)
        {
            long contentLength;
            Stream result = GetAtomic(path, out contentLength);
            return result;
        }

        public System.IO.Stream GetAtomic(string path, out long contentLength)
        {
            string furi;

            furi = GetUri(host, atomUriFmt, path);
            
            var webRequest = (HttpWebRequest)WebRequest.Create(furi);
            webRequest.Method = "GET";
            var webResponse = (HttpWebResponse)webRequest.GetResponse();
            contentLength = webResponse.ContentLength;
            lock (webRquestLock)
            {
                webRequest = null;
            }
            if (webResponse == null)
                return null;
            return webResponse.GetResponseStream();
        }

        public static string GetAtomicURI(string host, string path)
        {
            return GetUri(host, atomUriFmt, path);      
        }

        private static string GetUri(string host, string fmt, string path)
        {
            if (path.StartsWith("http://") || path.StartsWith("https://"))
            {
                return path;
            }

            string escpath = Uri.EscapeUriString(path);
            StringBuilder uri = new StringBuilder(host);
            uri.Append("/");
            string encpath = Uri.EscapeDataString(path);
            uri.Append(String.Format(fmt, encpath));
            String furi = uri.ToString();
            return furi;
        }

        public Item[] GetComposite(string path)
        {
            var furi = GetUri(host, compositeUriFmt, path);
            webRequest = (HttpWebRequest)WebRequest.Create(furi);
            webRequest.Method = "GET";
            var webResponse = (HttpWebResponse)webRequest.GetResponse();
            lock (webRquestLock)
            {
                webRequest = null;
            }
            if (webResponse == null || webResponse.StatusCode != HttpStatusCode.OK)
                return null;
            XmlSerializer Serializer = new XmlSerializer(typeof(Item[]));
            return (Item[])Serializer.Deserialize(webResponse.GetResponseStream());            
        }

        public async Task GetCompositeAsync(GetCompositeTask getctask, MediaMgrTask.AsyncTaskCompletedEvent notify)
        {
            HttpClient client = new HttpClient();

            var furi = GetUri(host, compositeUriFmt, getctask.Path);

            Task<HttpResponseMessage> uptask = client.GetAsync(furi, getctask.CancellationTokenSource.Token);
            await uptask;

            if (uptask.IsCanceled)
                return;

            if ((!uptask.IsFaulted) && uptask.Result.StatusCode == HttpStatusCode.OK)
            {
                string response = await uptask.Result.Content.ReadAsStringAsync();
                XmlSerializer Serializer = new XmlSerializer(typeof (Item[]));
                getctask.Results = (Item[]) Serializer.Deserialize(new StringReader(response));
            }
            else
            {
                if (uptask.Exception != null)
                    getctask.ErrorException = uptask.Exception;
            }

            if (notify != null)
                notify (getctask);
        }

        public int Anthologize(string anthologySection, string anthologyName, string path)
        {
            StringBuilder uri = new StringBuilder(host);
            uri.Append("/");
            uri.Append(
                String.Format(
                    anthologizeUriFmt, 
                    Uri.EscapeDataString(anthologySection), 
                    Uri.EscapeDataString(anthologyName), 
                    Uri.EscapeDataString(path)
                )
            );
            String furi = uri.ToString();

            webRequest = (HttpWebRequest)WebRequest.Create(furi);
            webRequest.Method = "GET";
            var webResponse = (HttpWebResponse)webRequest.GetResponse();
            lock (webRquestLock)
            {
                webRequest = null;
            }
            if (webResponse == null || webResponse.StatusCode != HttpStatusCode.OK)
                throw new Exception(
                    "Anthologize failed for section <" + 
                    anthologySection + "> with name <" + 
                    anthologyName + "> and path <" + 
                    path + ">");

            XmlSerializer Serializer = new XmlSerializer(typeof(int));
            return (int)Serializer.Deserialize(webResponse.GetResponseStream());    
        }

        public int UnAnthologize(string anthologySection, string anthologyName, string path)
        {
            StringBuilder uri = new StringBuilder(host);
            uri.Append("/");
            uri.Append(
                String.Format(
                    unanthologizeUriFmt,
                    Uri.EscapeDataString(anthologySection),
                    Uri.EscapeDataString(anthologyName),
                    Uri.EscapeDataString(path)
                )
            );
            String furi = uri.ToString();

            webRequest = (HttpWebRequest)WebRequest.Create(furi);
            webRequest.Method = "GET";
            var webResponse = (HttpWebResponse)webRequest.GetResponse();
            lock (webRquestLock)
            {
                webRequest = null;
            }
            if (webResponse == null || webResponse.StatusCode != HttpStatusCode.OK)
                throw new Exception(
                    "UnAnthologize failed for section <" +
                    anthologySection + "> with name <" +
                    anthologyName + "> and path <" +
                    path + ">");

            XmlSerializer Serializer = new XmlSerializer(typeof(int));
            return (int)Serializer.Deserialize(webResponse.GetResponseStream()); 
        }

        public Item[] ListAnthology(string anthologySection, string anthologyName)
        {
            StringBuilder uri = new StringBuilder(host);
            uri.Append("/");
            uri.Append(
                String.Format(
                    listAnthologyUriFmt,
                    Uri.EscapeDataString(anthologySection),
                    Uri.EscapeDataString(anthologyName)
                )
            );
            String furi = uri.ToString();

            webRequest = (HttpWebRequest)WebRequest.Create(furi);
            webRequest.Method = "GET";
            var webResponse = (HttpWebResponse)webRequest.GetResponse();
            lock (webRquestLock)
            {
                webRequest = null;
            }
            if (webResponse == null || webResponse.StatusCode != HttpStatusCode.OK)
                return null;
            XmlSerializer Serializer = new XmlSerializer(typeof(Item[]));
            return (Item[])Serializer.Deserialize(webResponse.GetResponseStream());       
        }
    }
}

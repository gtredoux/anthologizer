using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AnthologizerClient.AnthologizerService;
using System.IO;
using com.renoster.Anthologizer.Media;

namespace AnthologizerClient
{
    public interface IMediaClient
    {
        void Cancel();

        void Normalize(NormalizeEnum action, string path);

        Stream GetAtomic(string path);

        Item[] GetComposite(string path);

        Task GetCompositeAsync(GetCompositeTask getctask, MediaMgrTask.AsyncTaskCompletedEvent notify);

        int Anthologize(string anthologySection, string anthologyName, string path);
        int UnAnthologize(string anthologySection, string anthologyName, string path);

        Item[] ListAnthology(string anthologySection, string anthologyName);
    }
}

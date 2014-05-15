using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using com.renoster.Anthologizer.Media;

namespace com.renoster.Anthologizer.Contract
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IAnthologizerService" in both code and config file together.

    [XmlSerializerFormat]
    [ServiceContract(Namespace = "http://renoster.com/Anthologizer")]
    public interface IAnthologizerService
    {

        [OperationContract]
        [WebGet(UriTemplate = "Normalize?path={path}&action={action}")]
        void Normalize(NormalizeEnum action, string path);

        [OperationContract]
        [WebGet(UriTemplate  = "GetComposite?path={path}")]
        List<Item> GetComposite(string path);

        [OperationContract]
        [WebGet(UriTemplate = "GetRandom?context={context}&maxcount={maxcount}&path={path}")]
        List<Item> GetRandom(string context, int maxcount, string path);

        [OperationContract]
        [WebGet(UriTemplate = "GetAtom?path={path}")]
        Stream GetAtomic(string path);

        [OperationContract]
        [WebGet(UriTemplate = "Anthologize?anthologySection={anthologySection}&anthologyName={anthologyName}&path={path}")]
        int Anthologize(string anthologySection, string anthologyName, string path);

        [OperationContract]
        [WebGet(UriTemplate = "UnAnthologize?anthologySection={anthologySection}&anthologyName={anthologyName}&path={path}")]
        int UnAnthologize(string anthologySection, string anthologyName, string path);

        [OperationContract]
        [WebGet(UriTemplate = "ListAnthology?anthologySection={anthologySection}&anthologyName={anthologyName}")]
        List<Item> ListAnthology(string anthologySection, string anthologyName);
    }
}

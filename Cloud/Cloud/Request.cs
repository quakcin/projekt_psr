using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cloud
{
    [Serializable]
    public enum RequestType
    {
        Test,
        Listing /* List of directories and files */
    }

    [Serializable]
    public class Request
    {
        public RequestType requestType { get; set; }
        public Object body { get; set; }
    }
}

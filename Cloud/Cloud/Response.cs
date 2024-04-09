using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cloud
{
    [Serializable]
    public enum ResponseType
    {
        Test,
        Listing
    }

    [Serializable]
    public class Response
    {
        public ResponseType responseType { get; set; }
        public Object body { get; set; }
    }
}

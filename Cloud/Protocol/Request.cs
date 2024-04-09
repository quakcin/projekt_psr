using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cloud
{
    enum RequestType
    {
        Test
    }

    [Serializable]
    class Request
    {
        public RequestType requestType { get; set; }
        public Object body { get; set; }
    }
}

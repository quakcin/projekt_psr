using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


    enum ResponseType
    {
        Test
    }

    [Serializable]
    class Response
    {
        public ResponseType responseType { get; set; }
    }

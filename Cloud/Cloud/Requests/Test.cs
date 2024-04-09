using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cloud.Requests
{
    class Test
    {
        public static Response Exec (Request request)
        {
            Response response = new Response();
            response.responseType = ResponseType.Test;
            response.body = null;
            return response;
        }
    }
}

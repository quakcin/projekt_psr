using Cloud.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cloud
{
    internal class RequestHandler
    {
        public static Response Handle (Request request)
        {
            Console.WriteLine("Serving request of type: " + request.requestType.ToString());
            switch (request.requestType)
            {
                case RequestType.Test:
                    return Test.Exec(request);

                case RequestType.Listing:
                    return Listing.Exec(request);
            }

            return null;
        }
    }
}

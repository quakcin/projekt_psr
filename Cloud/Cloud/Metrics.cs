using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cloud
{
    internal class Metrics
    {
        private String id;
        private String operation;
        private long start;

        public Metrics (String operationName)
        {
            operation = operationName;
            id = Guid.NewGuid().ToString();
            start = CurrentTimestamp();
        }

        public void Finish ()
        {
            long endTime = CurrentTimestamp();
            long duration = endTime - start;

            if (!Directory.Exists("logs"))
            {
                Directory.CreateDirectory("logs");
            }

            string path = "logs/" + operation + ".csv";
            string log = this.id + ";" + start.ToString() + ";" + endTime.ToString() + ";" + duration.ToString();

            using (StreamWriter sw = new StreamWriter(path, true))
            {
                sw.WriteLine(log);
            }

        }


        private static long CurrentTimestamp ()
        {
            DateTimeOffset now = DateTimeOffset.UtcNow;
            return now.ToUnixTimeMilliseconds();
        }
    }
}

using System;
using System.IO;

namespace BlobUploader
{
    public class BlobLog : StreamWriter
    {
        const string LAST_RUN_TIME_FILE = "LastRunTime.txt";

        public BlobLog() 
            : base("Log.txt", append: true)
        {
        }

        public override void WriteLine(string value)
        {
            base.WriteLine(value);
            Console.WriteLine(value);
        }

        public DateTime GetLastRunTime()
        {
            DateTime lastRunTime;
            using (var file = new StreamReader(LAST_RUN_TIME_FILE))
            {
                var lastRunTimeStr = file.ReadLine();
                if (string.IsNullOrWhiteSpace(lastRunTimeStr))
                {
                    throw new Exception(LAST_RUN_TIME_FILE + " is empty");
                }
                lastRunTime = DateTime.Parse(lastRunTimeStr);
            }
            return lastRunTime;
        }

        public void SetLastRunTime(DateTime value)
        {
            using (var file = new StreamWriter(LAST_RUN_TIME_FILE, append: false))
            {
                file.WriteLine(value);
            }
        }
    }
}

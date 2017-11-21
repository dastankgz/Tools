using System;
using System.Threading;

namespace CorePublisher
{
    public class Program
    {
        static int Main()
        {
            try
            {
                var dir = @"F:\mit\Projects\Distr-BackEnd\Distribution\Distribution.Web";
                var info = new PublishInfo
                {
                    Directory = dir,
                    ProjectFile = dir + @"\Distribution.Web.csproj",
                    OutputDirectory = dir + @"\bin\Release\PublishOutput",
                    Config = "Release",
                    ServiceName = "distribution",
                    ServicePath = "/opt/distribution",
                    Host = "8.8.8.8",
                    Port = 22,
                    User = "publisher",
                    Password = "VeryStrongP@$$w0rd"

                };
                var publisher = new Publisher(info);
                publisher.Publish();
                Thread.Sleep(1000 * 3);

                return 0;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.Message);
                Console.ReadLine();
                return -1;
            }
        }
    }
}
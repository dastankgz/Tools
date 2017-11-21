using System;
using System.Diagnostics;
using System.IO;
using Renci.SshNet;

namespace CorePublisher
{
    class Publisher
    {
        private readonly PublishInfo _info;

        public Publisher(PublishInfo info)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));

            _info = info;
        }

        public void Publish()
        {
            ClearBuildDir();
            Console.WriteLine("_____THE OUTPUT DIRECTORY CLEARED_____");

            Build();
            Console.WriteLine("_____THE PROJECT BUILT_____");

            Update();
            Console.WriteLine("_____PUBLISHED_____");
        }

        private void Update()
        {
            using (var client = new SshClient(_info.Host, _info.Port, _info.User, _info.Password))
            {
                client.Connect();
                var stop = client.RunCommand($"systemctl stop {_info.ServiceName}");

                if (stop.ExitStatus != 0)
                    throw new Exception("Unable to stop service: " + _info.ServiceName);

                var clear = client.RunCommand($"rm -rf {_info.ServicePath}//*");

                if (clear.ExitStatus != 0)
                    throw new Exception("Unable to clear the service directory: " + _info.ServicePath);

                CopyDllsToServer();

                var start = client.RunCommand($"systemctl start {_info.ServiceName}");

                if (start.ExitStatus != 0)
                    throw new Exception("Unable to start service: " + _info.ServiceName);
            }
        }

        private void CopyDllsToServer()
        {
            using (var client = new ScpClient(_info.Host, _info.Port, _info.User, _info.Password))
            {
                client.Connect();
                var directory = new DirectoryInfo(_info.OutputDirectory);
                client.Upload(directory, _info.ServicePath);
            }
        }

        private void ClearBuildDir()
        {
            var directory = new DirectoryInfo(_info.OutputDirectory);

            if (!directory.Exists)
                throw new Exception("Incorrect path to project output directory: " + _info.OutputDirectory);

            foreach (var file in directory.GetFiles())
                file.Delete();

            foreach (var dir in directory.GetDirectories())
                dir.Delete(true);
        }

        private void Build()
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "dotnet.exe",
                    Arguments = $"publish {_info.ProjectFile} --configuration {_info.Config} --output {_info.OutputDirectory}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };
            process.Start();
            process.WaitForExit();

            if (process.ExitCode != 0)
                throw new Exception("Error on building the project: " + process.StandardError.ReadToEnd());
        }
    }
}

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace TPL
{
    public class TPLScanner : IPScanner
    {
        public Task Scan(IPAddress[] ipAddresses, int[] ports)
        {
            var tasksToPing = new List<(Task<PingReply> pingTask, IPAddress ip)>();
            foreach (var ipAddress in ipAddresses)
                tasksToPing.Add((PingAddress(ipAddress), ipAddress));

            var mainTask = Task.Factory.StartNew(() =>
            {
                foreach (var taskToPing in tasksToPing)
                foreach (var port in ports)
                    taskToPing.pingTask.ContinueWith(previous =>
                        {
                            Console.WriteLine($"Pinged {taskToPing.ip} +++{previous.Result.Status}");
                            using var tcpClient = new TcpClient();
                            Console.WriteLine($"Checking {taskToPing.ip}:{port}");
                            var portStatusTask = tcpClient.ConnectAsync(taskToPing.ip, port, 3000);
                            portStatusTask.ContinueWith(portTask =>
                            {
                                Console.WriteLine($"Checked {taskToPing.ip}:{port} - {portTask.Result}");
                            }, TaskContinuationOptions.AttachedToParent | TaskContinuationOptions.OnlyOnRanToCompletion);
                        }, TaskContinuationOptions.AttachedToParent | TaskContinuationOptions.OnlyOnRanToCompletion);
            });
            mainTask.Wait();
            
            return Task.CompletedTask;
        }
        
        private static Task<PingReply> PingAddress(IPAddress ipAddress, int timeout = 5000)
        {
            using var ping = new Ping();
            Console.WriteLine($"Pinging in {ipAddress}");
            var taskToPing = ping.SendPingAsync(ipAddress, timeout);
            
            var _ = taskToPing.ContinueWith(task => {
                Console.WriteLine($"Pinged in {ipAddress}: {task.Result.Status}");
                if (task.Result.Status != IPStatus.Success) { throw new Exception(); }
            }, TaskContinuationOptions.AttachedToParent | TaskContinuationOptions.OnlyOnRanToCompletion);
            
            return taskToPing;
        }
    }
}
using Microsoft.Azure.Devices.Client;
using Opc.UaFx.Client;
using Opc.UaFx;
using Newtonsoft.Json;
using System.Net.Mime;
using System.Text;
using Microsoft.Azure.Amqp.Framing;

namespace ProjektIoTSdk.Device
{
    public class VirtualDevice
    {
        private readonly DeviceClient deviceClient;

        public VirtualDevice(DeviceClient deviceClient)
        {
            this.deviceClient = deviceClient;
        }

        public async Task SendMessages(int MachienNumber)
        {
            using (var client = new OpcClient("opc.tcp://localhost:4840/"))
            {
                client.Connect();

                var commands = new OpcReadNode[]
                {
                    new OpcReadNode($"ns=2;s=Device {MachienNumber}/ProductionStatus", OpcAttribute.DisplayName),
                    new OpcReadNode($"ns=2;s=Device {MachienNumber}/ProductionStatus"),

                    new OpcReadNode($"ns=2;s=Device {MachienNumber}/WorkorderId", OpcAttribute.DisplayName),
                    new OpcReadNode($"ns=2;s=Device {MachienNumber}/WorkorderId"),

                    new OpcReadNode($"ns=2;s=Device {MachienNumber}/Temperature", OpcAttribute.DisplayName),
                    new OpcReadNode($"ns=2;s=Device {MachienNumber}/Temperature"),

                    new OpcReadNode($"ns=2;s=Device {MachienNumber}/GoodCount", OpcAttribute.DisplayName),
                    new OpcReadNode($"ns=2;s=Device {MachienNumber}/GoodCount"),

                    new OpcReadNode($"ns=2;s=Device {MachienNumber}/BadCount", OpcAttribute.DisplayName),
                    new OpcReadNode($"ns=2;s=Device {MachienNumber}/BadCount"),

                    new OpcReadNode($"ns=2;s=Device {MachienNumber}/DeviceError", OpcAttribute.DisplayName),
                    new OpcReadNode($"ns=2;s=Device {MachienNumber}/DeviceError"),
                };

                var job = client.ReadNodes(commands); // dane w tablicę

                var d = "";
                foreach (var item in job)
                {
                    d += ($"{item.Value}: {item.SourceTimestamp}");
                }
                var dataString = JsonConvert.SerializeObject(d);

                Message eventMassage = new Message(Encoding.UTF8.GetBytes(dataString));
                eventMassage.ContentType = MediaTypeNames.Application.Json;
                eventMassage.ContentEncoding = "utf-8";

                //Console.WriteLine($"\t{DateTime.Now.ToLocalTime()} > Device: {MachienNumber} send message, Data: [{dataString}]"); // wypis w konsoli że wysłało

                await deviceClient.SendEventAsync(eventMassage); // oficjalne wysłanie
            }
            Console.WriteLine();
        }


    }
}
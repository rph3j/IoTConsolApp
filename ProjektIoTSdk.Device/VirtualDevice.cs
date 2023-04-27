using Microsoft.Azure.Devices.Client;
using Opc.UaFx.Client;
using Opc.UaFx;
using Newtonsoft.Json;
using System.Net.Mime;
using System.Text;
using Microsoft.Azure.Amqp.Framing;
using Org.BouncyCastle.Asn1.X500;

namespace ProjektIoTSdk.Device
{
    public class VirtualDevice
    {
        private readonly DeviceClient deviceClient;

        public VirtualDevice(DeviceClient deviceClient)
        {
            this.deviceClient = deviceClient;
        }


        #region SendMessage

        public async Task SendMessages(int MachienNumber)
        {
            using (var client = new OpcClient("opc.tcp://localhost:4840/"))
            {
                client.Connect();

                var commands = new OpcReadNode[]
                {
               
                    new OpcReadNode($"ns=2;s=Device {MachienNumber}/ProductionStatus"),

                    new OpcReadNode($"ns=2;s=Device {MachienNumber}/WorkorderId"),

                    new OpcReadNode($"ns=2;s=Device {MachienNumber}/Temperature"),

                    new OpcReadNode($"ns=2;s=Device {MachienNumber}/GoodCount"),

                    new OpcReadNode($"ns=2;s=Device {MachienNumber}/BadCount"),

                    new OpcReadNode($"ns=2;s=Device {MachienNumber}/DeviceError"),
                };

                var job = client.ReadNodes(commands); // dane w tablicę

                var data = new
                {
                    ProductionStatus = job.ElementAt(0).Value,
                    WorkorderId = job.ElementAt(1).Value,
                    temperature = job.ElementAt(2).Value,
                    GoodCount = job.ElementAt(3).Value,
                    BadCount = job.ElementAt(4).Value,
                    DeviceError = job.ElementAt(5).Value,
                };

                var dataString = JsonConvert.SerializeObject(data);

                Message eventMassage = new Message(Encoding.UTF8.GetBytes(dataString));
                eventMassage.ContentType = MediaTypeNames.Application.Json;
                eventMassage.ContentEncoding = "utf-8";

                //Console.WriteLine($"\t{DateTime.Now.ToLocalTime()} > Device: {MachienNumber} send message, Data: [{dataString}]"); // wypis w konsoli że wysłało

                await deviceClient.SendEventAsync(eventMassage); // oficjalne wysłanie
            }
            Console.WriteLine();
        }
        #endregion

        #region Emergancy Stop
        public async Task EmergancyStop(int MachienNumber)
        {
            using (var client = new OpcClient("opc.tcp://localhost:4840/"))
            {
                client.Connect();
                var method = new OpcCallMethod($"ns=2;s=Device {MachienNumber}", $"ns=2;s=Device {MachienNumber}/EmergencyStop");
                client.CallMethod(method);
            }
            Console.WriteLine();
        }

        #endregion
        #region Restart Error Status

        public async Task RES(int MachienNumber)
        {
            using (var client = new OpcClient("opc.tcp://localhost:4840/"))
            {
                client.Connect();
                var method = new OpcCallMethod($"ns=2;s=Device {MachienNumber}", $"ns=2;s=Device {MachienNumber}/ResetErrorStatus");
                client.CallMethod(method);
            }
            Console.WriteLine();
        }
        #endregion

        #region Direct Methods

        private async Task<MethodResponse> RESHandler(MethodRequest methodRequest, object userContext)
        {
            Console.WriteLine($"\t METHOD EXECUTED: {methodRequest.Name}");

            var payload = JsonConvert.DeserializeAnonymousType(methodRequest.DataAsJson, new { MachineNumber = default(int) });
            await RES(payload.MachineNumber);

            return new MethodResponse(0);
        }

        private async Task<MethodResponse> EmergancyStopHandler(MethodRequest methodRequest, object userContext)
        {
            Console.WriteLine($"\t METHOD EXECUTED: {methodRequest.Name}");

            var payload = JsonConvert.DeserializeAnonymousType(methodRequest.DataAsJson, new { MachineNumber = default(int) });
            await EmergancyStop(payload.MachineNumber);

            return new MethodResponse(0);
        }

        #endregion

    }
}
using Microsoft.Azure.Devices.Client;
using Opc.UaFx.Client;
using Opc.UaFx;
using Newtonsoft.Json;
using System.Net.Mime;
using System.Text;
using Microsoft.Azure.Amqp.Framing;
using Org.BouncyCastle.Asn1.X500;
using Microsoft.Azure.Devices.Shared;

namespace ProjektIoTSdk.Device
{
    public class VirtualDevice
    {
        private readonly DeviceClient deviceClient;
        private readonly int MachienNumber;

        public VirtualDevice(DeviceClient deviceClient, int MachienNumber)
        {
            this.deviceClient = deviceClient;
            this.MachienNumber = MachienNumber;
        }


        #region SendMessage

        public async Task SendMessages()
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
        public async Task EmergancyStop()
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

        public async Task RES()
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

            var payload = JsonConvert.DeserializeAnonymousType(methodRequest.DataAsJson, new {});
            await RES();

            return new MethodResponse(0);
        }

        private async Task<MethodResponse> EmergancyStopHandler(MethodRequest methodRequest, object userContext)
        {
            Console.WriteLine($"\t METHOD EXECUTED: {methodRequest.Name}");

            var payload = JsonConvert.DeserializeAnonymousType(methodRequest.DataAsJson, new {});
            await EmergancyStop();

            return new MethodResponse(0);
        }

        private static async Task<MethodResponse> DefaultSerivceHandler(MethodRequest methodRequest, object userContext)
        {
            Console.WriteLine($"\t METHOD EXECUTED: {methodRequest.Name}");

            await Task.Delay(1000);

            return new MethodResponse(0);
        }

        #endregion

        #region Device Twin

        public async Task UpdateTwinAsync()
        {
            var twin = await deviceClient.GetTwinAsync();

            Console.WriteLine($"\n Initil twin value received: \n{JsonConvert.SerializeObject(twin, Formatting.Indented)}");
            Console.WriteLine();

            var report = new TwinCollection();

            #region Read data from machine

            using (var client = new OpcClient("opc.tcp://localhost:4840/"))
            {
                client.Connect();
                var commands = new OpcReadNode[]
                {
                    new OpcReadNode($"ns=2;s=Device {MachienNumber}/ProductionRate"),
                    new OpcReadNode($"ns=2;s=Device {MachienNumber}/DeviceError")
                };

                var job = client.ReadNodes(commands);

                report["ProductionRate"] = job.ElementAt(0).Value;
                report["DeviceError"] = job.ElementAt(1).Value;
                
            }

            #endregion

            await deviceClient.UpdateReportedPropertiesAsync(report);
        }

        private async Task OnDesiredPropertyChange(TwinCollection desiredProperties, object _)
        {
            Console.WriteLine($"\tDesired property change\n\t {JsonConvert.SerializeObject(desiredProperties)}");
            Console.WriteLine("\tSending current time as repreted property");

            TwinCollection report = new TwinCollection();


            #region Read data from machine

            using (var client = new OpcClient("opc.tcp://localhost:4840/"))
            {
                client.Connect();
                if(desiredProperties.Contains("DeviceError") == true)
                {
                    var com = new OpcWriteNode($"ns=2;s=Device {MachienNumber}", desiredProperties.Contains("DeviceError"));
                    report["DeviceError"] = com;
                }
                else
                {
                    var com = new OpcReadNode($"ns=2;s=Device {MachienNumber}/DeviceError");
                    report["DeviceError"] = com;
                }

                if(desiredProperties.Contains("ProductionRate") == true)
                {
                    var com = new OpcWriteNode($"ns=2;s=Device {MachienNumber}", desiredProperties.Contains("ProductionRate"));
                    report["ProductionRate"] = com;
                }else{
                    var com = new OpcReadNode($"ns=2;s=Device {MachienNumber}/ProductionRate");
                    report["ProductionRate"] = com;
                }
            }
            #endregion

            await deviceClient.UpdateReportedPropertiesAsync(report);
        }
        #endregion

        public async Task InitializeHendlers()
        {
            await deviceClient.SetMethodDefaultHandlerAsync(DefaultSerivceHandler, deviceClient);
            await deviceClient.SetMethodHandlerAsync("RES", RESHandler, deviceClient);
            await deviceClient.SetMethodHandlerAsync("EmergancyStop", EmergancyStopHandler, deviceClient);

            await deviceClient.SetDesiredPropertyUpdateCallback(OnDesiredPropertyChange, deviceClient);
        }

    }
}
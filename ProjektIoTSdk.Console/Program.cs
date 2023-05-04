using ProjektIoTSdk.Device;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using System.IO;
using Opc.UaFx.Client;
using Opc.UaFx;
using Microsoft.Azure.Amqp.Framing;
using Newtonsoft.Json.Linq;


string config;
try
{
    #region Read Config file
    using (var sr = new StreamReader("A:\\VisualRepo\\ProjektIoTSdk.Console\\Config.json"))
    {
        config = sr.ReadToEnd();
    }
    #endregion

    JObject jsonObject = JObject.Parse(config);

    string KeyI = jsonObject["conectionStrings"][0].ToString();
    using var deviceClientI = DeviceClient.CreateFromConnectionString(KeyI, TransportType.Mqtt);
    await deviceClientI.OpenAsync();
    var deviceI = new VirtualDevice(deviceClientI, 1);

    string KeyII = jsonObject["conectionStrings"][1].ToString();
    using var deviceClientII = DeviceClient.CreateFromConnectionString(KeyII, TransportType.Mqtt);
    await deviceClientII.OpenAsync();
    var deviceII = new VirtualDevice(deviceClientII, 2);

    await deviceI.InitializeHendlers();
    await deviceII.InitializeHendlers();
    await deviceI.SendMessages();
    await deviceII.SendMessages();

    Console.WriteLine("Prase kay to continue...");
    Console.ReadLine();
}
catch (Exception e) 
{
    Console.WriteLine("The file could not be read:");
    Console.WriteLine(e.Message); 
}


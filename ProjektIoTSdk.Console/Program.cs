using ProjektIoTSdk.Device;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using System.IO;
using Opc.UaFx.Client;
using Opc.UaFx;
using Microsoft.Azure.Amqp.Framing;
using Newtonsoft.Json.Linq;


List<string> Keys = new List<string>();
int DevicesCount = 0;

#region Read Config file

try
{
    string config;
    
    using (var sr = new StreamReader("A:\\VisualRepo\\ProjektIoTSdk.Console\\Config.json"))
    {
        config = sr.ReadToEnd();
    }

    JObject jsonObject = JObject.Parse(config);
    
    // add config data to program properties.
    DevicesCount = (int)jsonObject["CountOfDevices"];
    for (int i = 0; i < DevicesCount; i++)
    {
        Keys.Add(jsonObject["conectionStrings"][i].ToString());
    }
}
catch (Exception e) 
{
    Console.WriteLine("The file could not be read:");
    Console.WriteLine(e.Message); 
}

#endregion


string KeyI = Keys[0];
using var deviceClientI = DeviceClient.CreateFromConnectionString(KeyI, TransportType.Mqtt);
await deviceClientI.OpenAsync();
var deviceI = new VirtualDevice(deviceClientI, 1);

await deviceI.InitializeHendlers();

await deviceI.UpdateTwinAsync();

await deviceI.SendMessages();

Console.WriteLine("Prase kay to continue...");
Console.ReadLine();

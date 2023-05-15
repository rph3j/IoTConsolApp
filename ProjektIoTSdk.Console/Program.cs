using ProjektIoTSdk.Device;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using System.IO;
using Opc.UaFx.Client;
using Opc.UaFx;
using Microsoft.Azure.Amqp.Framing;
using Newtonsoft.Json.Linq;


//List<string> Keys = new List<string>();
//List<DeviceClient> Clients = new List<DeviceClient>();
List<VirtualDevice> VirtualDevices = new List<VirtualDevice>();
int Delay = 0;

#region Read Config file

try
{
    string config;
    
    using (var sr = new StreamReader("..\\..\\..\\..\\Config.json"))
    {
        config = sr.ReadToEnd();
    }

    JObject jsonObject = JObject.Parse(config);

    // add config data to program properties.
    int counter = ((int)jsonObject["CountOfDevices"]);
    Delay = ((int)jsonObject["Delay"]);
    for (int i = 1; i <= counter; i++)
    {
        #region Conection

        string Key = jsonObject["conectionStrings"][i - 1].ToString();
        var deviceClient = DeviceClient.CreateFromConnectionString(Key, TransportType.Mqtt);
        await deviceClient.OpenAsync();
        var device = new VirtualDevice(deviceClient, i);

        VirtualDevices.Add(device);

        Console.Write("Connection succesful!");
        Console.WriteLine();

        #endregion
    }
}
catch (Exception e) 
{
    Console.WriteLine("The file could not be read:");
    Console.WriteLine(e.Message); 
}

#endregion

foreach(var device in VirtualDevices)
{
    await device.InitializeHendlers();
}
while (true)
{
    foreach (var device in VirtualDevices)
    {
        await device.UpdateTwinAsync();
        await device.SendMessages();
    }
    Thread.Sleep(Delay);
}

Console.WriteLine("Prase kay to continue...");
Console.ReadLine();

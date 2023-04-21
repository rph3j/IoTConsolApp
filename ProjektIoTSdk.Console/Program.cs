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

    int counter = ((int)jsonObject["CountOfDevices"]);
    for (int i = 1; i <= counter; i++)
    {
        #region Conection

        string Key = jsonObject["conectionStrings"][i-1].ToString();
        using var deviceClient = DeviceClient.CreateFromConnectionString(Key, TransportType.Mqtt);
        await deviceClient.OpenAsync();
        var device = new VirtualDevice(deviceClient);

        Console.Write("Connection succesful!");
        Console.WriteLine();

        #endregion
        await device.SendMessages(i);
    }

    Console.WriteLine("Prase kay to continue...");
    Console.ReadLine();
}
catch (Exception e) 
{
    Console.WriteLine("The file could not be read:");
    Console.WriteLine(e.Message); 
}


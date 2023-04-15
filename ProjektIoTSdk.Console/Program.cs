using ProjektIoTSdk.Device;
using Microsoft.Azure.Devices.Client;
using Opc.UaFx.Client;
using Opc.UaFx;

// tworze i uzywam połączeniea
string Key = "HostName=zajencia-IoT-23.azure-devices.net;DeviceId=Device01;SharedAccessKey=F5MFBFYHLsG8iAbKJUpe8iBg16JpW3DZvHMgEtj79jc=";
using var deviceClient = DeviceClient.CreateFromConnectionString(Key, TransportType.Mqtt);

// Odpalam deviceClient i tworze VirtualDevice
await deviceClient.OpenAsync();
var device = new VirtualDevice(deviceClient);
Console.Write("Connection succesful!");
Console.WriteLine();


await device.SendMessages(1);

    




Console.WriteLine("Prase kay to continue...");
Console.ReadLine();
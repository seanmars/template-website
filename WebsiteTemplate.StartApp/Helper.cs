using System.Net.NetworkInformation;

namespace WebsiteTemplate.StartApp;

public static class Helper
{
    public static void GetIpAndPorts()
    {
        var tcpListeners = IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpListeners();
        foreach (var tcpListener in tcpListeners)
        {
            Console.WriteLine($"{tcpListener.Address}:{tcpListener.Port}");
        }
    }

    public static bool IsPortInUse(int port)
    {
        var ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
        var tcpConnections = ipGlobalProperties.GetActiveTcpConnections();
        var tcpListeners = ipGlobalProperties.GetActiveTcpListeners();
        return tcpConnections.Any(x => x.LocalEndPoint.Port == port) ||
               tcpListeners.Any(x => x.Port == port);
    }
}
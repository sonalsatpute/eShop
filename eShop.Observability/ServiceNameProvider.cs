using Microsoft.Extensions.Configuration;

namespace eShop.Observability;

public interface IServiceInfoProvider
{
    string GetServiceName(IConfiguration settings);
    string GetHostName();
}

public class ServiceInfoProvider : IServiceInfoProvider
{
    const string SERVICE_NAME = "SERVICE_NAME";
    const string DELIMITER = "-";

    public string GetServiceName(IConfiguration settings)
    {
        string? value = settings.GetValue(SERVICE_NAME, string.Empty);
        if (!string.IsNullOrWhiteSpace(value)) return value;

        // Machine Name Format: <service-name>-<replica set id>-<pod id>
        // Example: my-service-7d9b7b4b4f-4z5z2
        // Skip last 2 elements (<replica set id>-<pod id>) to get service name
        // Result: my-service
        
        string[] hostname = GetHostName().Split(DELIMITER, StringSplitOptions.RemoveEmptyEntries);
        value = string.Join(DELIMITER, hostname.SkipLast(2));

        return string.IsNullOrWhiteSpace(value)
            ? "missing-service-name" // <-- ❓Question: Should throw exception here?
            : value;
    }

    public string GetHostName()
    {
        try
        {
            return Environment.MachineName;
        }
        catch (Exception e)
        {
            /*
             The System.Environment.MachineName property in .NET retrieves the NetBIOS name of the local computer. 
             However, there are a few scenarios where this might fail:
             
              - InvalidOperationException: This exception is thrown if the machine name cannot be obtained. This can happen if the system is unable to read the machine name from the registry during startup1.
              - Cluster Nodes: If the computer is part of a cluster, the name returned might be the node name rather than the actual machine name1.
              - Permissions Issues: If the application does not have the necessary permissions to access system information, it might fail to retrieve the machine name1.
             */
            
            Console.WriteLine(e);
        }
        
        return "unknown-host";
    }
}
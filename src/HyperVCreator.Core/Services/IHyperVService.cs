using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HyperVCreator.Core.Models;

namespace HyperVCreator.Core.Services
{
    public interface IHyperVService
    {
        Task<List<string>> GetVirtualSwitchesAsync();
        Task<bool> CreateVirtualMachineAsync(object vmConfiguration, IProgress<int> progress = null);
        Task<List<string>> GetVirtualMachinesAsync();
        Task<bool> StartVirtualMachineAsync(string vmName);
        Task<bool> StopVirtualMachineAsync(string vmName);
        Task<bool> CreateVirtualMachine(string vmName, int cpuCount, int memoryGB, int storageGB, string virtualSwitch);
        Task<bool> ConfigureNetwork(string vmName, bool dynamicIP, string ipAddress, string subnetMask, string defaultGateway, string dnsServer);
        Task<bool> AddVirtualDisk(string vmName, int sizeGB, string diskLabel);
        Task<bool> MountISOFile(string vmName, string isoPath);
        Task<bool> StartVM(string vmName);
        Task<bool> StopVM(string vmName);
        Task<List<string>> GetAvailableVirtualSwitches();
        Task<bool> CheckHyperVEnabled();
    }
} 
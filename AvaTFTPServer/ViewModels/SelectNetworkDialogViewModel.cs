using AvaTFTPServer.AvaloniaTools;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Tmds.DBus.Protocol;
using static AvaTFTPServer.ViewModels.ConfigDialogViewModel;

namespace AvaTFTPServer.ViewModels;

public partial class EndPointSelectionDialogViewModel : ObservableValidator
{
    public record class NetworkInterfaceItem
    {
        public string UserText { get; init; } = "woot";
        public IPEndPoint IPEndPoint { get; init; } = new IPEndPoint(IPAddress.Any, 0);

        public string Family => AddressFamilyPresentation(IPEndPoint.Address.AddressFamily);

        private static string AddressFamilyPresentation(AddressFamily addressFamily)
        {
            switch(addressFamily)
            {
                case AddressFamily.InterNetwork:
                    return "ipv4";
                case AddressFamily.InterNetworkV6:
                    return "ipv6";
                default:
                    return addressFamily.ToString();
            }
        }
    }

    public List<NetworkInterfaceItem> EndPoints { get; private set; } = [
        new()
    ];

    [ObservableProperty]
    private int _selectedIPEndPointIndex = 0;
    private readonly IViewModelCloser _viewModelCloser;

    partial void OnSelectedIPEndPointIndexChanged(int value)
    {
        //CleanupTransfersAfter = TimeSpans[value].TimeSpan;
    }

    [RelayCommand]
    private void Apply()
    {
        _viewModelCloser.Close(this, DialogResult.Ok);
    }

    [RelayCommand]
    private void Cancel()
    {
        _viewModelCloser.Close(this, DialogResult.Cancel);
    }

    public EndPointSelectionDialogViewModel(IViewModelCloser viewModelCloser)
    {
        _viewModelCloser = viewModelCloser;

        var nics = NetworkInterface.GetAllNetworkInterfaces();

        var networks = nics.Select(x => x.GetIPProperties().UnicastAddresses
            .Select(y => new {
                nic = x,
                uni = y
            }))
            .SelectMany(x => x)
            .Select(x => new NetworkInterfaceItem
            {
                UserText = x.nic.Name,
                IPEndPoint = new IPEndPoint(x.uni.Address,69)
            })
            .ToList();

        networks.Insert(0, new NetworkInterfaceItem { UserText = "Any", IPEndPoint = new IPEndPoint(IPAddress.Any,69) });
        networks.Insert(1, new NetworkInterfaceItem { UserText = "Any", IPEndPoint = new IPEndPoint(IPAddress.IPv6Any,69) });
        networks.Insert(2, new NetworkInterfaceItem { UserText = "Test", IPEndPoint = new IPEndPoint(IPAddress.Parse("[DEAD:BEEF:DEAD:BEEF:DEAD:BEEF:DEAD:BEEF]"), 69) });

        networks = networks.OrderBy(x => x.IPEndPoint.Address.AddressFamily).ToList();

        this.EndPoints = networks;
    }
}

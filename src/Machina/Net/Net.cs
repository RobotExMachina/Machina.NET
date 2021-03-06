﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace Machina.Net
{
    public static class Net
    {
        /// <summary>
        /// Returns true if input it is a valid IPv4 address.
        /// https://stackoverflow.com/a/11412991/1934487
        /// </summary>
        /// <param name="ipString"></param>
        /// <returns></returns>
        public static bool ValidateIPv4(string ipString)
        {
            if (String.IsNullOrWhiteSpace(ipString))
            {
                return false;
            }

            string[] splitValues = ipString.Split('.');
            if (splitValues.Length != 4)
            {
                return false;
            }

            byte tempForParsing;

            return splitValues.All(r => byte.TryParse(r, out tempForParsing));
        }

        /// <summary>
        /// Returns true if input it is a valid IPv4 address + port, like "127.0.0.1:7000"
        /// </summary>
        /// <param name="ipString"></param>
        /// <returns></returns>
        public static bool ValidateIPv4Port(string ipString)
        {
            string[] parts = ipString.Split(':');
            if (parts.Length != 2)
            {
                return false;
            }

            int port;
            if (!int.TryParse(parts[1], out port) || port < 0 || port > 65535)
            {
                return false;
            }

            return ValidateIPv4(parts[0]);
        }

        /// <summary>
        /// Given a remote IP address and a subnet mask, tries to find the local IP address of this host in the same subnet.
        /// This is useful to figure out which IP this host is using in the same network as the remote. 
        /// Inspired by https://stackoverflow.com/a/6803109/1934487
        /// </summary>
        /// <param name="remoteIP">The remote IP of the device we are trying to find the local network for.</param>
        /// <param name="subnetMask">Typically "255.255.255.0", filters how many hosts are accepted in the subnet. https://www.iplocation.net/subnet-mask </param>
        /// <param name="localIP">The found localIP</param>
        /// <returns></returns>
        public static bool GetLocalIPAddressInNetwork(string remoteIP, string subnetMask, out string localIP)
        {
            IPAddress _remote = IPAddress.Parse(remoteIP);
            IPAddress _subnetMask = IPAddress.Parse(subnetMask);

            var host = Dns.GetHostEntry(Dns.GetHostName());

            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork && IsInSameSubnet(_remote,ip, _subnetMask))
                {
                    localIP = ip.ToString();
                    return true;
                }
            }

            localIP = "";
            return false;
        }

        // https://blogs.msdn.microsoft.com/knom/2008/12/31/ip-address-calculations-with-c-subnetmasks-networks/
        private static IPAddress GetBroadcastAddress(this IPAddress address, IPAddress subnetMask)
        {
            byte[] ipAdressBytes = address.GetAddressBytes();
            byte[] subnetMaskBytes = subnetMask.GetAddressBytes();

            if (ipAdressBytes.Length != subnetMaskBytes.Length)
                throw new ArgumentException("Lengths of IP address and subnet mask do not match.");

            byte[] broadcastAddress = new byte[ipAdressBytes.Length];
            for (int i = 0; i < broadcastAddress.Length; i++)
            {
                broadcastAddress[i] = (byte)(ipAdressBytes[i] | (subnetMaskBytes[i] ^ 255));
            }
            return new IPAddress(broadcastAddress);
        }

        private static IPAddress GetNetworkAddress(this IPAddress address, IPAddress subnetMask)
        {
            byte[] ipAdressBytes = address.GetAddressBytes();
            byte[] subnetMaskBytes = subnetMask.GetAddressBytes();

            if (ipAdressBytes.Length != subnetMaskBytes.Length)
                throw new ArgumentException("Lengths of IP address and subnet mask do not match.");

            byte[] broadcastAddress = new byte[ipAdressBytes.Length];
            for (int i = 0; i < broadcastAddress.Length; i++)
            {
                broadcastAddress[i] = (byte)(ipAdressBytes[i] & (subnetMaskBytes[i]));
            }
            return new IPAddress(broadcastAddress);
        }

        private static bool IsInSameSubnet(this IPAddress address2, IPAddress address, IPAddress subnetMask)
        {
            IPAddress network1 = address.GetNetworkAddress(subnetMask);
            IPAddress network2 = address2.GetNetworkAddress(subnetMask);

            return network1.Equals(network2);
        }

        
    }
}

using ConfigurationTool.Models;
using ConfigurationTool.MVVM;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Management;

namespace ConfigurationTool.ViewModels
{
    internal class MainWindowViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<IPConfigurationInformation> ConfigurationInformations
        { get; set; }

        /// <summary>
        /// RelayCommand to update the adapters and related information.
        /// </summary>
        public RelayCommand UpdateCommand => new RelayCommand(execute => RefreshConfigurationInformation(), canExecute => { return true; });

        const string SELECTALL = "Select all adapters";

        /// <summary>
        /// Constructor of the view model.
        /// </summary>
        public MainWindowViewModel()
        {
            ConfigurationInformations = new ObservableCollection<IPConfigurationInformation>();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertychanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Backing field of property AdapterNames.
        /// </summary>
        private IList<string> adapterNames;

        /// <summary>
        /// AdapterNames which will be displayed in the combobox for the user to select.
        /// </summary>
        public IList<string> AdapterNames
        {
            get
            {
                adapterNames = GetAdapterNames();
                return adapterNames;
            }
            set
            {
                if (adapterNames != value)
                {
                    value = adapterNames;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs($"{nameof(AdapterNames)}"));

                    }
                }
            }
        }

        /// <summary>
        /// Backing field of property SelectedAdapter.
        /// </summary>
        private string selectedAdapter;
        /// <summary>
        /// This property indicates the name of the selected adapter.
        /// </summary>
        public string SelectedAdapter
        {
            get { return selectedAdapter; }
            set
            {
                if (selectedAdapter != value)
                {
                    selectedAdapter = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs($"{nameof(SelectedAdapter)}"));
                        GetNetWorkInterfaceInformation(selectedAdapter);
                    }
                }
            }
        }

        /// <summary>
        /// This method updates the displayed adapter names and also the displayed information of the selected adapter.
        /// </summary>
        public void RefreshConfigurationInformation()
        {
            AdapterNames = GetAdapterNames();
            GetNetWorkInterfaceInformation(SelectedAdapter);
        }

        /// <summary>
        /// Gets related information of a selected adapter.
        /// </summary>
        /// <param name="selectedAdapter"></param>
        /// <exception cref="ApplicationException"></exception>
        private void GetNetWorkInterfaceInformation(string selectedAdapter)
        {
            ConfigurationInformations.Clear();

            try
            {
                if (selectedAdapter == SELECTALL)
                {
                    string query = $"SELECT * FROM Win32_NetworkAdapter WHERE NetConnectionID != NULL";
                    GetConfigurationInformation(query);
                }
                else
                {
                    string query = $"SELECT * FROM Win32_NetworkAdapter WHERE Name = '{selectedAdapter}'";
                    GetConfigurationInformation(query);
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Something went wrong during retrieving information about ip configuration.", ex.InnerException);
            }
        }

        /// <summary>
        /// Gets Configuration Information.
        /// </summary>
        /// <param name="query">Query to execute to search.</param>
        private void GetConfigurationInformation(string query)
        {
            ManagementObjectCollection adapters = CreateManagementCollection(query);
            foreach (ManagementObject adapter in adapters)
            {
                IPConfigurationInformation configuration = new IPConfigurationInformation();

                // Get Adapter Name.
                GetAdapterName(adapter, configuration);

                // Get Adapter Type.
                GetAdapterType(adapter, configuration);

                // Get Adapter Status
                GetAdapterStatus(adapter, configuration);

                //Get related information.
                uint index = (uint)adapter["Index"];
                string configQuery = $"SELECT * FROM Win32_NetworkAdapterConfiguration WHERE Index = {index}";
                ManagementObjectCollection configCollection = CreateManagementCollection(configQuery);
                foreach (ManagementObject config in configCollection)
                {
                    //Get MAC-Address.
                    GetMACAddress(config, configuration);

                    // Get the IP addresses
                    GetIPAddress(config, configuration);

                    // Get the IP addresses
                    GetSubnet(config, configuration);

                    // Get DNS Suffix.
                    GetDNSDomain(config, configuration);

                    ConfigurationInformations.Add(configuration);
                }
            }
        }

        /// <summary>
        /// Gets the name of the Adapter.
        /// </summary>
        /// <param name="adapter"></param>
        /// <param name="configuration"></param>
        private void GetAdapterName(ManagementObject adapter, IPConfigurationInformation configuration)
        { // Get Adapter Name.
            try
            {
                configuration.AdapterName = adapter["Name"]?.ToString();
            }
            catch (Exception ex) 
            {
                throw new ApplicationException("Something went wrong during retrieving adapter name.", ex.InnerException);
            }
        }

        /// <summary>
        /// Gets the type of the Adapter.
        /// </summary>
        /// <param name="adapter"></param>
        /// <param name="configuration"></param>
        private void GetAdapterType(ManagementObject adapter, IPConfigurationInformation configuration)
        { 
            try
            {
                configuration.AdapterType = (adapter["AdapterType"] != null) ? adapter["AdapterType"].ToString() : "N/A";
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Something went wrong during retrieving adapter type.", ex.InnerException);
            }
        }

        /// <summary>
        /// Gets the type of the Adapter.
        /// </summary>
        /// <param name="adapter"></param>
        /// <param name="configuration"></param>
        private void GetAdapterStatus(ManagementObject adapter, IPConfigurationInformation configuration)
        { 
            try
            {
                bool netEnabled = (bool)adapter["NetEnabled"];
                var netconnectionStatus = adapter["NetConnectionStatus"];
                string status = GetNetConnectionStatus(Convert.ToUInt32(netconnectionStatus));
                configuration.Status = GetAdapterStatus(netEnabled, status);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Something went wrong during retrieving adapter status.", ex.InnerException);
            }
        }

        /// <summary>
        /// Gets MAC-Address of the adapter.
        /// </summary>
        /// <param name="adapter"></param>
        /// <param name="configuration"></param>
        private void GetMACAddress(ManagementObject adapter, IPConfigurationInformation configuration)
        {
            try
            {
                configuration.MACAddress = (adapter["MACAddress"] !=null) ? adapter["MACAddress"].ToString() :"N/A";
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Something went wrong during retrieving MAC-Address.", ex.InnerException);
            }
        }

        /// <summary>
        /// Gets the IP Address of the adapter.
        /// </summary>
        /// <param name="adapter"></param>
        /// <param name="configuration"></param>
        private void GetIPAddress(ManagementObject adapter, IPConfigurationInformation configuration)
        {
            try
            {
                string[] ipAddresses = (string[])adapter["IPAddress"];              
                configuration.IPAddress = (ipAddresses !=null) ? ipAddresses.FirstOrDefault() : "N/A";              
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Something went wrong during retrieving IP Address.", ex.InnerException);
            }
        }

        /// <summary>
        /// Gets the DNS Domain of the Adapter.
        /// </summary>
        /// <param name="adapter"></param>
        /// <param name="configuration"></param>
        private void GetDNSDomain(ManagementObject adapter, IPConfigurationInformation configuration)
        {
            try
            {
                configuration.DNSDomain = (adapter["DNSDomain"] != null) ? adapter["DNSDomain"].ToString() : "N/A";
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Something went wrong during retrieving DNS Domain.", ex.InnerException);
            }
        }

        /// <summary>
        /// Gets the subnet of the adapter.
        /// </summary>
        /// <param name="adapter"></param>
        /// <param name="configuration"></param>
        /// <exception cref="ApplicationException"></exception>
        private void GetSubnet(ManagementObject adapter, IPConfigurationInformation configuration)
        {
            try
            {
                string[] ipsubnet = (string[])adapter["IPSubnet"];
               
                configuration.Subnetmask = (ipsubnet != null) ? ipsubnet.FirstOrDefault() : "N/A";
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Something went wrong during retrieving Subnet Mask.", ex.InnerException);
            }
        }

        /// <summary>
        /// Creates ManagementCollection based on query.
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        private ManagementObjectCollection CreateManagementCollection(string query)
        {
            SelectQuery wmiQuery = new SelectQuery(query);
            ManagementObjectSearcher searchProcedure = new ManagementObjectSearcher(wmiQuery);
            ManagementObjectCollection adapters = searchProcedure.Get();
            return adapters;
        }

        /// <summary>
        /// Map the NetConnectionStatus values to human-readable descriptions
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        static string GetNetConnectionStatus(uint status)
        {         
            switch (status)
            {
                case 0: return "Disconnected";
                case 1: return "Connecting";
                case 2: return "Connected";
                case 3: return "Disconnecting";
                case 4: return "Hardware not present";
                case 5: return "Hardware disabled";
                case 6: return "Hardware malfunction";
                case 7: return "Media disconnected";
                case 8: return "Authenticating";
                case 9: return "Invalid address";
                case 10: return "Credentials required";
                default: return "Unknown";
            }
        }

        /// <summary>
        /// Gets Adapter Status.
        /// </summary>
        /// <param name="enabled"></param>
        /// <param name="connectionStatus"></param>
        /// <returns></returns>
        static Status GetAdapterStatus(bool enabled, string connectionStatus)
        {
            if (!enabled) return Status.Disabled;
            else if (connectionStatus == "Connected") return Status.Connected;
            else if (connectionStatus == "Disconnected") return Status.Disconnected;           
            return Status.Disconnected;
        }

        /// <summary>
        /// Gets Adapter Names.
        /// </summary>
        /// <returns></returns>
        private List<string> GetAdapterNames()
        {
            List<string> names = new List<string>();
            string query = "SELECT * FROM Win32_NetworkAdapter WHERE NetConnectionID != NULL";

            var adapters = CreateManagementCollection(query);
            foreach (ManagementObject adapter in adapters)
            {
                names.Add(adapter["Name"].ToString());
            }
            names.Add(SELECTALL);
            return names;
        }    
    }
}

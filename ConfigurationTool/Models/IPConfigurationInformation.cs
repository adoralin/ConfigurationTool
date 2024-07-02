namespace ConfigurationTool.Models
{
    public class IPConfigurationInformation
    {
        /// <summary>
        /// Adapter name.
        /// </summary>
        public string AdapterName { get; set; }
        /// <summary>
        /// IP address
        /// </summary>
        public string IPAddress { get; set; }
        /// <summary>
        /// Status : Disabled, Disconnected, connected.
        /// </summary>
        public Status Status { get; set; }
        /// <summary>
        /// Network Adapter Type
        /// </summary>
        public string AdapterType { get; set; }
        /// <summary>
        /// MAC-Address.
        /// </summary>
        public string MACAddress { get; set; }
        /// <summary>
        /// DNS Domain.
        /// </summary>
        public string DNSDomain { get; set; }

        /// <summary>
        /// Subnetmask
        /// </summary>
        public string Subnetmask { get; set; }
    }
}

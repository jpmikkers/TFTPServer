using CodePlex.JPMikkers.TFTP;
using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Xml.Serialization;

namespace TFTPServerApp
{
    [Serializable()]
    public class TFTPServerConfiguration
    {
        public string Name { get; set; }
        public SerializableIPEndPoint EndPoint { get; set; }
        public bool SinglePort { get; set; }
        public int Ttl { get; set; }
        public bool DontFragment { get; set; }
        public string RootPath { get; set; }
        public bool AutoCreateDirectories { get; set; }
        public bool AllowRead { get; set; }
        public bool AllowWrite { get; set; }
        public int Timeout { get; set; }
        public int Retries { get; set; }
        public bool ConvertPathSeparator { get; set; }
        public ushort WindowSize { get; set; }
        public BindingList<ConfigurationAlternative> Alternatives { get; set; }

        public TFTPServerConfiguration()
        {
            Name = "TFTP";
            EndPoint = new IPEndPoint(IPAddress.Loopback, 69);
            SinglePort = false;
            Ttl = -1;
            DontFragment = false;
            RootPath = ".";
            AutoCreateDirectories = true;
            AllowRead = true;
            AllowWrite = true;
            Timeout = 2000;
            Retries = 5;
            ConvertPathSeparator = true;
            WindowSize = 1;
            Alternatives = new BindingList<ConfigurationAlternative>();
        }

        public TFTPServerConfiguration Clone()
        {
            TFTPServerConfiguration result = new TFTPServerConfiguration();
            result.Name = Name;
            result.EndPoint = EndPoint.Clone();
            result.SinglePort = SinglePort;
            result.Ttl = Ttl;
            result.DontFragment = DontFragment;
            result.RootPath = RootPath;
            result.AutoCreateDirectories = AutoCreateDirectories;
            result.AllowRead = AllowRead;
            result.AllowWrite = AllowWrite;
            result.Timeout = Timeout;
            result.Retries = Retries;
            result.ConvertPathSeparator = ConvertPathSeparator;
            result.WindowSize = WindowSize;

            if(Alternatives != null)
            {
                result.Alternatives = new BindingList<ConfigurationAlternative>();
                foreach(ConfigurationAlternative alternative in Alternatives)
                {
                    result.Alternatives.Add(alternative.Clone());
                }
            }

            return result;
        }
    }

    [Serializable()]
    public class ConfigurationAlternative
    {
        public string Filter { get; set; }
        public bool IsRegularExpression { get; set; }
        public ushort WindowSize { get; set; }

        public ConfigurationAlternative()
        {
            Filter = "*";
            WindowSize = 1;
        }

        public ConfigurationAlternative Clone()
        {
            ConfigurationAlternative result = new ConfigurationAlternative();
            result.Filter = Filter;
            result.IsRegularExpression = IsRegularExpression;
            result.WindowSize = WindowSize;
            return result;
        }

        public TFTPServer.ConfigurationAlternative Convert()
        {
            TFTPServer.ConfigurationAlternative result;
            if(IsRegularExpression)
            {
                result = TFTPServer.ConfigurationAlternative.CreateRegex(Filter);
            }
            else
            {
                result = TFTPServer.ConfigurationAlternative.CreateWildcard(Filter);
            }
            result.WindowSize = WindowSize;
            return result;
        }
    }

    [Serializable()]
    public class SerializableIPEndPoint
    {
        private IPEndPoint _endPoint;

        public string Address
        {
            get
            {
                return _endPoint.Address.ToString();
            }
            set
            {
                _endPoint = new IPEndPoint(IPAddress.Parse(value), _endPoint.Port);
            }
        }

        public int Port
        {
            get
            {
                return _endPoint.Port;
            }
            set
            {
                _endPoint = new IPEndPoint(_endPoint.Address, value);
            }
        }

        public SerializableIPEndPoint()
        {
            _endPoint = new IPEndPoint(IPAddress.Loopback, 0);
        }

        public SerializableIPEndPoint(IPEndPoint p)
        {
            _endPoint = (IPEndPoint)p.Create(p.Serialize());
        }

        static public implicit operator SerializableIPEndPoint(IPEndPoint c)
        {
            return new SerializableIPEndPoint(c);
        }

        static public implicit operator IPEndPoint(SerializableIPEndPoint c)
        {
            return (IPEndPoint)c._endPoint.Create(c._endPoint.Serialize());
        }

        public SerializableIPEndPoint Clone()
        {
            return new SerializableIPEndPoint(_endPoint);
        }
    }

    [Serializable()]
    public class TFTPServerConfigurationList : BindingList<TFTPServerConfiguration>
    {
        private static readonly XmlSerializer serializer = new XmlSerializer(typeof(TFTPServerConfigurationList));

        public static TFTPServerConfigurationList Read(string file)
        {
            TFTPServerConfigurationList result;

            if(File.Exists(file))
            {
                using(Stream s = File.OpenRead(file))
                {
                    result = (TFTPServerConfigurationList)serializer.Deserialize(s);
                }
            }
            else
            {
                result = new TFTPServerConfigurationList();
            }

            return result;
        }

        public void Write(string file)
        {
            using(Stream s = File.Open(file, FileMode.Create))
            {
                serializer.Serialize(s, this);
            }
        }
    }
}
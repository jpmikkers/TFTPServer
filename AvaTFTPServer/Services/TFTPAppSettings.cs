using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AvaTFTPServer;

public class TFTPAppSettings
{
    private static string _configFolderPath;
    private static string _configPath;

    public string ConfigPath
    {
        get => _configPath;
    }

    private static JsonSerializerOptions s_serializerOptions = new()
    {
        WriteIndented = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        Converters = { new IPEndPointJsonConverter() }
    };

    public UISettings UISettings { get; set; } = new UISettings();

    public ServerSettings ServerSettings { get; set; } = new ServerSettings();

    static TFTPAppSettings()
    {
        _configFolderPath = Path.Combine( Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AvaTFTPServer");
        _configPath = Path.Combine(_configFolderPath, "settings.json");
    }

    public static TFTPAppSettings Load()
    {
        try
        {
            if(File.Exists(_configPath))
            {
                using var stream = File.OpenRead(_configPath);
                return JsonSerializer.Deserialize<TFTPAppSettings>(stream, s_serializerOptions) ?? new();
            }
        }
        catch 
        { 
        }
        return new();
    }

    public void Save()
    {
        try
        {
            if(!Directory.Exists(_configFolderPath))
            {
                Directory.CreateDirectory(_configFolderPath);
            }

            using var stream = File.Create(_configPath);
            JsonSerializer.Serialize(stream, this, s_serializerOptions);
        }
        catch(Exception ex) 
        { 
            System.Diagnostics.Debug.WriteLine(ex.Message);
        }
    }
}

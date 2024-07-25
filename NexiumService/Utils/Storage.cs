using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace NexiumService.Utils;

public enum StorageType
{
    
}

public class Storage
{
    public static readonly string ConfigDir = System.AppDomain.CurrentDomain.BaseDirectory;
    
    private readonly string _path;
    
    public Storage(string fileName, bool create = false)
    {
        string[] directories =
        {
            "cache",
            "cache/banners",
            "cache/games"
        };
        
        foreach (var directory in directories)
        {
            if (!Directory.Exists(ConfigDir + "/" + directory))
            {
                Directory.CreateDirectory(ConfigDir + "/" + directory);
            }
        }
        
        _path = ConfigDir + "cache/" + fileName;
        if (fileName.Length > 3)
        {
            string disk = fileName.Substring(0, 3);
            bool isDisk = Regex.IsMatch(disk, @"[A-Z]:\\");
            if (isDisk)
            {
                _path = fileName;
            }
        }
    }

    public void Write(JsonObject data)
    {
        File.WriteAllText(_path, data.ToString());
    }
    
    public JsonObject Read()
    {
        if (!File.Exists(_path))
        {
            return new JsonObject();
        }
        
        string data = File.ReadAllText(_path);
        if (string.IsNullOrEmpty(data))
        {
            return new JsonObject();
        }
        
        return JsonNode.Parse(data).AsObject();
    }
    
    public bool Exists()
    {
        return File.Exists(_path);
    }
    
    public void Delete()
    {
        File.Delete(_path);
    }
    
    public bool Download(string url)
    {
        using (var client = new HttpClient())
        {
            var response = client.GetAsync(url).Result;
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return false;
            }
            
            var content = response.Content.ReadAsByteArrayAsync().Result;
            File.WriteAllBytes(_path, content);
        }
        
        return true;
    }
    
    public string GetPath()
    {
        return _path.Replace("/", @"\");
    }
    
}
using Cysharp.Threading.Tasks;
using OMD.Zones.Data;
using OMD.Zones.Models.Zones;
using OpenMod.Core.Persistence;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace OMD.Zones.Persistence;

internal sealed class ZonesDataStore
{
    internal IReadOnlyList<Zone> Zones => _instance.Zones.AsReadOnly();
    internal ZonesStore Instance => _instance;


    private ZonesStore _instance = new();

    private readonly ISerializer _serializer;

    private readonly IDeserializer _deserializer;

    private readonly string _filePath;

    private int _writeCounter = 1;

    private readonly object _lock = new();

    internal ZonesDataStore(string workingDirectory)
    {
        var typeConverters = new IYamlTypeConverter[] {
            new YamlVector3TypeConverter()
        };
        var zoneTypes = FindZoneTypes();

        var serializerBuilder = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .DisableAliases();
        var deserializerBuilder = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .IgnoreUnmatchedProperties();

        foreach (var typeConverter in typeConverters)
        {
            serializerBuilder.WithTypeConverter(typeConverter);
            deserializerBuilder.WithTypeConverter(typeConverter);
        }

        foreach (var zoneType in zoneTypes)
        {
            var tag = $"tag:yaml.org,2002:{zoneType.Name}";

            serializerBuilder.WithTagMapping(tag, zoneType);
            deserializerBuilder.WithTagMapping(tag, zoneType);
        }

        _filePath = Path.Combine(workingDirectory, "zones.data.yaml");
        _serializer = serializerBuilder.Build();
        _deserializer = deserializerBuilder.Build();
    }

    internal async Task Load()
    {
        if (!File.Exists(_filePath))
            return;

        var encodedData = await InternalRetry.DoAsync(() => File.ReadAllBytes((_filePath)), TimeSpan.FromMilliseconds(1), 5);
        var serializedYaml = Encoding.UTF8.GetString(encodedData);

        _instance = _deserializer.Deserialize<ZonesStore>(serializedYaml); ;
    }

    internal Task Save()
    {
        var serializedYaml = _serializer.Serialize(_instance);
        var encodedData = Encoding.UTF8.GetBytes(serializedYaml);

        lock (_lock)
        {
            _writeCounter += 1;

            try
            {
                File.WriteAllBytes(_filePath, encodedData);
            }
            catch
            {
                DecrementWriteCounter();
                throw;
            }
        }

        return Task.CompletedTask;
    }

    internal Task<bool> Add<TZone>(TZone zone)
        where TZone : Zone
    {
        async UniTask<bool> InnerTask()
        {
            if (zone is null)
                throw new ArgumentNullException(nameof(zone));

            if (_instance!.Zones!.Exists(z => z.Name.Equals(zone.Name, StringComparison.OrdinalIgnoreCase)))
                return false;

            _instance.Zones.Add(zone);

            await Save();

            await UniTask.SwitchToMainThread();

            zone.Init();

            return true;
        }

        return InnerTask().AsTask();
    }

    internal Task<bool> Remove<TZone>(TZone zone)
        where TZone : Zone
    {
        async UniTask<bool> InnerTask()
        {
            if (zone is null)
                throw new ArgumentNullException(nameof(zone));

            if (!_instance.Zones.Remove(zone))
                return false;

            await Save();

            await UniTask.SwitchToMainThread();

            zone.Destroy();

            return true;
        }

        return InnerTask().AsTask();
    }

    internal Task<bool> RemoveByName(string name)
    {
        async UniTask<bool> InnerTask()
        {
            if (string.IsNullOrEmpty(name))
                throw new InvalidOperationException("Name of target zone cannot be null or empty!");

            var index = _instance.Zones.FindIndex(z => z.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

            if (index == -1)
                return false;

            var zone = _instance.Zones[index];

            _instance.Zones.RemoveAt(index);

            await Save();

            await UniTask.SwitchToMainThread();

            zone.Destroy();

            return true;
        }

        return InnerTask().AsTask();
    }

    private IEnumerable<Type> FindZoneTypes()
    {
        var targetType = typeof(Zone);
        var targetTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly => {
            try { return assembly.GetTypes(); }
            catch (Exception) { return []; }
        }).Where(type => {
            try { return !type.IsAbstract && targetType.IsAssignableFrom(type); }
            catch (Exception) { return false; }
        });

        return targetTypes;
    }

    private void DecrementWriteCounter()
    {
        if (_writeCounter == 0)
            return;

        if (_writeCounter < 0)
            throw new InvalidOperationException("DecrementWriteCounter has become negative");

        _writeCounter -= 1;
    }
}

internal static class InternalRetry
{
    public static async Task<T> DoAsync<T>(Func<T> action, TimeSpan retryInterval, int maxAttempts)
    {
        var exceptions = new List<Exception>();

        for (var attempted = 0; attempted < maxAttempts; attempted++)
        {
            try
            {
                if (attempted > 0)
                {
                    await Task.Delay(retryInterval);
                }

                return action();
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }
        }

        throw new AggregateException(exceptions);
    }
}
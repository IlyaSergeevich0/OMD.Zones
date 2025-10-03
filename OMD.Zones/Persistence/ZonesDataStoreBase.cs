using Cysharp.Threading.Tasks;
using OMD.Zones.Data;
using OMD.Zones.Models.Zones;
using OMD.Zones.Persistence.Converters;
using OMD.Zones.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace OMD.Zones.Persistence;

/// <summary>
/// Implements base functionality for saving zones.
/// </summary>
/// <remarks>
/// Before v0.8.1 it was core data store of all zones.<br/>
/// But after v1.0.0 I decided to left it as base zones data store class for depended plugins.
/// </remarks>
public abstract class ZonesDataStoreBase<TContent>
    where TContent : class
{
    public TContent Content { get; protected set; } = null!;

    private readonly ISerializer _serializer;
    private readonly IDeserializer _deserializer;
    private readonly string _saveFilePath;

    private int _writeCounter = 1;

    private readonly object _lock = new();

    protected ZonesDataStoreBase(string saveFilePath)
    {
        _saveFilePath = saveFilePath;
        _serializer = BuildSerializer();
        _deserializer = BuildDeserializer();
    }

    protected virtual ISerializer BuildSerializer()
    {
        var typeConverters = new IYamlTypeConverter[] {
            new YamlSystemVector3TypeConverter(),
            new YamlUnityVector3TypeConverter(),
            new YamlQuaternionTypeConverter()
        };

        var serializerBuilder = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .IgnoreFields()
            .DisableAliases();

        foreach (var typeConverter in typeConverters)
        {
            serializerBuilder.WithTypeConverter(typeConverter);            
        }

        return serializerBuilder.Build();
    }

    protected virtual IDeserializer BuildDeserializer()
    {
        var typeConverters = new IYamlTypeConverter[] {
            new YamlSystemVector3TypeConverter(),
            new YamlUnityVector3TypeConverter(),
            new YamlQuaternionTypeConverter()
        };

        var deserializerBuilder = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .IgnoreFields()
            .IgnoreUnmatchedProperties();

        foreach (var typeConverter in typeConverters)
        {
            deserializerBuilder.WithTypeConverter(typeConverter);
        }

        return deserializerBuilder.Build();
    }

    public virtual async Task LoadAsync()
    {
        if (!File.Exists(_saveFilePath))
            return;

        var encodedData = await RetryUtility.DoAsync(() => File.ReadAllBytes(_saveFilePath), TimeSpan.FromMilliseconds(1), 5);
        var serializedYaml = Encoding.UTF8.GetString(encodedData);

        Content = _deserializer.Deserialize<TContent>(serializedYaml);
    }

    public virtual Task SaveAsync()
    {
        lock (_lock)
        {
            _writeCounter += 1;

            try
            {
                var serializedYaml = _serializer.Serialize(Content);
                var encodedData = Encoding.UTF8.GetBytes(serializedYaml);

                File.WriteAllBytes(_saveFilePath, encodedData);
            }
            catch
            {
                DecrementWriteCounter();
                throw;
            }
        }

        return Task.CompletedTask;
    }

    protected void DecrementWriteCounter()
    {
        if (_writeCounter == 0)
            return;

        if (_writeCounter < 0)
            throw new InvalidOperationException("DecrementWriteCounter has become negative");

        _writeCounter -= 1;
    }
}

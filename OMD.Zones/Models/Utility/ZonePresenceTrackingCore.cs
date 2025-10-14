using Cysharp.Threading.Tasks;
using OMD.Zones.Data;
using OMD.Zones.Models.Grids;
using OMD.Zones.Models.Zones;
using OpenMod.Extensions.Games.Abstractions.Entities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace OMD.Zones.Models.Utility;

internal sealed class ZonePresenceTrackingCore(ZonesConfiguration zonesConfiguration) : IDisposable
{
    public event Action<IGameObject, Zone>? GameObjectEnteredZone;
    public event Action<IGameObject, Zone>? GameObjectExitedZone;

    private readonly int _regionRange = zonesConfiguration.Grid.RegionRange;
    private readonly TimeSpan _updateDelay = TimeSpan.FromMilliseconds(zonesConfiguration.Delays.TickMilliseconds);
    private readonly SpatialGrid2D<Zone> _grid = new(zonesConfiguration.Grid.CellSize);
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly ConcurrentDictionary<IGameObject, HashSet<Zone>> _gameObjectPreviousZones = [];
    private readonly HashSet<Zone> _zonesInRegion = [];
    private readonly HashSet<Zone> _currentZonesBuffer = [];

    public void Run()
    {
        TrackingLoopAsync(_cancellationTokenSource.Token).Forget();
    }

    public void Dispose()
    {
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();
    }

    public void AddZoneToGrid(Zone zone)
    {
        _grid.Add(zone.Center, zone);
    }

    public bool RemoveZoneFromGrid(Zone zone)
    {
        return _grid.Remove(zone.Center, zone);
    }

    public bool RemoveZoneFromGridRoughly(Zone zone)
    {
        return _grid.RemoveRoughly(zone);
    }

    public void StartTracking(IGameObject gameObject)
    {
        _gameObjectPreviousZones.TryAdd(gameObject, []);
    }

    public void StopTracking(IGameObject gameObject)
    {
        _gameObjectPreviousZones.TryRemove(gameObject, out _);
    }

    private async UniTask TrackingLoopAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var wasCancelled = await UniTask.Delay(_updateDelay, cancellationToken: cancellationToken)
                .SuppressCancellationThrow();

            if (wasCancelled)
                break;

            foreach (var gameObject in _gameObjectPreviousZones.Keys)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                UpdateEntityState(gameObject);
            }
        }
    }

    private void UpdateEntityState(IGameObject gameObject)
    {
        if (!_gameObjectPreviousZones.TryGetValue(gameObject, out var previousZones))
            return;

        _currentZonesBuffer.Clear();
        _zonesInRegion.Clear();
        _grid.QueryRegion(gameObject.Transform.Position, _zonesInRegion, _regionRange);

        var shouldRemoveExitedZones = false;
        var currentPosition = gameObject.Transform.Position;

        foreach (var zone in _zonesInRegion)
        {
            if (zone.Contains(currentPosition))
            {
                _currentZonesBuffer.Add(zone);
            }
        }

        foreach (var previousZone in previousZones)
        {
            if (!_currentZonesBuffer.Contains(previousZone))
            {
                shouldRemoveExitedZones = true;
                OnGameObjectExitedZone(gameObject, previousZone);
            }
        }

        if (shouldRemoveExitedZones)
        {
            previousZones.RemoveWhere(z => !_currentZonesBuffer.Contains(z));
        }

        foreach (var currentZone in _currentZonesBuffer)
        {
            if (!previousZones.Contains(currentZone))
            {
                previousZones.Add(currentZone);
                OnGameObjectEnteredZone(gameObject, currentZone);
            }
        }
    }

    private void OnGameObjectEnteredZone(IGameObject gameObject, Zone zone)
    {
        GameObjectEnteredZone?.Invoke(gameObject, zone);
    }

    private void OnGameObjectExitedZone(IGameObject gameObject, Zone zone)
    {
        GameObjectExitedZone?.Invoke(gameObject, zone);
    }
}

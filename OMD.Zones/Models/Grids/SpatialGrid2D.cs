using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace OMD.Zones.Models.Grids;

public class SpatialGrid2D<T>(int cellSize)
{
    private readonly int _cellSize = cellSize;
    private readonly Dictionary<Vector2Int, List<T>> _grid = [];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector2Int GetCellCoords(in UVector3 position)
    {
        return GetCellCoords(position.x, position.z);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector2Int GetCellCoords(in SVector3 position)
    {
        return GetCellCoords(position.X, position.Z);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector2Int GetCellCoords(in float xPosition, in float zPosition)
    {
        var xCellCoordinate = (int)Math.Floor(xPosition / _cellSize);
        var yCellCoordinate = (int)Math.Floor(zPosition / _cellSize);

        return new(xCellCoordinate, yCellCoordinate);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(in UVector3 position, in T value)
    {
        var cellCoords = GetCellCoords(position);

        Add(ref cellCoords, value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(in SVector3 position, in T value)
    {
        var cellCoords = GetCellCoords(position);

        Add(ref cellCoords, value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(in float xPosition, in float zPosition, in T value)
    {
        var cellCoords = GetCellCoords(xPosition, zPosition);

        Add(ref cellCoords, value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Remove(in UVector3 position, in T value)
    {
        var cellCoords = GetCellCoords(position);

        return Remove(ref cellCoords, value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Remove(in SVector3 position, in T value)
    {
        var cellCoords = GetCellCoords(position);

        return Remove(ref cellCoords, value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Remove(in float xPosition, in float zPosition, in T value)
    {
        var cellCoords = GetCellCoords(xPosition, zPosition);

        return Remove(ref cellCoords, value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool RemoveRoughly(in T value)
    {
        foreach (var list in _grid.Values)
        {
            if (list.Remove(value))
                return true;
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void QueryCandidates(in UVector3 position, in HashSet<T> itemsInRegion, in int range = 0)
    {
        QueryCandidates(position.x, position.z, itemsInRegion, range);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void QueryRegion(in SVector3 position, in HashSet<T> itemsInRegion, in int range = 0)
    {
        QueryCandidates(position.X, position.Z, itemsInRegion, range);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void QueryCandidates(in float xPosition, in float zPosition, in HashSet<T> itemsInRegion, in int range = 0)
    {
        var cellCoords = GetCellCoords(xPosition, zPosition);

        if (range == 0)
        {
            if (_grid.TryGetValue(cellCoords, out var list))
            {
                foreach (var item in list)
                    itemsInRegion.Add(item);
            }

            return;
        }

        var initialCellX = cellCoords.x;
        var initialCellY = cellCoords.y;

        for (var dx = -range; dx <= range; dx += 1)
        {
            for (var dy = -range; dy <= range; dy += 1)
            {
                cellCoords.x += dx;
                cellCoords.y += dy;

                if (_grid.TryGetValue(cellCoords, out var list))
                {
                    foreach (var item in list)
                        itemsInRegion.Add(item);
                }

                cellCoords.x = initialCellX;
                cellCoords.y = initialCellY;
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Add(ref Vector2Int cellCoords, in T value)
    {
        if (!_grid.ContainsKey(cellCoords))
            _grid[cellCoords] = [];

        _grid[cellCoords].Add(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool Remove(ref Vector2Int cellCoords, in T value)
    {
        if (!_grid.TryGetValue(cellCoords, out var list))
            return false;

        var wasRemoved = list.Remove(value);

        if (list.Count == 0)
            _grid.Remove(cellCoords);

        return wasRemoved;
    }
}

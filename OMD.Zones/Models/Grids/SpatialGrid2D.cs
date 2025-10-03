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
    public Vector2Int GetCellCoords(UVector3 position)
    {
        return GetCellCoords(position.x, position.z);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector2Int GetCellCoords(SVector3 position)
    {
        return GetCellCoords(position.X, position.Z);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector2Int GetCellCoords(float xPosition, float zPosition)
    {
        var xCellCoordinate = (int)Math.Floor(xPosition / _cellSize);
        var yCellCoordinate = (int)Math.Floor(zPosition / _cellSize);

        return new(xCellCoordinate, yCellCoordinate);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(UVector3 position, T value)
    {
        var cellCoords = GetCellCoords(position);

        Add(ref cellCoords, value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(SVector3 position, T value)
    {
        var cellCoords = GetCellCoords(position);

        Add(ref cellCoords, value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(float xPosition, float zPosition, T value)
    {
        var cellCoords = GetCellCoords(xPosition, zPosition);

        Add(ref cellCoords, value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Remove(UVector3 position, T value)
    {
        var cellCoords = GetCellCoords(position);

        return Remove(ref cellCoords, value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Remove(SVector3 position, T value)
    {
        var cellCoords = GetCellCoords(position);

        return Remove(ref cellCoords, value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Remove(float xPosition, float zPosition, T value)
    {
        var cellCoords = GetCellCoords(xPosition, zPosition);

        return Remove(ref cellCoords, value);
    }

    public void QueryCandidates(float xPosition, float zPosition, List<T> candidates, int range = 0)
    {
        var cellCoords = GetCellCoords(xPosition, zPosition);

        if (range == 0)
        {
            if (_grid.TryGetValue(cellCoords, out var list))
                candidates.AddRange(list);

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
                    candidates.AddRange(list);
                }

                cellCoords.x = initialCellX;
                cellCoords.y = initialCellY;
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Add(ref Vector2Int cellCoords, T value)
    {
        if (!_grid.ContainsKey(cellCoords))
            _grid[cellCoords] = [];

        _grid[cellCoords].Add(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool Remove(ref Vector2Int cellCoords, T value)
    {
        if (!_grid.TryGetValue(cellCoords, out var list))
            return false;

        var wasRemoved = list.Remove(value);

        if (list.Count == 0)
            _grid.Remove(cellCoords);

        return wasRemoved;
    }
}

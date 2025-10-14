Better way of checking, whether player is in zone or not:

- No need in PhysX
- We already have a spatial hash grid for zones
- We already searching only trough players, ignoring other colliders
- No need in CPU-bound, using simple formulas

```cs
// ZoneSystem.cs
// Production-oriented spatial-hash zone detection for server plugins (Unity/Unturned).
// Key features:
// - Uniform grid spatial hash
// - Zone data stored in arrays (no boxing)
// - Pooling of lists
// - Per-player BitSet (fixed-size) for current zones (fast diff via bit ops)
// - Staggered updates: distribute players across ticks
// - Optional parallelism hooks (compute candidates in worker threads)

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace ZoneServer
{
    // Lightweight non-alloc bitset for fixed max zones.
    // Uses uint[] as storage. Operations: Clear, Set, Get, Copy, And/Or/Xor, Iteration (yield indices).
    public sealed class BitSet
    {
        readonly uint[] data;
        readonly int length; // number of bits

        public BitSet(int bits)
        {
            length = bits;
            data = new uint[(bits + 31) >> 5];
        }

        public void Clear()
        {
            Array.Clear(data, 0, data.Length);
        }

        public void Set(int index)
        {
            int word = index >> 5;
            int bit = index & 31;
            data[word] |= (1u << bit);
        }

        public void Unset(int index)
        {
            int word = index >> 5;
            int bit = index & 31;
            data[word] &= ~(1u << bit);
        }

        public bool Get(int index)
        {
            int word = index >> 5;
            int bit = index & 31;
            return (data[word] & (1u << bit)) != 0;
        }

        public void CopyFrom(BitSet other)
        {
            Buffer.BlockCopy(other.data, 0, data, 0, data.Length * 4);
        }

        // current &= ~other ; returns true if any bit changed from 1 to 0 (i.e., some exit)
        public bool RemoveBitsPresentIn(BitSet other)
        {
            bool changed = false;
            for (int i = 0; i < data.Length; i++)
            {
                uint before = data[i];
                uint after = before & ~other.data[i];
                if (before != after) changed = true;
                data[i] = after;
            }
            return changed;
        }

        // current OR= other ; returns true if any new bit set (enter)
        public bool OrWith(BitSet other)
        {
            bool changed = false;
            for (int i = 0; i < data.Length; i++)
            {
                uint before = data[i];
                uint after = before | other.data[i];
                if (before != after) changed = true;
                data[i] = after;
            }
            return changed;
        }

        // Iterate set bits (yield indices). Efficient enumeration: scanning words.
        public IEnumerable<int> SetBits()
        {
            for (int w = 0; w < data.Length; w++)
            {
                uint v = data[w];
                while (v != 0)
                {
                    int bit = BitOperations.TrailingZeroCount(v);
                    int idx = (w << 5) + bit;
                    if (idx >= length) yield break;
                    yield return idx;
                    v &= v - 1; // clear lowest set bit
                }
            }
        }
    }

    // Minimal BitOperations helper (for older frameworks you can implement trailing zero).
    static class BitOperations
    {
        public static int TrailingZeroCount(uint x)
        {
            // Using built-in when available, otherwise fallback.
            if (x == 0) return 32;
            int n = 0;
            while ((x & 1u) == 0) { n++; x >>= 1; }
            return n;
        }
    }

    // Zone struct (value type) stored in arrays
    public struct Zone
    {
        public int Id;
        public Vector3 Center;
        public float Radius;
        public float RadiusSqr;
        public Vector3 AabbMin;
        public Vector3 AabbMax;
    }

    // Simple list pool for reusing List<int>
    public static class ListPoolInt
    {
        static readonly Stack<List<int>> pool = new Stack<List<int>>();

        public static List<int> Rent()
        {
            lock (pool)
            {
                if (pool.Count > 0) return pool.Pop();
            }
            return new List<int>(16);
        }

        public static void Return(List<int> list)
        {
            list.Clear();
            lock (pool) pool.Push(list);
        }
    }

    public class ZoneManager
    {
        readonly float cellSize;
        readonly Dictionary<int, List<int>> grid = new Dictionary<int, List<int>>(1024);
        readonly List<Zone> zones = new List<Zone>();
        readonly Dictionary<int,int> zoneIdToIndex = new Dictionary<int,int>();

        public int ZoneCount => zones.Count;

        public ZoneManager(float cellSize = 50.0f)
        {
            this.cellSize = cellSize;
        }

        static int CellHash(int cx, int cy, int cz)
        {
            unchecked {
                return (cx * 73856093) ^ (cy * 19349663) ^ (cz * 83492791);
            }
        }

        static (int, int, int) PosToCell(Vector3 p, float cellSize)
        {
            return (Mathf.FloorToInt(p.x / cellSize),
                    Mathf.FloorToInt(p.y / cellSize),
                    Mathf.FloorToInt(p.z / cellSize));
        }

        public int RegisterZone(int id, Vector3 center, float radius)
        {
            var z = new Zone {
                Id = id,
                Center = center,
                Radius = radius,
                RadiusSqr = radius * radius,
                AabbMin = center - new Vector3(radius, radius, radius),
                AabbMax = center + new Vector3(radius, radius, radius)
            };
            int index = zones.Count;
            zones.Add(z);
            zoneIdToIndex[id] = index;

            var minCell = PosToCell(z.AabbMin, cellSize);
            var maxCell = PosToCell(z.AabbMax, cellSize);
            for (int x = minCell.Item1; x <= maxCell.Item1; x++)
                for (int y = minCell.Item2; y <= maxCell.Item2; y++)
                    for (int k = minCell.Item3; k <= maxCell.Item3; k++)
                    {
                        int h = CellHash(x,y,k);
                        if (!grid.TryGetValue(h, out var list)) { list = new List<int>(); grid[h]=list; }
                        list.Add(index);
                    }

            return index;
        }

        public bool UnregisterZone(int id)
        {
            if (!zoneIdToIndex.TryGetValue(id, out var idx)) return false;
            var z = zones[idx];
            var minCell = PosToCell(z.AabbMin, cellSize);
            var maxCell = PosToCell(z.AabbMax, cellSize);
            for (int x = minCell.Item1; x <= maxCell.Item1; x++)
                for (int y = minCell.Item2; y <= maxCell.Item2; y++)
                    for (int k = minCell.Item3; k <= maxCell.Item3; k++)
                    {
                        int h = CellHash(x,y,k);
                        if (grid.TryGetValue(h, out var list))
                        {
                            list.Remove(idx);
                            if (list.Count == 0) grid.Remove(h);
                        }
                    }
            // lazily keep zones array; mark as removed if needed (or compact periodically)
            zoneIdToIndex.Remove(id);
            // Optionally mark zones[idx] as invalid
            return true;
        }

        // Query unique candidate zone indices around position (neighborCells range)
        public void QueryCandidates(Vector3 pos, int neighborCells, List<int> outList)
        {
            outList.Clear();
            var c = PosToCell(pos, cellSize);
            var seen = HashSetPool.Rent(); // small temp hash to deduplicate indices
            try
            {
                for (int dx = -neighborCells; dx <= neighborCells; dx++)
                    for (int dy = -neighborCells; dy <= neighborCells; dy++)
                        for (int dz = -neighborCells; dz <= neighborCells; dz++)
                        {
                            int h = CellHash(c.Item1 + dx, c.Item2 + dy, c.Item3 + dz);
                            if (grid.TryGetValue(h, out var list))
                            {
                                for (int i = 0; i < list.Count; i++)
                                {
                                    int zi = list[i];
                                    if (!seen.Contains(zi))
                                    {
                                        seen.Add(zi);
                                        outList.Add(zi);
                                    }
                                }
                            }
                        }
            }
            finally
            {
                HashSetPool.Return(seen);
            }
        }

        public ref Zone GetZoneByIndex(int index)
        {
            return ref zones[index];
        }
    }

    // tiny HashSet<int> pool for dedupe in query (expected small size)
    public static class HashSetPool
    {
        static readonly Stack<HashSet<int>> pool = new Stack<HashSet<int>>();
        public static HashSet<int> Rent()
        {
            lock (pool)
            {
                if (pool.Count > 0) return pool.Pop();
            }
            return new HashSet<int>();
        }
        public static void Return(HashSet<int> hs)
        {
            hs.Clear();
            lock (pool) pool.Push(hs);
        }
    }

    // Per-player tracker. Uses BitSet of size Z_MAX to store current zones.
    public class PlayerZoneTracker
    {
        readonly ZoneManager zoneManager;
        readonly int zMax;
        readonly int neighborCells;
        readonly Dictionary<int, BitSet> playerBitsets = new Dictionary<int, BitSet>();
        readonly List<int> tempCandidates = new List<int>();

        // Events
        public event Action<int,int> OnEnter; // playerId, zoneIdIndex (index in zoneManager)
        public event Action<int,int> OnExit;

        public PlayerZoneTracker(ZoneManager zm, int zMax, int neighborCells = 1)
        {
            zoneManager = zm;
            this.zMax = zMax;
            this.neighborCells = neighborCells;
        }

        public void EnsurePlayerExists(int playerId)
        {
            if (!playerBitsets.ContainsKey(playerId))
                playerBitsets[playerId] = new BitSet(zMax);
        }

        // Called per-player when updating their position.
        // This is the hot path: try to avoid allocations here.
        public void UpdatePlayerPosition(int playerId, Vector3 pos)
        {
            if (!playerBitsets.TryGetValue(playerId, out var bitset))
            {
                bitset = new BitSet(zMax);
                playerBitsets[playerId] = bitset;
            }

            // Build a temporary bitset of current candidates that pass distance test
            var newBits = new BitSet(zMax); // Ideally reuse per-thread pool; simplified here
            zoneManager.QueryCandidates(pos, neighborCells, tempCandidates);

            for (int i = 0; i < tempCandidates.Count; i++)
            {
                int zi = tempCandidates[i];
                ref Zone z = ref zoneManager.GetZoneByIndex(zi);
                float dx = pos.x - z.Center.x;
                float dy = pos.y - z.Center.y;
                float dz = pos.z - z.Center.z;
                float d2 = dx*dx + dy*dy + dz*dz;
                if (d2 <= z.RadiusSqr) newBits.Set(zi);
            }

            // Determine enters: bits that are in newBits but not in bitset
            var enterBits = new BitSet(zMax);
            // enterBits = newBits & ~bitset  => we'll compute manually
            for (int w = 0; w < (zMax + 31) >> 5; w++)
            {
                uint nv = newBitsWord(newBits, w);
                uint bv = bitsetWord(bitset, w);
                uint enter = nv & ~bv;
                if (enter != 0)
                {
                    // iterate and fire OnEnter
                    uint v = enter;
                    while (v != 0)
                    {
                        int bit = BitOperations.TrailingZeroCount(v);
                        int idx = (w << 5) + bit;
                        OnEnter?.Invoke(playerId, idx);
                        v &= v - 1;
                    }
                }
            }

            // Determine exits: bits in bitset but not in newBits
            for (int w = 0; w < (zMax + 31) >> 5; w++)
            {
                uint bv = bitsetWord(bitset, w);
                uint nv = newBitsWord(newBits, w);
                uint exit = bv & ~nv;
                if (exit != 0)
                {
                    uint v = exit;
                    while (v != 0)
                    {
                        int bit = BitOperations.TrailingZeroCount(v);
                        int idx = (w << 5) + bit;
                        OnExit?.Invoke(playerId, idx);
                        v &= v - 1;
                    }
                }
            }

            // Replace old bitset with newBits (copy)
            bitset.CopyFrom(newBits);

            // local helpers to access private arrays via reflection-like approach omitted - you'd implement direct methods
            uint newBitsWord(BitSet bs, int wordIndex)
            {
                // Reflection avoided for clarity: if BitSet exposes internal array or method to get word - use it.
                // For now, implement simple workaround to iterate SetBits (slower).
                // In production, make BitSet expose internal uint[] or method GetWord(index).
                throw new NotImplementedException("Make BitSet expose GetWord(wordIndex) for performance.");
            }
            uint bitsetWord(BitSet bs, int wordIndex) => throw new NotImplementedException("See above");
        }
    }
}

```

```cs
public class ZonePlugin : OpenModUnturnedPlugin
{
    private ZoneManager _zoneManager;
    private PlayerZoneTracker _tracker;
    private CancellationTokenSource _cts;

    public ZonePlugin(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    protected override UniTask OnLoadAsync()
    {
        _zoneManager = new ZoneManager(cellSize: 50f);
        _tracker = new PlayerZoneTracker(_zoneManager, zMax: 30000);

        _tracker.OnEnter += (playerId, zoneIdx) =>
        {
            var zone = _zoneManager.GetZoneByIndex(zoneIdx);
            Logger.LogInformation($"Player {playerId} entered zone {zone.Id}");
        };

        _tracker.OnExit += (playerId, zoneIdx) =>
        {
            var zone = _zoneManager.GetZoneByIndex(zoneIdx);
            Logger.LogInformation($"Player {playerId} left zone {zone.Id}");
        };

        _cts = new CancellationTokenSource();
        StartBackgroundLoop(_cts.Token).Forget();

        return UniTask.CompletedTask;
    }

    protected override UniTask OnUnloadAsync()
    {
        _cts?.Cancel();
        return UniTask.CompletedTask;
    }
}

private async UniTaskVoid StartBackgroundLoop(CancellationToken ct)
{
    var delay = TimeSpan.FromMilliseconds(100); // проверка каждые 100 мс

    while (!ct.IsCancellationRequested)
    {
        try
        {
            var players = Provider.clients; // все игроки на сервере
            foreach (var client in players)
            {
                var steamId = client.playerID.steamID.m_SteamID;
                var playerId = (int)(steamId & int.MaxValue); // нормализация под BitSet

                // доступ к transform только на mainthread
                var pos = await UniTask.SwitchToMainThread().ContinueWith(() =>
                {
                    return client.player.transform.position;
                });

                // возвращаемся в threadpool и считаем зоны
                await UniTask.SwitchToThreadPool();
                _tracker.EnsurePlayerExists(playerId);
                _tracker.UpdatePlayerPosition(playerId, pos);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error in zone update loop");
        }

        await UniTask.Delay(delay, cancellationToken: ct);
    }
}

```
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace ROCHEFORT6000.Core.Database
{
    public class BaseLocation
    {
        public string Name { get; set; } = string.Empty;
        public int HexX { get; set; }
        public int HexY { get; set; }
        public string InitialNation { get; set; } = string.Empty;
        public string InitialRegion { get; set; } = string.Empty;
        public string CurrentController { get; set; } = string.Empty;
        public bool IsPort { get; set; }
    }

    public class BaseLocationRegistry
    {
        private static readonly Lazy<BaseLocationRegistry> _instance = new(() => new BaseLocationRegistry());
        public static BaseLocationRegistry Instance => _instance.Value;

        private readonly List<BaseLocation> _bases = new(1350);
        private readonly Dictionary<string, int> _nameIndex = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<int, List<int>> _coordIndex = new();

        public IReadOnlyList<BaseLocation> AllBases => _bases;
        public int Count => _bases.Count;

        private class JsonBaseItem
        {
            public string name { get; set; } = string.Empty;
            public int hexX { get; set; }
            public int hexY { get; set; }
            public string nation { get; set; } = string.Empty;
            public string region { get; set; } = string.Empty;
        }

        private class JsonRoot
        {
            public List<JsonBaseItem> bases { get; set; } = new();
        }

        private BaseLocationRegistry()
        {
            LoadFromAsset();
        }

        private void LoadFromAsset()
        {
            string[] possiblePaths = new[]
            {
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "assets", "data", "base_locations.json"),
                Path.Combine(Directory.GetCurrentDirectory(), "assets", "data", "base_locations.json"),
                Path.Combine("C:\\Users\\ozgur\\Documents\\Witp-CIC-MUTHUR", "assets", "data", "base_locations.json")
            };

            string? targetPath = null;
            foreach (var p in possiblePaths)
            {
                if (File.Exists(p))
                {
                    targetPath = p;
                    break;
                }
            }

            if (targetPath != null)
            {
                try
                {
                    string json = File.ReadAllText(targetPath);
                    var root = JsonSerializer.Deserialize<JsonRoot>(json);
                    if (root?.bases != null)
                    {
                        foreach (var b in root.bases)
                        {
                            var loc = new BaseLocation
                            {
                                Name = b.name,
                                HexX = b.hexX,
                                HexY = b.hexY,
                                InitialNation = b.nation,
                                InitialRegion = b.region,
                                CurrentController = b.nation,
                                IsPort = DetermineIsPort(b.name, b.region)
                            };
                            AddBase(loc);
                        }
                    }
                }
                catch { }
            }
        }

        private static bool DetermineIsPort(string name, string region)
        {
            string lName = name.ToLowerInvariant();
            string lReg = region.ToLowerInvariant();

            return lName.Contains("harbor") || lName.Contains("port") ||
                   lName.Contains("bay") || lName.Contains("island") ||
                   lName.Contains("sound") || lName.Contains("anchorage") ||
                   lName.Contains("atoll") || lName.Contains("reef") ||
                   lName.Contains("ominato") || lName.Contains("kure") ||
                   lName.Contains("sasebo") || lName.Contains("yokosuka") ||
                   lName.Contains("truk") || lName.Contains("rabaul") ||
                   lName.Contains("singapore") || lName.Contains("manila") ||
                   lName.Contains("surabaya") || lName.Contains("batavia") ||
                   lName.Contains("kota bharu") || lName.Contains("darwin") ||
                   lName.Contains("sydney") || lName.Contains("brisbane") ||
                   lName.Contains("melbourne") || lName.Contains("perth") ||
                   lName.Contains("fremantle") || lName.Contains("auckland") ||
                   lName.Contains("wellington") || lName.Contains("noumea") ||
                   lName.Contains("suva") || lName.Contains("pago pago") ||
                   lName.Contains("san francisco") || lName.Contains("san diego") ||
                   lName.Contains("seattle") || lName.Contains("bremerton") ||
                   lName.Contains("vancouver") || lName.Contains("victoria") ||
                   lReg.Contains("island") || lReg.Contains("hawaii") ||
                   lReg.Contains("aleutian") || lReg.Contains("marshall") ||
                   lReg.Contains("caroline") || lReg.Contains("mariana") ||
                   lReg.Contains("solomon") || lReg.Contains("gilbert") ||
                   lReg.Contains("ellice") || lReg.Contains("fiji") ||
                   lReg.Contains("samoa") || lReg.Contains("hebrides") ||
                   lReg.Contains("philippines") || lReg.Contains("dutch east indies") ||
                   lReg.Contains("malaya") || lReg.Contains("borneo") ||
                   lReg.Contains("sumatra") || lReg.Contains("java") ||
                   lReg.Contains("celeb") || lReg.Contains("molucca") ||
                   lReg.Contains("timor") || lReg.Contains("new guinea") ||
                   lReg.Contains("new britain") || lReg.Contains("new ireland");
        }

        private void AddBase(BaseLocation loc)
        {
            int idx = _bases.Count;
            _bases.Add(loc);
            _nameIndex[loc.Name] = idx;

            int key = loc.HexX * 1000 + loc.HexY;
            if (!_coordIndex.TryGetValue(key, out var list))
            {
                list = new List<int>();
                _coordIndex[key] = list;
            }
            list.Add(idx);
        }

        public BaseLocation? FindBaseByName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return null;
            if (_nameIndex.TryGetValue(name, out int idx)) return _bases[idx];

            string lName = name.ToLowerInvariant();
            foreach (var kvp in _nameIndex)
            {
                if (kvp.Key.Contains(lName, StringComparison.OrdinalIgnoreCase) ||
                    lName.Contains(kvp.Key, StringComparison.OrdinalIgnoreCase))
                {
                    return _bases[kvp.Value];
                }
            }
            return null;
        }

        public BaseLocation? FindBaseByCoords(int hexX, int hexY)
        {
            int key = hexX * 1000 + hexY;
            if (_coordIndex.TryGetValue(key, out var list) && list.Count > 0)
            {
                return _bases[list[0]];
            }
            return null;
        }

        public BaseLocation? FindNearestBase(int hexX, int hexY, out double distNM)
        {
            BaseLocation? best = null;
            double minDist = double.MaxValue;

            foreach (var b in _bases)
            {
                double dx = b.HexX - hexX;
                double dy = b.HexY - hexY;
                double dist = Math.Sqrt(dx * dx + dy * dy);
                if (dist < minDist)
                {
                    minDist = dist;
                    best = b;
                }
            }

            distNM = minDist * 40.0;
            return best;
        }

        public void UpdateBaseController(string baseName, string newController)
        {
            if (_nameIndex.TryGetValue(baseName, out int idx))
            {
                _bases[idx].CurrentController = newController;
            }
        }

        public List<BaseLocation> SearchBases(string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return new List<BaseLocation>(_bases);
            string q = query.ToLowerInvariant();
            return _bases.FindAll(b => b.Name.ToLowerInvariant().Contains(q) ||
                                       b.InitialRegion.ToLowerInvariant().Contains(q) ||
                                       b.CurrentController.ToLowerInvariant().Contains(q));
        }
    }
}

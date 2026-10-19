using AssetsTools.NET;
using AssetsTools.NET.Extra;

// Usage:
//   BundleTool list <bundle>...
//       Lists the MonoScripts each bundle references (assembly / namespace / class).
//   BundleTool retarget <in> <out> <newAssembly.dll> [oldAssembly.dll ...]
//       Rewrites m_AssemblyName on MonoScripts so bundle components bind to the merged assembly.
//       If old assembly names are given, only those are rewritten.

if (args.Length < 2)
{
    Console.WriteLine("usage: BundleTool list <bundle>... | retarget <in> <out> <newAssembly.dll> [oldAssembly.dll ...]");
    return 1;
}

var manager = new AssetsManager();

switch (args[0])
{
    case "list":
        foreach (var path in args.Skip(1))
            List(path);
        return 0;
    case "retarget":
        Retarget(args[1], args[2], args[3], args.Skip(4).ToHashSet(StringComparer.OrdinalIgnoreCase));
        return 0;
    case "sounds":
        foreach (var s in args.Skip(1).SelectMany(SoundStrings).Distinct().OrderBy(s => s))
            Console.WriteLine(s);
        return 0;
    case "tree":
        // tree <bundle> <rootGameObjectName>
        Tree(args[1], args[2]);
        return 0;
    case "field":
        // field <bundle> <fieldName> : prints every string value of that field, with the owning asset's name
        Field(args[1], args[2]);
        return 0;
    case "names":
        // names <bundle> : every loadable asset path in the bundle (AssetBundle.m_Container)
        foreach (var name in Names(args[1]).OrderBy(n => n))
            Console.WriteLine(name);
        return 0;
    case "guid":
        // guid <catalog.json> <guid>... : resolves Addressables keys (asset GUIDs) to asset paths and their bundles
        Guid(args[1], args.Skip(2).ToArray());
        return 0;
    case "bnk":
        // bnk <bank> <outDir> [candidateNames.txt]
        Bnk.Dump(args[1], args[2], args.Length > 3 ? args[3] : null);
        return 0;
    default:
        Console.WriteLine($"unknown command {args[0]}");
        return 1;
}

void List(string path)
{
    var bundle = manager.LoadBundleFile(path, true);
    Console.WriteLine($"=== {Path.GetFileName(path)} (unity {bundle.file.Header.EngineVersion})");
    for (int i = 0; i < bundle.file.BlockAndDirInfo.DirectoryInfos.Count; i++)
    {
        if (!bundle.file.IsAssetsFile(i))
            continue;
        var assets = manager.LoadAssetsFileFromBundle(bundle, i, false);
        var counts = new Dictionary<long, int>();
        foreach (var mb in assets.file.GetAssetsOfType(AssetClassID.MonoBehaviour))
        {
            var field = manager.GetBaseField(assets, mb);
            var script = field["m_Script"];
            long key = script["m_PathID"].AsLong + (script["m_FileID"].AsInt * 1_000_000_000_000L);
            counts[key] = counts.GetValueOrDefault(key) + 1;
        }
        var byAssembly = new SortedDictionary<string, List<string>>();
        foreach (var ms in assets.file.GetAssetsOfType(AssetClassID.MonoScript))
        {
            var field = manager.GetBaseField(assets, ms);
            string asm = field["m_AssemblyName"].AsString;
            string ns = field["m_Namespace"].AsString;
            string cls = field["m_ClassName"].AsString;
            int uses = counts.GetValueOrDefault(ms.PathId);
            if (!byAssembly.TryGetValue(asm, out var list))
                byAssembly[asm] = list = new List<string>();
            list.Add($"{(ns.Length > 0 ? ns + "." : "")}{cls} (x{uses})");
        }
        foreach (var (asm, list) in byAssembly)
        {
            Console.WriteLine($"  [{asm}]");
            foreach (var s in list.OrderBy(x => x))
                Console.WriteLine($"    {s}");
        }
        var external = assets.file.Metadata.Externals.Select(e => e.PathName).ToList();
        if (external.Count > 0)
            Console.WriteLine($"  externals: {string.Join(", ", external)}");
    }
    manager.UnloadAll();
}

void Retarget(string inPath, string outPath, string newAssembly, HashSet<string> oldAssemblies)
{
    var bundle = manager.LoadBundleFile(inPath, true);
    int changed = 0;
    for (int i = 0; i < bundle.file.BlockAndDirInfo.DirectoryInfos.Count; i++)
    {
        if (!bundle.file.IsAssetsFile(i))
            continue;
        var assets = manager.LoadAssetsFileFromBundle(bundle, i, false);
        foreach (var ms in assets.file.GetAssetsOfType(AssetClassID.MonoScript))
        {
            var field = manager.GetBaseField(assets, ms);
            string asm = field["m_AssemblyName"].AsString;
            if (oldAssemblies.Count > 0 ? !oldAssemblies.Contains(asm) : asm == newAssembly)
                continue;
            if (oldAssemblies.Count == 0 && !IsModAssembly(asm))
                continue;
            field["m_AssemblyName"].AsString = newAssembly;
            ms.SetNewData(field);
            changed++;
        }
        bundle.file.BlockAndDirInfo.DirectoryInfos[i].SetNewData(assets.file);
    }
    Console.WriteLine($"{Path.GetFileName(inPath)}: retargeted {changed} MonoScripts to {newAssembly}");

    // Write uncompressed first, then recompress with LZ4 so the output stays small.
    var tmp = outPath + ".tmp";
    using (var writer = new AssetsFileWriter(tmp))
        bundle.file.Write(writer);
    manager.UnloadAll();

    var raw = new AssetBundleFile();
    using (var reader = new AssetsFileReader(File.OpenRead(tmp)))
    {
        raw.Read(reader);
        using var writer = new AssetsFileWriter(outPath);
        raw.Pack(writer, AssetBundleCompressionType.LZ4);
    }
    File.Delete(tmp);
}

List<string> Names(string path)
{
    var names = new List<string>();
    var bundle = manager.LoadBundleFile(path, true);
    for (int i = 0; i < bundle.file.BlockAndDirInfo.DirectoryInfos.Count; i++)
    {
        if (!bundle.file.IsAssetsFile(i))
            continue;
        var assets = manager.LoadAssetsFileFromBundle(bundle, i, false);
        foreach (var ab in assets.file.GetAssetsOfType(AssetClassID.AssetBundle))
            foreach (var entry in manager.GetBaseField(assets, ab)["m_Container"]["Array"].Children)
                names.Add(entry["first"].AsString);
    }
    manager.UnloadAll();
    return names.Distinct().ToList();
}

// Addressables ContentCatalogData: keys (m_KeyDataString), buckets mapping key -> entries (m_BucketDataString),
// entries of 7 ints whose first is an index into m_InternalIds (m_EntryDataString).
void Guid(string catalogPath, string[] keys)
{
    using var json = System.Text.Json.JsonDocument.Parse(File.ReadAllBytes(catalogPath));
    var root = json.RootElement;
    var internalIds = root.GetProperty("m_InternalIds").EnumerateArray().Select(e => e.GetString()!).ToArray();
    var keyData = Convert.FromBase64String(root.GetProperty("m_KeyDataString").GetString()!);
    var bucketData = Convert.FromBase64String(root.GetProperty("m_BucketDataString").GetString()!);
    var entryData = Convert.FromBase64String(root.GetProperty("m_EntryDataString").GetString()!);

    string? ReadKey(int offset)
    {
        byte type = keyData[offset];
        int len = BitConverter.ToInt32(keyData, offset + 1);
        return type switch
        {
            0 => System.Text.Encoding.ASCII.GetString(keyData, offset + 5, len),
            1 => System.Text.Encoding.Unicode.GetString(keyData, offset + 5, len),
            _ => null
        };
    }

    var lookup = new Dictionary<string, List<int>>();
    int bucketCount = BitConverter.ToInt32(bucketData, 0);
    int pos = 4;
    for (int b = 0; b < bucketCount; b++)
    {
        int dataOffset = BitConverter.ToInt32(bucketData, pos);
        int entryCount = BitConverter.ToInt32(bucketData, pos + 4);
        pos += 8;
        var entries = new List<int>();
        for (int e = 0; e < entryCount; e++, pos += 4)
            entries.Add(BitConverter.ToInt32(bucketData, pos));
        var key = ReadKey(dataOffset);
        if (key != null)
            lookup[key] = entries;
    }

    foreach (var key in keys)
    {
        if (!lookup.TryGetValue(key, out var entries))
        {
            // Keys are GUIDs only; fall back to matching the asset path (internal id) itself.
            int entryCount = BitConverter.ToInt32(entryData, 0);
            entries = Enumerable.Range(0, entryCount)
                .Where(e => internalIds[BitConverter.ToInt32(entryData, 4 + e * 7 * 4)].EndsWith(key, StringComparison.OrdinalIgnoreCase))
                .ToList();
            if (entries.Count == 0)
            {
                Console.WriteLine($"{key}: not found");
                continue;
            }
        }
        foreach (var entry in entries)
        {
            int internalId = BitConverter.ToInt32(entryData, 4 + entry * 7 * 4);
            int dependencyKey = BitConverter.ToInt32(entryData, 4 + entry * 7 * 4 + 8);
            string bundle = "";
            if (dependencyKey >= 0)
            {
                // The dependency key's bucket lists the bundles this asset needs; the first is its own.
                int off = 4;
                for (int b = 0; b < bucketCount; b++)
                {
                    int count = BitConverter.ToInt32(bucketData, off + 4);
                    if (b == dependencyKey)
                    {
                        var deps = Enumerable.Range(0, count).Select(i => BitConverter.ToInt32(bucketData, off + 8 + i * 4));
                        bundle = string.Join(", ", deps.Select(d => Path.GetFileName(internalIds[BitConverter.ToInt32(entryData, 4 + d * 7 * 4)])).Take(1));
                        break;
                    }
                    off += 8 + count * 4;
                }
            }
            Console.WriteLine($"{key}: {internalIds[internalId]}  [{bundle}]");
        }
    }
}

void Field(string path, string fieldName)
{
    var bundle = manager.LoadBundleFile(path, true);
    for (int i = 0; i < bundle.file.BlockAndDirInfo.DirectoryInfos.Count; i++)
    {
        if (!bundle.file.IsAssetsFile(i))
            continue;
        var assets = manager.LoadAssetsFileFromBundle(bundle, i, false);
        foreach (var mb in assets.file.GetAssetsOfType(AssetClassID.MonoBehaviour))
        {
            AssetTypeValueField field;
            try { field = manager.GetBaseField(assets, mb); }
            catch { continue; }
            string owner = field["m_Name"].IsDummy ? mb.PathId.ToString() : field["m_Name"].AsString;
            Walk(field, owner, "");
        }
    }
    manager.UnloadAll();

    void Walk(AssetTypeValueField f, string owner, string prefix)
    {
        string here = prefix.Length > 0 ? prefix + "." + f.FieldName : f.FieldName;
        if (f.FieldName == fieldName && f.Value != null && f.Value.ValueType == AssetValueType.String)
            Console.WriteLine($"{owner}: {here} = \"{f.AsString}\"");
        foreach (var child in f.Children)
            Walk(child, owner, here);
    }
}

// Prints the transform hierarchy (with component types) under every root GameObject with the given name.
void Tree(string path, string rootName)
{
    var bundle = manager.LoadBundleFile(path, true);
    for (int i = 0; i < bundle.file.BlockAndDirInfo.DirectoryInfos.Count; i++)
    {
        if (!bundle.file.IsAssetsFile(i))
            continue;
        var assets = manager.LoadAssetsFileFromBundle(bundle, i, false);
        foreach (var go in assets.file.GetAssetsOfType(AssetClassID.GameObject))
        {
            var goField = manager.GetBaseField(assets, go);
            if (goField["m_Name"].AsString != rootName)
                continue;
            var transform = TransformOf(assets, goField);
            if (transform != null && transform["m_Father"]["m_PathID"].AsLong == 0)
                PrintNode(assets, goField, 0);
        }
    }
    manager.UnloadAll();
}

AssetTypeValueField? TransformOf(AssetsFileInstance assets, AssetTypeValueField goField)
{
    foreach (var c in goField["m_Component"]["Array"].Children)
    {
        var info = assets.file.GetAssetInfo(c["component"]["m_PathID"].AsLong);
        if (info != null && (info.TypeId == (int)AssetClassID.Transform || info.TypeId == (int)AssetClassID.RectTransform))
            return manager.GetBaseField(assets, info);
    }
    return null;
}

void PrintNode(AssetsFileInstance assets, AssetTypeValueField goField, int depth)
{
    var comps = new List<string>();
    AssetTypeValueField? transform = null;
    foreach (var c in goField["m_Component"]["Array"].Children)
    {
        var info = assets.file.GetAssetInfo(c["component"]["m_PathID"].AsLong);
        if (info == null)
            continue;
        var type = (AssetClassID)info.TypeId;
        if (type == AssetClassID.Transform || type == AssetClassID.RectTransform)
            transform = manager.GetBaseField(assets, info);
        if (type == AssetClassID.MonoBehaviour)
        {
            string name = "MonoBehaviour";
            try
            {
                var ext = manager.GetExtAsset(assets, manager.GetBaseField(assets, info)["m_Script"]);
                if (ext.baseField != null)
                    name = ext.baseField["m_ClassName"].AsString;
            }
            catch { }
            comps.Add(name);
        }
        else if (type == AssetClassID.MeshFilter || type == AssetClassID.SkinnedMeshRenderer)
        {
            var mesh = manager.GetBaseField(assets, info)["m_Mesh"];
            comps.Add($"{type}(mesh {mesh["m_FileID"].AsInt}:{mesh["m_PathID"].AsLong})");
        }
        else
            comps.Add(type.ToString());
        if (type is AssetClassID.MeshRenderer or AssetClassID.SkinnedMeshRenderer or AssetClassID.ParticleSystemRenderer or AssetClassID.LineRenderer)
        {
            var r = manager.GetBaseField(assets, info);
            var mats = r["m_Materials"]["Array"].Children.Select(m => $"{m["m_FileID"].AsInt}:{m["m_PathID"].AsLong}");
            comps[^1] += $"(enabled {r["m_Enabled"].AsByte}, mats [{string.Join(" ", mats)}])";
        }
    }
    Console.WriteLine($"{new string(' ', depth * 2)}{goField["m_Name"].AsString}  [{string.Join(", ", comps)}]");
    if (transform == null)
        return;
    foreach (var child in transform["m_Children"]["Array"].Children)
    {
        var childTransform = manager.GetBaseField(assets, assets.file.GetAssetInfo(child["m_PathID"].AsLong));
        var childGo = manager.GetBaseField(assets, assets.file.GetAssetInfo(childTransform["m_GameObject"]["m_PathID"].AsLong));
        PrintNode(assets, childGo, depth + 1);
    }
}

// Every string field in every MonoBehaviour that looks like a Wwise event name (soundName, sfx fields, ...).
IEnumerable<string> SoundStrings(string path)
{
    var found = new HashSet<string>();
    var bundle = manager.LoadBundleFile(path, true);
    for (int i = 0; i < bundle.file.BlockAndDirInfo.DirectoryInfos.Count; i++)
    {
        if (!bundle.file.IsAssetsFile(i))
            continue;
        var assets = manager.LoadAssetsFileFromBundle(bundle, i, false);
        foreach (var mb in assets.file.GetAssetsOfType(AssetClassID.MonoBehaviour))
        {
            AssetTypeValueField field;
            try { field = manager.GetBaseField(assets, mb); }
            catch { continue; }
            Walk(field);
        }
    }
    manager.UnloadAll();
    return found;

    void Walk(AssetTypeValueField f)
    {
        if (f.Value != null && f.Value.ValueType == AssetValueType.String)
        {
            var s = f.AsString;
            if (s.StartsWith("Play_") || s.StartsWith("Stop_") || s.StartsWith("Pause_") || s.StartsWith("Resume_") || s.StartsWith("Set_"))
                found.Add(s);
        }
        foreach (var child in f.Children)
            Walk(child);
    }
}

static bool IsModAssembly(string asm) =>
    !(asm.StartsWith("UnityEngine") || asm.StartsWith("Unity.") || asm == "RoR2.dll" || asm.StartsWith("Assembly-CSharp")
      || asm.StartsWith("com.unity") || asm == "Decalicious.dll" || asm == "KinematicCharacterController.dll"
      || asm == "Wwise.dll" || asm == "AkSoundEngine.dll" || asm.StartsWith("HG") || asm == "LegacyResourcesAPI.dll");

using System.Text;

// Minimal Wwise soundbank reader: extracts embedded .wem files and lists event IDs,
// naming them by FNV-1 hashing candidate event names (Wwise's ID scheme).
static class Bnk
{
    public static void Dump(string bankPath, string outDir, string? namesFile)
    {
        var data = File.ReadAllBytes(bankPath);
        Directory.CreateDirectory(outDir);
        var sections = new Dictionary<string, (int offset, int length)>();
        for (int pos = 0; pos + 8 <= data.Length;)
        {
            string tag = Encoding.ASCII.GetString(data, pos, 4);
            int len = BitConverter.ToInt32(data, pos + 4);
            sections[tag] = (pos + 8, len);
            pos += 8 + len;
        }
        uint version = BitConverter.ToUInt32(data, sections["BKHD"].offset);
        uint bankId = BitConverter.ToUInt32(data, sections["BKHD"].offset + 4);
        var report = new StringBuilder();
        report.AppendLine($"bank {Path.GetFileName(bankPath)}: version {version}, id {bankId}");

        // Embedded media: DIDX holds (id, offset, size) triples into DATA.
        var wemDir = Path.Combine(outDir, "wem");
        Directory.CreateDirectory(wemDir);
        if (sections.TryGetValue("DIDX", out var didx) && sections.TryGetValue("DATA", out var dataSec))
        {
            for (int p = didx.offset; p < didx.offset + didx.length; p += 12)
            {
                uint id = BitConverter.ToUInt32(data, p);
                int off = BitConverter.ToInt32(data, p + 4);
                int size = BitConverter.ToInt32(data, p + 8);
                File.WriteAllBytes(Path.Combine(wemDir, $"{id}.wem"), data.AsSpan(dataSec.offset + off, size).ToArray());
            }
            report.AppendLine($"extracted {didx.length / 12} .wem files to {wemDir}");
        }

        // HIRC objects: type 4 = Event, 3 = Action (references the sound/container it plays).
        var events = new List<uint>();
        var counts = new Dictionary<byte, int>();
        var hirc = sections["HIRC"];
        uint count = BitConverter.ToUInt32(data, hirc.offset);
        int q = hirc.offset + 4;
        for (int i = 0; i < count; i++)
        {
            byte type = data[q];
            int size = BitConverter.ToInt32(data, q + 1);
            uint id = BitConverter.ToUInt32(data, q + 5);
            counts[type] = counts.GetValueOrDefault(type) + 1;
            if (type == 4)
                events.Add(id);
            q += 5 + size;
        }
        report.AppendLine("HIRC object counts: " + string.Join(", ", counts.OrderBy(k => k.Key).Select(k => $"type{k.Key}={k.Value}")));

        var names = new Dictionary<uint, string>();
        if (namesFile != null)
            foreach (var name in File.ReadAllLines(namesFile).Select(l => l.Trim()).Where(l => l.Length > 0))
                names[Fnv1(name)] = name;

        report.AppendLine($"{events.Count} events:");
        int named = 0;
        foreach (var id in events)
        {
            if (names.TryGetValue(id, out var n)) named++;
            report.AppendLine($"  {id}\t{(n ?? "<unknown>")}");
        }
        report.AppendLine($"named {named}/{events.Count}");
        File.WriteAllText(Path.Combine(outDir, "events.txt"), report.ToString());
        Console.Write(report.ToString());
    }

    // Wwise ShortID: 32-bit FNV-1 over the lowercased name.
    public static uint Fnv1(string name)
    {
        uint hash = 2166136261;
        foreach (byte b in Encoding.ASCII.GetBytes(name.ToLowerInvariant()))
        {
            hash *= 16777619;
            hash ^= b;
        }
        return hash;
    }
}

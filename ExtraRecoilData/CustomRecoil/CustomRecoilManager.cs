using ExtraRecoilData.JSON;
using GTFO.API.Utilities;
using MTFO.API;
using System.Collections.Generic;
using System.IO;
using ExtraRecoilData.Utils;

namespace ExtraRecoilData.CustomRecoil
{
    public class CustomRecoilManager
    {
        public static CustomRecoilManager Current { get; private set; } = new();

        private readonly Dictionary<uint, CustomRecoilData> customData = new();

        private readonly LiveEditListener liveEditListener;

        public string DEFINITION_PATH { get; private set; }

        public override string ToString()
        {
            return "Printing manager: " + customData.ToString();
        }

        protected virtual void AddCustomRecoilData(CustomRecoilData? data)
        {
            if (data == null) return;

            if (customData.ContainsKey(data.ArchetypeID))
                ERDLogger.Warning("Replaced custom recoil for ArchetypeID " + data.ArchetypeID);

            customData[data.ArchetypeID] = data;
        }

        protected virtual void FileChanged(LiveEditEventArgs e)
        {
            ERDLogger.Warning($"LiveEdit File Changed: {e.FullPath}");
            LiveEdit.TryReadFileContent(e.FullPath, (content) =>
            {
                List<CustomRecoilData>? dataList = ERDJson.Deserialize<List<CustomRecoilData>>(content);

                if (dataList == null) return;

                foreach (CustomRecoilData data in dataList)
                    AddCustomRecoilData(data);
            });
        }

        public CustomRecoilData? GetCustomRecoilData(uint ArchetypeID) => customData.ContainsKey(ArchetypeID) ? customData[ArchetypeID] : null;

        private CustomRecoilManager()
        {
            DEFINITION_PATH = Path.Combine(MTFOPathAPI.CustomPath, EntryPoint.MODNAME);
            if (!Directory.Exists(DEFINITION_PATH))
            {
                Directory.CreateDirectory(DEFINITION_PATH);
                var file = File.CreateText(Path.Combine(DEFINITION_PATH, "Template.json"));
                file.WriteLine(ERDJson.Serialize(new List<CustomRecoilData>() { new() }));
                file.Flush();
                file.Close();
            }

            foreach (string confFile in Directory.EnumerateFiles(DEFINITION_PATH, "*.json", SearchOption.AllDirectories))
            {
                string content = File.ReadAllText(confFile);
                List<CustomRecoilData>? dataList = ERDJson.Deserialize<List<CustomRecoilData>>(content);

                if (dataList == null) continue;

                foreach (CustomRecoilData data in dataList)
                    AddCustomRecoilData(data);
            }

            liveEditListener = LiveEdit.CreateListener(DEFINITION_PATH, "*.json", true);
            liveEditListener.FileChanged += FileChanged;
        }
    }
}

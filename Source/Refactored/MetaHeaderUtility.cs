using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Verse;
using System.Text;
using System.Xml.Linq;
using System.IO;
using System.Linq;

namespace Madeline.ModMismatchFormatter
{
    public static class MetaHeaderUtility
    {
        #region Harmony Patch
        internal static bool UpdateLastAccessedSaveFileInLoadSelection(string path)
        {
            LastAccessedSaveFilePathInLoadSelection = path;
            return true;
        }
        #endregion
        public static string LastAccessedSaveFilePathInLoadSelection { get; private set; } // 게임 시작 전, 세이브 파일 선택할때 마지막으로 접근한 파일 경로
        private static string CurrnetUsingFilePath { get; set; }
        static readonly string MOD_META_DATAS = "modMetaDatas";
        static readonly string MOD_IDENTIFIER_NODENAME = "Identifier";
        static readonly string VERSION_NODENAME = "version";
        static XDocument xdoc;
        static string SaveFilePathCache;
        static Dictionary<string, string> versionDict = new Dictionary<string, string>();
        static bool isVersionDictDirty = false;
        public static void BeginReading(string filePath)
        {
            if(xdoc != null || string.IsNullOrEmpty(CurrnetUsingFilePath))
            {
                Log.Warning("xdoc is still open. force-closing");
                xdoc = null;
                CurrnetUsingFilePath = null;
            }
            SimpleLog.Log($"Trying to load {filePath}");
            xdoc = XDocument.Load(filePath);
            CurrnetUsingFilePath = filePath;
        }

        public static void EndReading()
        {
            xdoc.Save(CurrnetUsingFilePath);
            SimpleLog.Log("Saving xdoc");
            xdoc = null;
            CurrnetUsingFilePath = null;
            isVersionDictDirty = true;
        }

        public static string GetVersion(string Identifier)
        {
            string version = "Unknown";
            if(xdoc == null)
                throw new Exception($"invoke BeginReading before calling {nameof(RebuildVersionDict)}");

            if(isVersionDictDirty)
            {
                RebuildVersionDict();
            }
            
            if(versionDict.TryGetValue(Identifier, out var value))
                version = value;
            
            return version;
        }

        static void RebuildVersionDict()
        {
            SimpleLog.Log("Rebuilding VersionDict");
            isVersionDictDirty = false;
            versionDict.Clear();

            if(xdoc == null)
            {
                Log.Error($"invoke BeginReading before calling {nameof(RebuildVersionDict)}");
                return;
            }
            
            try
            {
                var ModMetaHeaders = xdoc.Root.Element("meta")?.Element(MOD_META_DATAS);
                if(ModMetaHeaders == null)
                    return;

                var elements = ModMetaHeaders.Elements();
                foreach(var node in elements)
                {
                    var Identifier = node.Element(MOD_IDENTIFIER_NODENAME)?.Value;
                    var Version = node.Element(VERSION_NODENAME)?.Value;

                    if(Identifier == null || Version == null) // variable sanity check
                        continue;

                    versionDict[Identifier] = Version;

                    SimpleLog.Log($"Set {Identifier} {Version}");
                }
            }
            catch(Exception ex)
            {
                Log.Error($"Exception while rebuilding VersionDict, exception : {ex.Message}");
                return;
            }
        }

        public static void SetVersions(List<ModMetaHeader> metaHeaders)
        {
            if(xdoc == null)
                throw new Exception("Invoke BeginReading before using this method");

            var meta = xdoc.Root.Element("meta");
            var modVersionsNode = GetElementWithForceCreation(meta, MOD_META_DATAS);
            modVersionsNode.RemoveAll();

            if(meta.Element(MOD_META_DATAS) == null)
                throw new Exception("F");

            for(int i = 0; i < metaHeaders.Count; i++)
            {
                var data = metaHeaders[i];
                var liNode = new XElement("li");
                liNode.Add(new XElement(MOD_IDENTIFIER_NODENAME) { Value = data.Identifier });
                liNode.Add(new XElement(VERSION_NODENAME) { Value = data.Version });
                SimpleLog.Log($"Writing {data.Identifier} {data.Version} to versionNode");
                modVersionsNode.Add(liNode);
            }
        }

        static XElement GetElementWithForceCreation(XElement node, string name)
        {
            var result = node.Element(name);
            if(result == null)
                node.Add(new XElement(name));
            
            result = node.Element(name);
            return result;
        }

        public static bool isVersionSame(List<Madeline.ModMismatchFormatter.Mod> saveMods, List<Madeline.ModMismatchFormatter.Mod> activeMods)
        { // saveMods와 activeMods는 길이가 무조건 같음
            SimpleLog.Log("Checking version difference...");
            if(saveMods.Count != activeMods.Count)
            {
                string ListOfSaveMods = string.Join(", ", saveMods.Select(mod => mod.Identifier));
                string ListOfActiveMods = string.Join(", ", activeMods.Select(mod => mod.Identifier));

                StringBuilder exceptionMessage = new StringBuilder();

                exceptionMessage.AppendLine("saveMods length and activeMods length are different.");
                exceptionMessage.AppendLine($"Save Mod Count : {saveMods.Count} Active Mods Count : {activeMods.Count}");
                exceptionMessage.AppendLine("Active Mods List");
                exceptionMessage.AppendLine(ListOfActiveMods);
                exceptionMessage.AppendLine("Save Mods List");
                exceptionMessage.AppendLine(ListOfSaveMods);

                throw new Exception(exceptionMessage.ToString());
            }
                
            int length = saveMods.Count;
            for(int i = 0; i < length; i++)
            {
                var save = saveMods[i];
                var active = activeMods[i];

                SimpleLog.Log($"Comparing {save.Identifier} ({save.Version}) - {active.Identifier} ({active.Version})");

                if(save.Identifier != active.Identifier)
                {
                    string ListOfSaveMods = string.Join(", ", saveMods.Select(mod => mod.Identifier));
                    string ListOfActiveMods = string.Join(", ", activeMods.Select(mod => mod.Identifier));

                    StringBuilder exceptionMessage = new StringBuilder();

                    exceptionMessage.AppendLine($"two mod's identifier is different while comparing version, save mod identifier : {save.Identifier}, active mod identifier : {active.Identifier}");
                    exceptionMessage.AppendLine($"Save Mod Count : {saveMods.Count} Active Mods Count : {activeMods.Count}");
                    exceptionMessage.AppendLine("Active Mods List");
                    exceptionMessage.AppendLine(ListOfActiveMods);
                    exceptionMessage.AppendLine("Save Mods List");
                    exceptionMessage.AppendLine(ListOfSaveMods);

                    throw new Exception(exceptionMessage.ToString());
                }

                if(save.Version != active.Version)
                    return false;
            }

            return true;
        }

        public static void StoreLastSavedFilePath(string path)
        {
            SaveFilePathCache = path;
        }

        public static void UpdateModVersionMetaHeader()
        {
            string rawFilePath = SaveFilePathCache;
            string filePath = GenFilePaths.FilePathForSavedGame(rawFilePath);
            MetaHeaderUtility.BeginReading(filePath);
            List<ModMetaHeader> metaHeaders = new List<ModMetaHeader>();
            foreach(var modContentPack in LoadedModManager.RunningMods)
            {
                // var metadata = modContentPack.GetMetaData();
                var version = MetaHeaderUtility.GetVersionFromManifestFile(modContentPack);
                metaHeaders.Add(new ModMetaHeader() { Identifier = modContentPack.PackageId, Version = version });
                SimpleLog.Log($"Add metadata to metaHeaders list : {modContentPack.PackageId}, {version}");
            }
            MetaHeaderUtility.SetVersions(metaHeaders);
            MetaHeaderUtility.EndReading();
        }

        public static string GetVersionFromManifestFile(ModContentPack pack)
        {
            string MenifestFilePath = Path.Combine(pack.RootDir, Path.Combine("About", "Manifest.xml"));
            string version = "Unknown";
            if(File.Exists(MenifestFilePath))
            {
                try
                {
                    XDocument Xdoc = XDocument.Load(MenifestFilePath);
                    var versionNode = Xdoc.Root.Element("version");
                    version = versionNode.Value ?? "Unknown";
                }
                catch(Exception ex)
                {
                    string error = $"exception in GetVersionFromManifestFile line\n{ex.ToString()}";
                    Log.Error(error);
                }
            }
            else
            {
                NoManifestFileInModWarning(pack.Name);
            }
            return version;
        }

        static void NoManifestFileInModWarning(string modName)
        {

        }
    }

    public struct ModMetaHeader
    {
        public string Identifier;
        public string Version;
    }
}
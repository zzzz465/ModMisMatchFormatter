using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Verse;
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
        static readonly string VERSION_NODENAME = "version";
        static XDocument xdoc;
        static string SaveFilePathCache;
        public static void BeginReading(string filePath)
        {
            if(xdoc != null || string.IsNullOrEmpty(CurrnetUsingFilePath))
            {
                Log.Warning("xdoc is still open. force-closing");
                xdoc = null;
                CurrnetUsingFilePath = null;
            }
            
            xdoc = XDocument.Load(filePath);
            CurrnetUsingFilePath = filePath;
        }

        public static void EndReading()
        {
            xdoc.Save(CurrnetUsingFilePath);
            xdoc = null;
            CurrnetUsingFilePath = null;
        }

        public static string GetVersion(string modName)
        {
            string version = "Unknown";
            if(xdoc == null)
                throw new Exception("invoke BeginReading before using this method");
            try
            {
                //version = xdoc.Root.Element("meta")?.Element(MOD_META_DATAS)?.Elements().ElementAt(index)?.Element(VERSION_NODENAME)?.Value ?? "Unknown";
                var ModMetaHeaders = xdoc.Root.Element("meta")?.Element(MOD_META_DATAS);
                if(ModMetaHeaders != null)
                {
                    version = (from node in ModMetaHeaders.Elements()
                               let modNameInNode = node.Element("ModName")?.Value
                               where string.Equals(modNameInNode, modName)
                               select node.Element(VERSION_NODENAME)?.Value).FirstOrDefault() ?? "Unknown";
                }
            }
            catch(Exception ex)
            {
                Log.Warning(ex.ToString());
            }
            return version;
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
                liNode.Add(new XElement("ModName") { Value = data.ModName });
                liNode.Add(new XElement(VERSION_NODENAME) { Value = data.Version });
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
            if(saveMods.Count != activeMods.Count)
                throw new Exception("Error in Version Compare logic in exception 1. please contact to modder");
                
            int length = saveMods.Count;
            for(int i = 0; i < length; i++)
            {
                var save = saveMods[i];
                var active = activeMods[i];

                if(save.ModName != active.ModName)
                    throw new Exception("Error in Version Compare Logic in exception 2, please contact to modder.");

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
                var metadata = modContentPack.GetMetaData();
                var version = MetaHeaderUtility.GetVersionFromManifestFile(modContentPack);
                metaHeaders.Add(new ModMetaHeader() { ModName = metadata.Name, Version = version });
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
        public string ModName;
        public string Version;
    }
}
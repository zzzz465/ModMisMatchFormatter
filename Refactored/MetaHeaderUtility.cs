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

        public static string GetVersion(int index)
        {
            string version;
            if(xdoc == null)
                throw new Exception("invoke BeginReading before using this method");
            try
            {
                version = xdoc.Root.Element("meta")?.Element(MOD_META_DATAS)?.Elements().ElementAt(index)?.Element(VERSION_NODENAME)?.Value ?? "Unknown";
            }
            catch
            {
                version = "Unknown";
            }
            return version;
        }

        public static void SetVersions(List<Pair<string, string>> datas)
        {
            if(xdoc == null)
                throw new Exception("Invoke BeginReading before using this method");

            var meta = xdoc.Root.Element("meta");
            var modVersionsNode = GetElementWithForceCreation(meta, MOD_META_DATAS);
            modVersionsNode.RemoveAll();
            if(meta.Element(MOD_META_DATAS) == null)
                throw new Exception("F");
            for(int i = 0; i < datas.Count; i++)
            {
                var data = datas[i];
                var liNode = new XElement("li");
                liNode.Add(new XElement("ModName") { Value = data.First });
                liNode.Add(new XElement(VERSION_NODENAME) { Value = data.Second });
                modVersionsNode.Add(liNode);
                Log.Message($"set mod {data.First} 's version to {data.Second}");
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

        public static void UpdateModVersionMetaHeader(string fileName)
        {
            string filePath = GenFilePaths.FilePathForSavedGame(fileName);
            MetaHeaderUtility.BeginReading(filePath);
            List<Pair<string,string>> ModMetaDatas = new List<Pair<string, string>>();
            foreach(var modContentPack in LoadedModManager.RunningMods)
            {
                var metadata = modContentPack.GetMetaData();
                var version = MetaHeaderUtility.GetVersionFromManifestFile(modContentPack);
                ModMetaDatas.Add(new Pair<string, string>(metadata.Name, version));
            }
            MetaHeaderUtility.SetVersions(ModMetaDatas);
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
                    version = versionNode.Value;
                }
                catch(Exception ex)
                {
                    Log.Error(ex.ToString());
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
}
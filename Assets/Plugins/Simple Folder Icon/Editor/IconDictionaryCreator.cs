using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
namespace SimpleFolderIcon.Editor
{
    public class IconDictionaryCreator : AssetPostprocessor
    {
        private const string AssetsPath = "Plugins/Simple Folder Icon/Icons";
        internal static Dictionary<string, Texture> IconDictionary;

        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            if (!ContainsIconAsset(importedAssets) &&
                !ContainsIconAsset(deletedAssets) &&
                !ContainsIconAsset(movedAssets) &&
                !ContainsIconAsset(movedFromAssetPaths))
            {
                return;
            }

            BuildDictionary();
        }

        private static bool ContainsIconAsset(string[] assets)
        {
            foreach (string str in assets)
            {
                var dirName = Path.GetDirectoryName(str);
                if (dirName != null && ReplaceSeparatorChar(dirName) == "Assets/" + AssetsPath)
                {
                    return true;
                }
            }
            return false;
        }

        private static string ReplaceSeparatorChar(string path)
        {
            return path.Replace("\\", "/");
        }

        internal static void BuildDictionary()
        {
            var dictionary = new Dictionary<string, Texture>();

            var dirPath = Path.Combine(Application.dataPath, AssetsPath).Replace("\\", "/");
            if (!Directory.Exists(dirPath))
            {
                IconDictionary = dictionary;
                return;
            }
            var dir = new DirectoryInfo(dirPath);
            var info = dir.GetFiles("*.png");
            foreach (var f in info)
            {
                var texture = (Texture)AssetDatabase.LoadAssetAtPath($"Assets/{AssetsPath}/{f.Name}", typeof(Texture2D));
                dictionary.Add(Path.GetFileNameWithoutExtension(f.Name), texture);
            }

            var infoSO = dir.GetFiles("*.asset");
            foreach (var f in infoSO)
            {
                var folderIconSO = (FolderIconSO)AssetDatabase.LoadAssetAtPath($"Assets/{AssetsPath}/{f.Name}", typeof(FolderIconSO));

                if (folderIconSO != null)
                {
                    var texture = (Texture)folderIconSO.icon;

                    foreach (string folderName in folderIconSO.folderNames)
                    {
                        if (folderName != null)
                        {
                            // dictionary.TryAdd(folderName, texture);
                            dictionary.Add(folderName, texture);
                        }
                    }
                }
            }

            IconDictionary = dictionary;
        }
    }
}

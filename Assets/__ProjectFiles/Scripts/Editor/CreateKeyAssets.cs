using UnityEditor;
using UnityEngine;
using Orpita.Items;

namespace Orpita.Editor
{
    internal static class CreateKeyAssets
    {
        [MenuItem("Tools/Orpita/Create Missing Key Assets")]
        private static void Create()
        {
            const string folder = "Assets/__ProjectFiles/Data";

            int created = 0;
            created += Ensure(folder, KeyType.Iron,   "Iron Key",   "IronKey");
            created += Ensure(folder, KeyType.Brass,  "Brass Key",  "BrassKey");
            created += Ensure(folder, KeyType.Silver, "Silver Key", "SilverKey");
            created += Ensure(folder, KeyType.Gold,   "Gold Key",   "GoldKey");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[CreateKeyAssets] Done — {created} asset(s) created, {4 - created} already existed.");
            EditorUtility.DisplayDialog("Key Assets", $"{created} key asset(s) created.", "OK");
        }

        private static int Ensure(string folder, KeyType type, string displayName, string fileName)
        {
            string path = $"{folder}/{fileName}.asset";
            if (AssetDatabase.LoadAssetAtPath<KeyDefinition>(path) != null)
                return 0;

            var asset = ScriptableObject.CreateInstance<KeyDefinition>();
            AssetDatabase.CreateAsset(asset, path);

            var so = new SerializedObject(asset);
            so.FindProperty("keyType").enumValueIndex = (int)type;
            so.FindProperty("displayName").stringValue = displayName;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(asset);
            return 1;
        }
    }
}

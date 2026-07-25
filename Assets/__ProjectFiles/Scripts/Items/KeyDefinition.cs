using UnityEngine;

namespace Orpita.Items
{
    /// <summary>
    /// Defines a key type by asset reference. Two keys are considered matching
    /// iff they reference the same asset instance (reference equality), so no id
    /// field is required.
    /// </summary>
    [CreateAssetMenu(menuName = "Orpita/Key Definition", fileName = "KeyDefinition")]
    public sealed class KeyDefinition : ScriptableObject
    {
        [SerializeField] private KeyType keyType = KeyType.Iron;
        [SerializeField] private string displayName = "Key";

        /// <summary>The elemental category of this key.</summary>
        public KeyType Type => keyType;

        /// <summary>Human-readable name, for UI/feedback.</summary>
        public string DisplayName => displayName;
    }
}

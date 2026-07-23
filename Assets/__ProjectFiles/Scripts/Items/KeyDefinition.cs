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
        [SerializeField] private string displayName = "Key";

        /// <summary>Human-readable name, for UI/feedback.</summary>
        public string DisplayName => displayName;
    }
}

using UnityEngine;

namespace MurderMystery
{
    /// <summary>
    /// ScriptableObject defining an immutable evidence clue definition.
    /// Clue definitions are strictly decoupled from player collection state.
    /// </summary>
    [CreateAssetMenu(fileName = "NewEvidenceItem", menuName = "Murder Mystery/Evidence Item")]
    public class EvidenceItem : ScriptableObject
    {
        [Header("Evidence Identity")]
        [Tooltip("Unique, stable evidence identifier (e.g. EV-KEYCARD-001)")]
        [SerializeField] private string evidenceId = "EV-UNKNOWN-000";

        [Header("Display Properties")]
        [Tooltip("Human-readable name of the clue")]
        [SerializeField] private string displayName = "New Clue";

        [Tooltip("Category of the clue")]
        [SerializeField] private EvidenceCategory category = EvidenceCategory.Physical;

        [Tooltip("Detailed description or transcript revealed upon inspection")]
        [TextArea(4, 10)]
        [SerializeField] private string description = "";

        [Tooltip("Optional icon or sprite representing the evidence in the journal")]
        [SerializeField] private Sprite icon;

        public string EvidenceId => evidenceId;
        public string DisplayName => displayName;
        public EvidenceCategory Category => category;
        public string Description => description;
        public Sprite Icon => icon;

        /// <summary>
        /// Factory helper to create an EvidenceItem instance at runtime (useful for automated testing or dynamic clues).
        /// </summary>
        public static EvidenceItem CreateInstance(
            string id,
            string name,
            string desc,
            EvidenceCategory cat = EvidenceCategory.Physical,
            Sprite clueIcon = null)
        {
            var item = ScriptableObject.CreateInstance<EvidenceItem>();
            item.evidenceId = id;
            item.displayName = name;
            item.description = desc;
            item.category = cat;
            item.icon = clueIcon;
            return item;
        }

        /// <summary>
        /// Mutator helper for runtime configuration or testing.
        /// </summary>
        public void SetData(
            string id,
            string name,
            string desc,
            EvidenceCategory cat = EvidenceCategory.Physical,
            Sprite clueIcon = null)
        {
            evidenceId = id;
            displayName = name;
            description = desc;
            category = cat;
            icon = clueIcon;
        }
    }
}

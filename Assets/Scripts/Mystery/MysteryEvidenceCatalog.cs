using System;
using System.Collections.Generic;
using UnityEngine;

namespace MurderMystery
{
    [CreateAssetMenu(
        fileName = "MysteryEvidenceCatalog",
        menuName = "Murder Mystery/Evidence Catalog")]
    public sealed class MysteryEvidenceCatalog : ScriptableObject
    {
        [SerializeField] private List<EvidenceItem> evidence = new List<EvidenceItem>();

        public EvidenceItem GetEvidence(string evidenceId)
        {
            if (string.IsNullOrWhiteSpace(evidenceId))
                return null;

            foreach (EvidenceItem item in evidence)
            {
                if (item != null &&
                    string.Equals(
                        item.EvidenceId,
                        evidenceId,
                        StringComparison.OrdinalIgnoreCase))
                {
                    return item;
                }
            }

            return null;
        }

        public bool HasAllCanonicalEvidence()
        {
            string[] required =
            {
                MysteryManager.KeycardId,
                MysteryManager.LogbookId,
                MysteryManager.PhoneId,
                MysteryManager.CctvId,
                MysteryManager.GrantId,
                MysteryManager.PowerId
            };

            foreach (string evidenceId in required)
            {
                if (GetEvidence(evidenceId) == null)
                    return false;
            }

            return true;
        }
    }
}

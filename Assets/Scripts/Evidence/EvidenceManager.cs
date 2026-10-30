using System;
using System.Collections.Generic;
using UnityEngine;

namespace MurderMystery
{
    /// <summary>
    /// Central manager responsible for tracking collected evidence clues.
    /// Ensures duplicate prevention via unique IDs, notifies listeners of collection events,
    /// and provides safe read-only queries.
    /// </summary>
    public class EvidenceManager : MonoBehaviour
    {
        private static EvidenceManager instance;

        public static EvidenceManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindFirstObjectByType<EvidenceManager>();
                }
                return instance;
            }
            internal set => instance = value;
        }

        [Header("Manager Settings")]
        [Tooltip("If enabled, marks this GameObject with DontDestroyOnLoad to survive scene changes.")]
        [SerializeField] private bool persistAcrossScenes = false;

        [Header("Initial Evidence (Optional Debug Preload)")]
        [Tooltip("Optional clues preloaded into the inventory on start.")]
        [SerializeField] private List<EvidenceItem> initialEvidence = new List<EvidenceItem>();

        // Internal evidence storage:
        // - List preserves collection order for UI journal display.
        // - Dictionary allows O(1) duplicate checks and lookup by evidence ID.
        private readonly List<EvidenceItem> collectedList = new List<EvidenceItem>();
        private readonly Dictionary<string, EvidenceItem> collectedById = new Dictionary<string, EvidenceItem>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Fired immediately after an evidence item is successfully collected.
        /// Person 3 (narrative) and Person 4 (UI/HUD) can subscribe to this event.
        /// </summary>
        public event Action<EvidenceItem> OnEvidenceCollected;

        /// <summary>
        /// Fired when all evidence is cleared (useful for resets and automated tests).
        /// </summary>
        public event Action OnEvidenceCleared;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning($"[EvidenceManager] Duplicate instance detected on GameObject '{gameObject.name}'. Destroying duplicate.", this);
                Destroy(gameObject);
                return;
            }

            Instance = this;

            if (persistAcrossScenes)
            {
                transform.SetParent(null);
                DontDestroyOnLoad(gameObject);
            }

            // Preload initial evidence if configured
            foreach (var item in initialEvidence)
            {
                if (item != null)
                {
                    TryCollectEvidence(item);
                }
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        /// <summary>
        /// Attempts to collect an evidence clue.
        /// Returns true if successfully collected; false if null, ID is empty, or duplicate.
        /// </summary>
        public bool TryCollectEvidence(EvidenceItem item)
        {
            if (item == null)
            {
                Debug.LogWarning("[EvidenceManager] TryCollectEvidence failed: EvidenceItem is null.", this);
                return false;
            }

            if (string.IsNullOrWhiteSpace(item.EvidenceId))
            {
                Debug.LogWarning($"[EvidenceManager] TryCollectEvidence failed: Item '{item.DisplayName}' has an empty or invalid EvidenceId.", this);
                return false;
            }

            if (collectedById.ContainsKey(item.EvidenceId))
            {
                Debug.Log($"[EvidenceManager] Evidence '{item.EvidenceId}' ({item.DisplayName}) has already been collected. Duplicate ignored.", this);
                return false;
            }

            collectedList.Add(item);
            collectedById.Add(item.EvidenceId, item);

            Debug.Log($"[EvidenceManager] Collected evidence: [{item.EvidenceId}] {item.DisplayName} (Category: {item.Category})", this);

            try
            {
                OnEvidenceCollected?.Invoke(item);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[EvidenceManager] Exception caught in OnEvidenceCollected callback: {ex}", this);
            }

            return true;
        }

        /// <summary>
        /// Checks if an evidence item with the given ID has been collected.
        /// </summary>
        public bool HasEvidence(string evidenceId)
        {
            if (string.IsNullOrWhiteSpace(evidenceId))
                return false;

            return collectedById.ContainsKey(evidenceId);
        }

        /// <summary>
        /// Retrieves a collected evidence item by ID, or null if not yet acquired.
        /// </summary>
        public EvidenceItem GetEvidence(string evidenceId)
        {
            if (string.IsNullOrWhiteSpace(evidenceId))
                return null;

            collectedById.TryGetValue(evidenceId, out var item);
            return item;
        }

        /// <summary>
        /// Returns a read-only list of all collected clues in the order they were acquired.
        /// </summary>
        public IReadOnlyList<EvidenceItem> GetCollectedEvidence()
        {
            return collectedList.AsReadOnly();
        }

        /// <summary>
        /// Total number of collected clues.
        /// </summary>
        public int CollectedCount => collectedList.Count;

        /// <summary>
        /// Clears all collected evidence.
        /// </summary>
        public void ClearCollectedEvidence()
        {
            collectedList.Clear();
            collectedById.Clear();
            OnEvidenceCleared?.Invoke();
        }
    }
}

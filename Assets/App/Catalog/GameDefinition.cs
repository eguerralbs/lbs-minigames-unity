using System.Collections.Generic;
using UnityEngine;

namespace Lbs.MiniGames.Catalog
{
    [CreateAssetMenu(menuName = "LBS Mini Games/Catalog/Game Definition", fileName = "GameDefinition")]
    public sealed class GameDefinition : ScriptableObject
    {
        [SerializeField] private string gameId;
        [SerializeField] private string visibleName;
        [SerializeField] private string hubSubjectLabel;
        [SerializeField] private GameCategory category;
        [SerializeField] private Sprite thumbnail;
        [SerializeField] private string sceneName;
        [SerializeField, TextArea] private string description;
        [SerializeField] private List<DifficultyDefinition> supportedDifficulties = new();
        [SerializeField] private DifficultyDefinition defaultDifficulty;
        // Show this game in the Hub. Test/legacy games keep their assets in the project but
        // are hidden from the catalog's UI by setting this false (default is true).
        [SerializeField] private bool visibleInHub = true;

        public string GameId => gameId;
        public string VisibleName => visibleName;
        public string HubSubjectLabel => hubSubjectLabel;
        public GameCategory Category => category;
        public Sprite Thumbnail => thumbnail;
        public string SceneName => sceneName;
        public string Description => description;
        public IReadOnlyList<DifficultyDefinition> SupportedDifficulties => supportedDifficulties;
        public DifficultyDefinition DefaultDifficulty => defaultDifficulty;
        public bool VisibleInHub => visibleInHub;

        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(gameId)
                && !string.IsNullOrWhiteSpace(visibleName)
                && category != null
                && !string.IsNullOrWhiteSpace(sceneName);
        }

        public bool IsValidWithDifficulties()
        {
            if (!IsValid()) return false;
            if (supportedDifficulties == null || supportedDifficulties.Count == 0) return true; // legacy fallback
            if (defaultDifficulty == null) return false;
            return supportedDifficulties.Contains(defaultDifficulty) && defaultDifficulty.IsValid();
        }

        public bool SupportsDifficulty(DifficultyDefinition difficulty)
        {
            if (difficulty == null) return false;
            return supportedDifficulties != null && supportedDifficulties.Contains(difficulty);
        }

        public DifficultyDefinition GetDefaultDifficulty()
        {
            if (defaultDifficulty != null && defaultDifficulty.IsValid()) return defaultDifficulty;
            if (supportedDifficulties != null)
            {
                foreach (DifficultyDefinition d in supportedDifficulties)
                {
                    if (d != null && d.IsValid()) return d;
                }
            }
            return null;
        }

#if UNITY_EDITOR
        public void Configure(
            string id,
            string name,
            GameCategory gameCategory,
            string targetSceneName,
            string gameDescription)
        {
            gameId = id;
            visibleName = name;
            category = gameCategory;
            sceneName = targetSceneName;
            description = gameDescription;
        }

        public void SetThumbnail(Sprite sprite)
        {
            thumbnail = sprite;
        }

        public void ConfigureDifficulties(List<DifficultyDefinition> difficulties, DifficultyDefinition defaultDiff)
        {
            supportedDifficulties = difficulties != null ? new List<DifficultyDefinition>(difficulties) : new List<DifficultyDefinition>();
            defaultDifficulty = defaultDiff;
        }

        public void SetVisibleInHub(bool visible)
        {
            visibleInHub = visible;
        }
#endif
    }
}

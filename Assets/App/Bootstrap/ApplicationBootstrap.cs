using Lbs.MiniGames.Catalog;
using Lbs.MiniGames.Navigation;
using Lbs.MiniGames.Shared.Audio;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Lbs.MiniGames.Bootstrap
{
    public sealed class ApplicationBootstrap : MonoBehaviour
    {
        [SerializeField] private GameCatalog catalog;
        [SerializeField] private string lobbySceneName = "Lobby";
        [SerializeField] private AppAudioConfig audioConfig;

        private AppServices services;
        private AppAudioService audioService;
        private readonly AppSceneConfigurationGate sceneConfiguration = new();

        public void SetCatalog(GameCatalog gameCatalog)
        {
            catalog = gameCatalog;
        }

        public void SetAudioConfig(AppAudioConfig config)
        {
            audioConfig = config;
        }

        private void Awake()
        {
            Application.targetFrameRate = 60;
            DontDestroyOnLoad(gameObject);

            EnsureAudioService();
            GameSession session = new();
            LevelSequenceController sequence = gameObject.GetComponent<LevelSequenceController>();
            if (sequence == null) sequence = gameObject.AddComponent<LevelSequenceController>();
            services = new AppServices(session, new GameLauncher(session, new UnitySceneLoader(), lobbySceneName, audioService, sequence), audioService, sequence);
            sequence.Configure(services, catalog);
            SceneManager.sceneLoaded += ConfigureLoadedScene;
            SceneManager.sceneUnloaded += ForgetUnloadedScene;
        }

        private void EnsureAudioService()
        {
            if (FindAnyObjectByType<AudioListener>() == null) gameObject.AddComponent<AudioListener>();
            audioService = GetComponent<AppAudioService>();
            if (audioService == null) audioService = gameObject.AddComponent<AppAudioService>();
            if (audioConfig == null)
            {
                // Runtime-safe transient fallback: does not mutate persisted assets.
                AudioClip fallbackMusic = Resources.Load<AudioClip>("ShapeAnalogy/Music/bg_cabinet_menu");
                if (fallbackMusic == null) fallbackMusic = Resources.Load<AudioClip>("ShapeAnalogy/Music/bg_puzzle_shell");
                if (fallbackMusic != null)
                {
                    AppAudioConfig fallback = AppAudioConfig.CreateRuntimeFallback(fallbackMusic, 0.25f, 0.125f);
                    audioConfig = fallback;
                    Debug.LogWarning("[Bootstrap] AppAudioConfig not assigned — using transient fallback music clip. Assign a persistent config asset to suppress this.", this);
                }
            }
            audioService.Initialize(audioConfig);
            // Bootstrap owns shared music playback; scenes only receive the shared audio service.
        }

        private void Start()
        {
            if (catalog == null)
            {
                Debug.LogError("Bootstrap requires a GameCatalog.", this);
                return;
            }

            services.GameLauncher.ShowLobby();
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= ConfigureLoadedScene;
            SceneManager.sceneUnloaded -= ForgetUnloadedScene;
        }

        private void ConfigureLoadedScene(Scene scene, LoadSceneMode mode)
        {
            sceneConfiguration.Configure(scene, services);
            if (scene.name == lobbySceneName)
            {
                audioService.PlayMusic(audioConfig?.HubMusic, true, audioConfig?.MusicVolume ?? 0.25f);
            }
            else if (LevelSequenceRoute.IsLogicSequenceGame(services.Session.CurrentRequest?.Game?.GameId)
                     && audioConfig != null
                     && audioConfig.GlobalMusic != null)
            {
                audioService.PlayMusic(audioConfig.GlobalMusic, true, audioConfig.MusicVolume);
            }
            else
            {
                audioService.StopMusic();
            }
        }

        private void ForgetUnloadedScene(Scene scene) => sceneConfiguration.Forget(scene);
    }
}

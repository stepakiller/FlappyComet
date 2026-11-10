using FlappyComet.Core;
using FlappyComet.Gameplay;
using FlappyComet.UI;
using UnityEngine;

namespace FlappyComet.Infrastructure
{
    [DefaultExecutionOrder(-100)]
    public class GameBootstrap : MonoBehaviour
    {
        [SerializeField] GameConfig config;

        [Header("Scene references")]
        [SerializeField] Camera gameCamera;
        [SerializeField] GameManager gameManager;
        [SerializeField] CometController comet;
        [SerializeField] CometTrail cometTrail;
        [SerializeField] ObstacleSpawner obstacles;
        [SerializeField] PickupSpawner pickups;
        [SerializeField] HudView hud;

        void Awake()
        {
            Application.targetFrameRate = 60;
            gameCamera.orthographicSize = config.cameraSize;

            gameManager.Init(config);
            comet.Init(gameManager, gameCamera);
            cometTrail.Init(gameManager);
            obstacles.Init(gameManager, gameCamera);
            pickups.Init(gameManager, gameCamera, comet, obstacles);
            hud.Init(gameManager);
        }
    }
}

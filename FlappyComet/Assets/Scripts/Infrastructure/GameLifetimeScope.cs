using FlappyComet.Core;
using FlappyComet.Gameplay;
using FlappyComet.UI;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace FlappyComet.Infrastructure
{
    public class GameLifetimeScope : LifetimeScope
    {
        [SerializeField] GameConfig config;

        [Header("Prefabs")]
        [SerializeField] Obstacle obstaclePrefab;
        [SerializeField] Pickup pickupPrefab;

        [Header("Scene")]
        [SerializeField] Camera gameCamera;
        [SerializeField] CometController comet;
        [SerializeField] CometTrail cometTrail;
        [SerializeField] HudView hud;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(config);
            builder.RegisterComponent(gameCamera);
            builder.Register<IInputService, GameInput>(Lifetime.Singleton);

            builder.RegisterEntryPoint<GameManager>().AsSelf();
            builder.RegisterEntryPoint<ObstacleSpawner>().WithParameter(obstaclePrefab);
            builder.RegisterEntryPoint<PickupSpawner>().WithParameter(pickupPrefab);

            builder.RegisterComponent(comet).AsImplementedInterfaces();
            builder.RegisterComponent(cometTrail);
            builder.RegisterComponent(hud);

            builder.RegisterBuildCallback(_ =>
            {
                Application.targetFrameRate = 60;
                gameCamera.orthographicSize = config.cameraSize;
            });
        }
    }
}

using UnityEngine;

namespace FlappyComet.Core
{
    [CreateAssetMenu(fileName = "GameConfig", menuName = "Flappy Comet/Game Config")]
    public class GameConfig : ScriptableObject
    {
        [Header("Play area")]
        public float cameraSize = 5f;
        public float barHeight = 0.9f;
        [Range(0.05f, 0.5f)] public float cometScreenX = 0.18f;

        [Header("Comet")]
        public float gravity = 20f;
        public float thrust = 42f;
        public float maxRiseSpeed = 10f;
        public float maxFallSpeed = 7f;
        public bool ceilingKills = true;
        public bool floorKills = true;

        [Header("Trail")]
        public float trailTime = 0.15f;

        [Header("Speed")]
        public float startSpeed = 15f;
        public float maxSpeed = 20f;
        public float acceleration = 0.2f;

        [Header("Obstacles")]
        public float firstObstacleDistance = 16f;
        public float minGap = 12f;
        public float maxGap = 25f;
        public float minObstacleSize = 0.8f;
        public float maxObstacleSize = 1.3f;

        [Header("Pickups")]
        public float firstPickupDistance = 18f;
        public float pickupMinGap = 9f;
        public float pickupMaxGap = 16f;
        public float pickupSize = 0.35f;
        public float magnetRadius = 2.2f;
        public float magnetSpeed = 22f;
        public float pickupSpinSpeed = 180f;

        [Header("Flow")]
        public float restartDelay = 0.4f;

        public float PlayTop => cameraSize - barHeight;
        public float PlayBottom => -cameraSize + barHeight;

        public float HalfWidth(float aspect) => cameraSize * aspect;
        public float CometX(float aspect) => -HalfWidth(aspect) + cometScreenX * HalfWidth(aspect) * 2f;
    }
}

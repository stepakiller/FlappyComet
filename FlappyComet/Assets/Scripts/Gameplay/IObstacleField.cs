using UnityEngine;

namespace FlappyComet.Gameplay
{
    public interface IObstacleField
    {
        float DistanceToNextSpawn { get; }
        bool Overlaps(Vector2 point, float radius);
    }
}

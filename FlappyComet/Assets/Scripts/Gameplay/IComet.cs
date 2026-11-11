using UnityEngine;

namespace FlappyComet.Gameplay
{
    public interface IComet
    {
        Vector2 Position { get; }
        float Radius { get; }
    }
}

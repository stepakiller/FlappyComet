using UnityEngine;

namespace FlappyComet.Gameplay
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class Pickup : MonoBehaviour
    {
        float _size;
        float _magnetSpeed;

        public bool Attracted { get; private set; }

        public void Place(Vector2 position, float size)
        {
            transform.position = position;
            transform.rotation = Quaternion.identity;
            transform.localScale = new Vector3(size, size, 1f);
            _size = size;
            _magnetSpeed = 0f;
            Attracted = false;
        }

        public void Spin(float degreesPerSecond, float dt) => transform.Rotate(0f, 0f, degreesPerSecond * dt);

        public bool PullTowards(Vector2 target, float targetSpeed, float magnetSpeed, float catchRadius, float dt)
        {
            Attracted = true;
            _magnetSpeed = Mathf.MoveTowards(_magnetSpeed, magnetSpeed, magnetSpeed * 4f * dt);

            Vector2 pos = transform.position;
            float dist = Vector2.Distance(pos, target);
            if (dist <= catchRadius) return true;

            float scale = _size * Mathf.Clamp(dist / (catchRadius * 4f), 0.35f, 1f);
            transform.localScale = new Vector3(scale, scale, 1f);

            float step = (targetSpeed + _magnetSpeed) * dt;
            transform.position = Vector2.MoveTowards(pos, target, step);
            return Vector2.Distance(transform.position, target) <= catchRadius;
        }
    }
}

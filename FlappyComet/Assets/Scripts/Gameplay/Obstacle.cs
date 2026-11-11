using UnityEngine;

namespace FlappyComet.Gameplay
{
    [RequireComponent(typeof(SpriteRenderer), typeof(BoxCollider2D), typeof(Rigidbody2D))]
    public class Obstacle : MonoBehaviour
    {
        Rigidbody2D _rb;

        public float HalfSize { get; private set; }

        void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
        }

        public void Place(Vector2 position, float size)
        {
            transform.position = position;
            _rb.position = position;
            transform.localScale = new Vector3(size, size, 1f);
            HalfSize = size * 0.5f;
            SetSpeed(0f);
        }

        public void SetSpeed(float speed)
        {
            var v = new Vector2(-speed, 0f);
#if UNITY_6000_0_OR_NEWER
            _rb.linearVelocity = v;
#else
            _rb.velocity = v;
#endif
        }
    }
}

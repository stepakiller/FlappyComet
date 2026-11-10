using FlappyComet.Core;
using UnityEngine;

namespace FlappyComet.Gameplay
{
    [RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D))]
    public class CometController : MonoBehaviour
    {
        GameManager _game;
        GameConfig _cfg;
        Camera _cam;
        Rigidbody2D _rb;
        CircleCollider2D _hitbox;
        float _vy;
        bool _held;

        public float Radius => _hitbox.radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.y);

        public void Init(GameManager game, Camera cam)
        {
            _game = game;
            _cfg = game.Config;
            _cam = cam;
            _rb = GetComponent<Rigidbody2D>();
            _hitbox = GetComponent<CircleCollider2D>();

            _game.StateChanged += OnStateChanged;
            ResetComet();
        }

        void OnDestroy()
        {
            if (_game != null) _game.StateChanged -= OnStateChanged;
        }

        void Update()
        {
            _held = GameInput.ThrustHeld();
        }

        void FixedUpdate()
        {
            if (_game == null) return;
            if (_game.State != GameState.Playing)
            {
                SetVelocity(Vector2.zero);
                return;
            }

            float dt = Time.fixedDeltaTime;
            float accel = _held ? _cfg.thrust : -_cfg.gravity;
            _vy = Mathf.Clamp(_vy + accel * dt, -_cfg.maxFallSpeed, _cfg.maxRiseSpeed);

            float top = _cfg.PlayTop - Radius;
            float bottom = _cfg.PlayBottom + Radius;
            float nextY = _rb.position.y + _vy * dt;

            if (nextY >= top)
            {
                if (_cfg.ceilingKills) { _game.GameOver(); return; }
                nextY = top;
                if (_vy > 0f) _vy = 0f;
            }
            else if (nextY <= bottom)
            {
                if (_cfg.floorKills) { _game.GameOver(); return; }
                nextY = bottom;
                if (_vy < 0f) _vy = 0f;
            }

            float x = _cfg.CometX(_cam.aspect);
            if (!Mathf.Approximately(_rb.position.x, x)) _rb.position = new Vector2(x, _rb.position.y);
            SetVelocity(new Vector2(0f, (nextY - _rb.position.y) / dt));
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponent<Obstacle>() != null) _game.GameOver();
        }

        void OnStateChanged(GameState state)
        {
            if (state == GameState.Ready) ResetComet();
            if (state == GameState.GameOver) SetVelocity(Vector2.zero);
        }

        void ResetComet()
        {
            _vy = 0f;
            SetVelocity(Vector2.zero);
            var start = new Vector2(_cfg.CometX(_cam.aspect), 0f);
            _rb.position = start;
            transform.position = start;
        }

        void SetVelocity(Vector2 v)
        {
#if UNITY_6000_0_OR_NEWER
            _rb.linearVelocity = v;
#else
            _rb.velocity = v;
#endif
        }
    }
}

using System.Collections.Generic;
using FlappyComet.Core;
using UnityEngine;
using VContainer;

namespace FlappyComet.Gameplay
{
    [RequireComponent(typeof(LineRenderer))]
    public class CometTrail : MonoBehaviour
    {
        struct Point
        {
            public Vector2 Position;
            public float Age;
        }

        GameManager _game;
        GameConfig _cfg;
        LineRenderer _line;
        readonly List<Point> _points = new List<Point>();
        Vector3[] _buffer = new Vector3[64];

        [Inject]
        public void Construct(GameManager game, GameConfig config)
        {
            _game = game;
            _cfg = config;
            _line = GetComponent<LineRenderer>();
            _line.useWorldSpace = true;
            _line.positionCount = 0;
            _game.StateChanged += OnStateChanged;
        }

        void OnDestroy()
        {
            if (_game != null) _game.StateChanged -= OnStateChanged;
        }

        void OnStateChanged(GameState state)
        {
            if (state == GameState.Ready)
            {
                _points.Clear();
                _line.positionCount = 0;
            }
        }

        void LateUpdate()
        {
            if (_game == null || _game.State != GameState.Playing) return;

            float dt = Time.deltaTime;
            float shift = _game.Speed * dt;

            for (int i = _points.Count - 1; i >= 0; i--)
            {
                Point p = _points[i];
                p.Position.x -= shift;
                p.Age += dt;
                if (p.Age > _cfg.trailTime) _points.RemoveAt(i);
                else _points[i] = p;
            }

            _points.Add(new Point { Position = transform.position, Age = 0f });
            Draw();
        }

        void Draw()
        {
            int n = _points.Count;
            if (_buffer.Length < n) _buffer = new Vector3[n * 2];
            for (int i = 0; i < n; i++) _buffer[i] = _points[i].Position;
            _line.positionCount = n;
            _line.SetPositions(_buffer);
        }
    }
}

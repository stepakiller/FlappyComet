using System.Collections.Generic;
using FlappyComet.Core;
using UnityEngine;
using UnityEngine.Pool;

namespace FlappyComet.Gameplay
{
    public class ObstacleSpawner : MonoBehaviour
    {
        GameManager _game;
        GameConfig _cfg;
        Camera _cam;
        [SerializeField] Obstacle prefab;

        ObjectPool<Obstacle> _pool;
        readonly List<Obstacle> _active = new List<Obstacle>();

        public float DistanceToNextSpawn { get; private set; }

        public void Init(GameManager game, Camera cam)
        {
            _game = game;
            _cfg = game.Config;
            _cam = cam;

            _pool = new ObjectPool<Obstacle>(
                createFunc: () => Instantiate(prefab, transform),
                actionOnGet: o => o.gameObject.SetActive(true),
                actionOnRelease: o => o.gameObject.SetActive(false),
                actionOnDestroy: o => Destroy(o.gameObject),
                collectionCheck: false, defaultCapacity: 16, maxSize: 64);

            _game.StateChanged += OnStateChanged;
            OnStateChanged(_game.State);
        }

        void OnDestroy()
        {
            if (_game != null) _game.StateChanged -= OnStateChanged;
        }

        void OnStateChanged(GameState state)
        {
            if (state == GameState.Ready) Clear();
            if (state == GameState.Playing) DistanceToNextSpawn = _cfg.firstObstacleDistance;
            if (state == GameState.GameOver)
                foreach (Obstacle o in _active) o.SetSpeed(0f);
        }

        void Update()
        {
            if (_game == null || _game.State != GameState.Playing) return;

            float halfWidth = _cfg.HalfWidth(_cam.aspect);
            float spawnX = halfWidth + _cfg.maxObstacleSize;
            float despawnX = -halfWidth - _cfg.maxObstacleSize;

            DistanceToNextSpawn -= _game.Speed * Time.deltaTime;
            if (DistanceToNextSpawn <= 0f)
            {
                Spawn(spawnX + DistanceToNextSpawn);
                DistanceToNextSpawn += Random.Range(_cfg.minGap, _cfg.maxGap);
            }

            for (int i = _active.Count - 1; i >= 0; i--)
            {
                Obstacle o = _active[i];
                o.SetSpeed(_game.Speed);
                if (o.transform.position.x < despawnX)
                {
                    _active.RemoveAt(i);
                    _pool.Release(o);
                }
            }
        }

        public bool Overlaps(Vector2 point, float radius)
        {
            foreach (Obstacle o in _active)
            {
                Vector2 c = o.transform.position;
                float reach = o.HalfSize + radius;
                if (Mathf.Abs(point.x - c.x) < reach && Mathf.Abs(point.y - c.y) < reach) return true;
            }
            return false;
        }

        void Spawn(float x)
        {
            float size = Random.Range(_cfg.minObstacleSize, _cfg.maxObstacleSize);
            float half = size * 0.5f;
            float y = Random.Range(_cfg.PlayBottom + half, _cfg.PlayTop - half);

            Obstacle o = _pool.Get();
            o.Place(new Vector2(x, y), size);
            o.SetSpeed(_game.Speed);
            _active.Add(o);
        }

        void Clear()
        {
            foreach (Obstacle o in _active) _pool.Release(o);
            _active.Clear();
        }
    }
}

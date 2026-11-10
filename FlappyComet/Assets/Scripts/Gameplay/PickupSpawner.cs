using System.Collections.Generic;
using FlappyComet.Core;
using UnityEngine;
using UnityEngine.Pool;

namespace FlappyComet.Gameplay
{
    public class PickupSpawner : MonoBehaviour
    {
        const int PlacementAttempts = 8;

        GameManager _game;
        GameConfig _cfg;
        Camera _cam;
        CometController _comet;
        ObstacleSpawner _obstacles;
        [SerializeField] Pickup prefab;

        ObjectPool<Pickup> _pool;
        readonly List<Pickup> _active = new List<Pickup>();
        float _distanceToNext;

        public void Init(GameManager game, Camera cam, CometController comet, ObstacleSpawner obstacles)
        {
            _game = game;
            _cfg = game.Config;
            _cam = cam;
            _comet = comet;
            _obstacles = obstacles;

            _pool = new ObjectPool<Pickup>(
                createFunc: () => Instantiate(prefab, transform),
                actionOnGet: p => p.gameObject.SetActive(true),
                actionOnRelease: p => p.gameObject.SetActive(false),
                actionOnDestroy: p => Destroy(p.gameObject),
                collectionCheck: false, defaultCapacity: 8, maxSize: 32);

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
            if (state == GameState.Playing) _distanceToNext = _cfg.firstPickupDistance;
        }

        void LateUpdate()
        {
            if (_game == null || _game.State != GameState.Playing) return;

            float dt = Time.deltaTime;
            float shift = _game.Speed * dt;
            float halfWidth = _cfg.HalfWidth(_cam.aspect);
            float spawnX = halfWidth + _cfg.maxObstacleSize;
            float despawnX = -halfWidth - 1f;

            _distanceToNext -= shift;
            if (_distanceToNext <= 0f && TrySpawn(spawnX + _distanceToNext))
                _distanceToNext += Random.Range(_cfg.pickupMinGap, _cfg.pickupMaxGap);

            Vector2 cometPos = _comet.transform.position;
            float catchRadius = _comet.Radius;

            for (int i = _active.Count - 1; i >= 0; i--)
            {
                Pickup p = _active[i];
                p.Spin(_cfg.pickupSpinSpeed, dt);

                bool inRange = p.Attracted || Vector2.Distance(p.transform.position, cometPos) <= _cfg.magnetRadius;
                if (inRange)
                {
                    if (p.PullTowards(cometPos, _game.Speed, _cfg.magnetSpeed, catchRadius, dt))
                    {
                        _game.AddPoint();
                        Remove(i);
                    }
                    continue;
                }

                p.transform.position += Vector3.left * shift;
                if (p.transform.position.x < despawnX) Remove(i);
            }
        }

        bool TrySpawn(float x)
        {
            float half = _cfg.pickupSize * 0.5f;
            float clearance = half + _comet.Radius;

            if (_obstacles.DistanceToNextSpawn < clearance + _cfg.maxObstacleSize) return false;

            float margin = _comet.Radius * 2f;
            float minY = _cfg.PlayBottom + margin + half;
            float maxY = _cfg.PlayTop - margin - half;

            for (int attempt = 0; attempt < PlacementAttempts; attempt++)
            {
                var pos = new Vector2(x, Random.Range(minY, maxY));
                if (_obstacles.Overlaps(pos, clearance)) continue;

                Pickup p = _pool.Get();
                p.Place(pos, _cfg.pickupSize);
                _active.Add(p);
                return true;
            }
            return false;
        }

        void Remove(int index)
        {
            Pickup p = _active[index];
            _active.RemoveAt(index);
            _pool.Release(p);
        }

        void Clear()
        {
            foreach (Pickup p in _active) _pool.Release(p);
            _active.Clear();
        }
    }
}

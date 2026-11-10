using System;
using System.Collections.Generic;
using FlappyComet.Core;
using UnityEngine;
using UnityEngine.Pool;
using VContainer.Unity;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace FlappyComet.Gameplay
{
    public class PickupSpawner : IStartable, ILateTickable, IDisposable
    {
        const int PlacementAttempts = 8;

        readonly GameManager _game;
        readonly GameConfig _cfg;
        readonly Camera _cam;
        readonly IComet _comet;
        readonly IObstacleField _obstacles;
        readonly Pickup _prefab;
        readonly List<Pickup> _active = new List<Pickup>();
        Transform _root;
        ObjectPool<Pickup> _pool;
        float _distanceToNext;

        public PickupSpawner(GameManager game, GameConfig config, Camera cam, IComet comet, IObstacleField obstacles, Pickup prefab)
        {
            _game = game;
            _cfg = config;
            _cam = cam;
            _comet = comet;
            _obstacles = obstacles;
            _prefab = prefab;
        }

        public void Start()
        {
            _root = new GameObject("Pickups").transform;
            _pool = new ObjectPool<Pickup>(
                createFunc: () => Object.Instantiate(_prefab, _root),
                actionOnGet: p => p.gameObject.SetActive(true),
                actionOnRelease: p => p.gameObject.SetActive(false),
                actionOnDestroy: p => Object.Destroy(p.gameObject),
                collectionCheck: false, defaultCapacity: 8, maxSize: 32);

            _game.StateChanged += OnStateChanged;
        }

        public void Dispose()
        {
            _game.StateChanged -= OnStateChanged;
        }

        void OnStateChanged(GameState state)
        {
            if (state == GameState.Ready) Clear();
            if (state == GameState.Playing) _distanceToNext = _cfg.firstPickupDistance;
        }

        public void LateTick()
        {
            if (_game.State != GameState.Playing) return;

            float dt = Time.deltaTime;
            float shift = _game.Speed * dt;
            float halfWidth = _cfg.HalfWidth(_cam.aspect);
            float spawnX = halfWidth + _cfg.maxObstacleSize;
            float despawnX = -halfWidth - 1f;

            _distanceToNext -= shift;
            if (_distanceToNext <= 0f && TrySpawn(spawnX + _distanceToNext))
                _distanceToNext += Random.Range(_cfg.pickupMinGap, _cfg.pickupMaxGap);

            Vector2 cometPos = _comet.Position;
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

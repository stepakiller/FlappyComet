using System;
using UnityEngine;

namespace FlappyComet.Core
{
    public enum GameState { Ready, Playing, GameOver }

    public class GameManager : MonoBehaviour
    {
        public GameConfig Config { get; private set; }
        public GameState State { get; private set; } = GameState.Ready;
        public int Score { get; private set; }
        public float Speed { get; private set; }

        public event Action<GameState> StateChanged;
        public event Action<int> ScoreChanged;

        float _restartTimer;

        public void Init(GameConfig config)
        {
            Config = config;
            Speed = config.startSpeed;
        }

        void Update()
        {
            if (Config == null) return;
            switch (State)
            {
                case GameState.Ready:
                    if (GameInput.FlapPressed()) SetState(GameState.Playing);
                    break;
                case GameState.Playing:
                    Speed = Mathf.Min(Config.maxSpeed, Speed + Config.acceleration * Time.deltaTime);
                    break;
                case GameState.GameOver:
                    _restartTimer -= Time.deltaTime;
                    if (_restartTimer <= 0f) ResetRun();
                    break;
            }
        }

        public void AddPoint()
        {
            if (State != GameState.Playing) return;
            Score++;
            ScoreChanged?.Invoke(Score);
        }

        public void GameOver()
        {
            if (State != GameState.Playing) return;
            _restartTimer = Config.restartDelay;
            SetState(GameState.GameOver);
        }

        void ResetRun()
        {
            Score = 0;
            Speed = Config.startSpeed;
            ScoreChanged?.Invoke(Score);
            SetState(GameState.Ready);
        }

        void SetState(GameState next)
        {
            State = next;
            StateChanged?.Invoke(next);
        }
    }
}

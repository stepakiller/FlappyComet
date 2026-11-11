using System;
using UnityEngine;
using VContainer.Unity;

namespace FlappyComet.Core
{
    public enum GameState { Ready, Playing, GameOver }

    public class GameManager : ITickable
    {
        readonly GameConfig _config;
        readonly IInputService _input;
        float _restartTimer;

        public GameState State { get; private set; } = GameState.Ready;
        public int Score { get; private set; }
        public float Speed { get; private set; }

        public event Action<GameState> StateChanged;
        public event Action<int> ScoreChanged;

        public GameManager(GameConfig config, IInputService input)
        {
            _config = config;
            _input = input;
            Speed = config.startSpeed;
        }

        public void Tick()
        {
            switch (State)
            {
                case GameState.Ready:
                    if (_input.StartPressed) SetState(GameState.Playing);
                    break;
                case GameState.Playing:
                    Speed = Mathf.Min(_config.maxSpeed, Speed + _config.acceleration * Time.deltaTime);
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
            _restartTimer = _config.restartDelay;
            SetState(GameState.GameOver);
        }

        void ResetRun()
        {
            Score = 0;
            Speed = _config.startSpeed;
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

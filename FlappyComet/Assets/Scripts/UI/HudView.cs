using FlappyComet.Core;
using UnityEngine;
using UnityEngine.UI;

namespace FlappyComet.UI
{
    public class HudView : MonoBehaviour
    {
        [SerializeField] Text score;
        [SerializeField] Text prompt;

        GameManager _game;

        public void Init(GameManager game)
        {
            _game = game;
            _game.ScoreChanged += OnScore;
            _game.StateChanged += OnState;
            OnScore(_game.Score);
            OnState(_game.State);
        }

        void OnDestroy()
        {
            if (_game == null) return;
            _game.ScoreChanged -= OnScore;
            _game.StateChanged -= OnState;
        }

        void OnScore(int value) => score.text = value.ToString();

        void OnState(GameState state)
        {
            prompt.gameObject.SetActive(state == GameState.Ready);
        }
    }
}

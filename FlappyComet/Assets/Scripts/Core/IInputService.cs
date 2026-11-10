namespace FlappyComet.Core
{
    public interface IInputService
    {
        bool StartPressed { get; }
        bool ThrustHeld { get; }
    }
}

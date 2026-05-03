namespace Gcc.Shared.System {
    public interface ILateUpdateSystem : ISystem {
        void OnLateUpdate(float deltaTime);
    }
}
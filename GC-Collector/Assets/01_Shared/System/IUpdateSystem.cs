namespace Gcc.Shared.System {
    public interface IUpdateSystem : ISystem {
        void OnUpdate(float deltaTime);
    }
}
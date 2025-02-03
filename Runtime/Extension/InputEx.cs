
namespace UnityEngine.EventSystems
{
    public static class InputEx
    {
        public static float GetScrollWheelDelta(this BaseInput input)
            => input.GetAxisRaw("Scroll Wheel");
        public static bool TryGetScrollWheelDelta(this BaseInput input, out float delta)
            => (delta = input.GetScrollWheelDelta()) != 0;
        public static Vector2 GetAxisRaw2D(this BaseInput input)
            => new Vector2(input.GetAxisRaw("Horizontal"), input.GetAxisRaw("Vertical"));
    } 
}
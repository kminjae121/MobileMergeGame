using System;

namespace Code.Core.Events.Bus
{
    public interface IEvent
    {
    }

    public static class Bus<T> where T : IEvent
    {
        public static event Action<T> OnEvent;

        public static void Raise(T evt) => OnEvent?.Invoke(evt);

        public static void Subscribe(Action<T> handler)
            => OnEvent += handler;

        public static void Unsubscribe(Action<T> handler)
            => OnEvent -= handler;
    }
}
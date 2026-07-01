using UnityEngine;

namespace _Code.Manager
{
    public enum PlayMode
    {
        Moving,
        Spawning,
    }
    public class GameManager : MonoSingleton<GameManager>
    {
        public PlayMode PlayMode { get; set; } = PlayMode.Spawning;
        
        public void SetPlayMode(PlayMode playMode)
            => PlayMode = playMode;
    }
}
using _Code.GunData;
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

        private string _name = string.Copy("Ay");
        
        public void SetPlayMode(PlayMode playMode)
            => PlayMode = playMode;

        public GunType GetMaxGunType => GunType.Launcher;
    }   
}
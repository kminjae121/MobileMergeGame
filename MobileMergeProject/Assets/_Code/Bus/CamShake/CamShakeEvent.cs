namespace Code.Core.Events.Bus
{
    public struct CamShakeEvent : IEvent
    {
        public float force;

        public CamShakeEvent(float force)
        {
            this.force = force;
        }
    }
}
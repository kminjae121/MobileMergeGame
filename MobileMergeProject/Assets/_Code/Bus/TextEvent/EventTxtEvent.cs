using _Code.SO;

namespace Code.Core.Events.Bus.TextEvent
{
    public struct EventTxtEvent : IEvent
    {
        public TextType TxtTypeType;

        public EventTxtEvent(TextType txtType)
        {
            this.TxtTypeType = txtType;
        }
    }
}
using UnityEngine;

namespace _Code.SO
{
    public enum TextType
    {
        Clear,
        Perfect,
        Double,
        Tripple
    }
    [CreateAssetMenu(fileName = "Event/Text", menuName = "Event/Text", order = 0)]
    public class TextSO : ScriptableObject
    {
        public TextType EventType;
        public string Text;
    }
}
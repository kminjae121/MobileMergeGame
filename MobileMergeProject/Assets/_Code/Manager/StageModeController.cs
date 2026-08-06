using _Code.Block;
using _Code.Field;
using _Code.Stage;
using UnityEngine;

namespace _Code.Manager
{
    public sealed class StageModeController : MonoBehaviour
    {
        private const string ScoreSuffix = "\uC810";
        private const string StageLabel = "\uC2A4\uD14C\uC774\uC9C0";
        private const string GoalLabel = "\uBAA9\uD45C";

        private StageDefinition _stageDefinition;

        public bool IsStageMode { get; private set; }
        public int TargetScore => IsStageMode ? _stageDefinition.TargetScore : 0;

        public void Initialize(BlockField blockField, GameObject owner)
        {
            IsStageMode = StageRunContext.TryGetSelectedStage(out _stageDefinition);

            if (!IsStageMode || blockField == null)
                return;

            int groupId = 10000 + _stageDefinition.Number * 100;

            foreach (Vector2Int point in _stageDefinition.StartingCells)
            {
                if (blockField.TryGetField(point, out _Code.Field.Field field))
                    field.SetObject(owner, Color.white, BlockBlastSpriteLibrary.GetRandomCatBlockSprite(), groupId++);
            }
        }

        public bool IsComplete(int score)
        {
            return IsStageMode && score >= _stageDefinition.TargetScore;
        }

        public string GetStartMessage()
        {
            if (!IsStageMode)
                return string.Empty;

            return $"{StageLabel} {_stageDefinition.Number} / {GoalLabel} {_stageDefinition.TargetScore}{ScoreSuffix}";
        }

        public string GetClearMessage()
        {
            if (!IsStageMode)
                return string.Empty;

            return $"{StageLabel} {_stageDefinition.Number} \uD074\uB9AC\uC5B4!";
        }
    }
}

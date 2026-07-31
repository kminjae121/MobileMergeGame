namespace _Code.Stage
{
    public static class StageRunContext
    {
        public static int SelectedStageNumber { get; private set; }
        public static bool IsStageMode => SelectedStageNumber > 0;

        public static void SelectInfiniteMode()
        {
            SelectedStageNumber = 0;
        }

        public static void SelectStage(int stageNumber)
        {
            if (stageNumber < 1)
                stageNumber = 1;
            else if (stageNumber > StageCatalog.MaxStage)
                stageNumber = StageCatalog.MaxStage;

            SelectedStageNumber = stageNumber;
        }

        public static bool TryGetSelectedStage(out StageDefinition stage)
        {
            return StageCatalog.TryGetStage(SelectedStageNumber, out stage);
        }
    }
}

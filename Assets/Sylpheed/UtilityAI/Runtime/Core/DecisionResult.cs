namespace Sylpheed.UtilityAI
{
    public class DecisionResult
    {
        public Decision Decision { get; set; }
        public bool Best { get; set; }
        public bool IsSameDecision { get; set; }
        public float WeightedScore { get; set; }
        
        public bool Skipped => Decision.Skipped;
        public bool Scored => Decision.Scored;
        public float Score => Decision.Score;
    }
}
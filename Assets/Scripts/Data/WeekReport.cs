using System.Collections.Generic;

/// <summary>Result of WeekSummary (S7), consumed by WeekEndUI (S11).</summary>
public class WeekReport
{
    public readonly string GeneralMessage;
    public readonly IReadOnlyList<StatLine> Lines;

    public WeekReport(string generalMessage, IReadOnlyList<StatLine> lines)
    {
        GeneralMessage = generalMessage;
        Lines = lines;
    }

    public readonly struct StatLine
    {
        public readonly StatId Stat;
        public readonly RegulationCategory Category;
        public readonly string Text;

        public StatLine(StatId stat, RegulationCategory category, string text)
        {
            Stat = stat;
            Category = category;
            Text = text;
        }
    }
}

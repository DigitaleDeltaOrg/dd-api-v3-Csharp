namespace DigitaleDelta;

public class TimeseriesMetadata
{
	public string?             TemporalExtent             { get; set; }
	public DateTimeOffset      BaseTime                   { get; set; }
	public string?             Spacing                    { get; set; }
	public CommentBlock?       CommentBlock               { get; set; }
	public List<CommentBlock>? CommentBlocks              { get; set; }
	public string?             IntendedObservationSpacing { get; set; }
	public Reference?          Status                     { get; set; }
	public bool?               Cumulative                 { get; set; }
	public DateTimeOffset?     AccumulationAnchorTime     { get; set; }
	public DateTimeOffset?     StartAnchorPoint           { get; set; }
	public DateTimeOffset?     EndAnchorPoint             { get; set; }
	public DateTimeOffset?     MaxGapPeriod               { get; set; }
}
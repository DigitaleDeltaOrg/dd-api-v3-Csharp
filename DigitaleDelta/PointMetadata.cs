namespace DigitaleDelta;

public class PointMetadata
{
	public Reference?      Quality             { get; set; }
	public Reference?      Uom                 { get; set; }
	public Reference?      InterpolationType   { get; set; }
	public Reference?      NilReason           { get; set; }
	public Reference?      CensoredReason      { get; set; }
	public string?         Comment             { get; set; }
	public Measure?        Accuracy            { get; set; }
	public Observation?    RelatedObservation  { get; set; }
	public DateTimeOffset? AggregationDuration { get; set; }
	public Reference?      Qualifier           { get; set; }
	public Reference?      Processing          { get; set; }
	public Reference?      Source              { get; set; }
}
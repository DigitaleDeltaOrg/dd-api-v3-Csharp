namespace DigitaleDelta;

using System.ComponentModel.DataAnnotations;

public class TimeseriesResult
{
	[Key]      public string              Id                   { get; set; } = null!;
	[Required] public string              Type                 { get; set; } = null!;
	public            TimeseriesMetadata? MetaData             { get; set; }
	public            PointMetadata?      DefaultPointMetaData { get; set; }
	[Required] public IList<PointData>    Points               { get; set; } = new List<PointData>();
}
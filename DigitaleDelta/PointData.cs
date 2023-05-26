namespace DigitaleDelta;

using System.ComponentModel.DataAnnotations;

public class PointData
{
	[Required] public DateTimeOffset Time     { get; set; } // not null
	[Required] public double         Value    { get; set; } // not null
	public            PointMetadata? MetaData { get; set; } // not null
}
namespace DigitaleDelta;

using System.ComponentModel;

public enum ObservationType
{
	[Description("measure")]    Measure,
	[Description("truth")]      Truth,
	[Description("count")]      Count,
	[Description("timeseries")] Timeseries,
	[Description("category")]   Category,
	[Description("text")]       Text,
	[Description("geometry")]   Geometry,
}
namespace DigitaleDelta;

using System.Runtime.Serialization;
using Microsoft.Spatial;

public class Result
{
	public                    string            Id         { get; set; } = null!;
	public                    bool?             Truth      { get; set; }
	public                    long?             Count      { get; set; }
	public                    Measure?          Measure    { get; set; }
	public                    CategoryVerb?     Vocab      { get; set; }
	public                    TimeseriesResult? Timeseries { get; set; }
	public                    GeometryPoint?    Geometry   { get; set; }
	[IgnoreDataMember] public object?           Complex    { get; set; }
}
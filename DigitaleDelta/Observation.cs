namespace DigitaleDelta;

using System.ComponentModel.DataAnnotations;
using Microsoft.OData.ModelBuilder;

public class Observation : IBaseResponseObject
{
	[Key]      public   string                    Id                  { get; set; } = null!;
	[Required] public   string                    Type                { get; set; } = null!;
	[Required] public   DateTimeOffset            ResultTime          { get; set; }
	public              DateTimeOffset?           PhenomenonTime      { get; set; }
	public              DateTimeOffset?           ValidTime           { get; set; }
	[AutoExpand] public ParameterReference?       Foi                 { get; set; }
	public              Parameter                 Parameter           { get; set; } = new();
	public              Metadata?                 Metadata            { get; set; }
	[AutoExpand] public Result                    Result              { get; set; } = new();
	public              List<RelatedObservation>? RelatedObservations { get; set; }
}

public class Parameter
{
	public IDictionary<string, object>? parameter { get; set; }
}

public class Metadata
{
	public IDictionary<string, object>? metadata { get; set; }
}

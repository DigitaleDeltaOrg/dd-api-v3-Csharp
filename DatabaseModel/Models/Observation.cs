namespace DatabaseModel.Models;

using System.Runtime.CompilerServices;
using NetTopologySuite.Geometries;

public class Observation
{
	public                  long?                         Count               { get; set; }
	public                  Guid                          Id                  { get; set; }
	public                  string                        ObservationType     { get; set; } = null!;
	public                  DateTime                      PhenomenonStart     { get; set; }
	public                  DateTime?                     PhenomenonEnd       { get; set; }
	public                  DateTime                      ResultTime          { get; set; }
	public                  Guid?                         FeatureOfInterestId { get; set; }
	public                  Guid?                         ResultUomId         { get; set; }
	public                  double?                       ResultMeasure       { get; set; }
	public                  bool?                         ResultTruth         { get; set; }
	public                  string?                       ResultTerm          { get; set; }
	public                  string?                       ResultVocab         { get; set; }
	public                  string?                       ResultTimeseries    { get; set; }
	public                  Geometry?                     ResultGeometry      { get; set; }
	public                  long?                         ResultCount         { get; set; }
	public                  DateTime                      ValidTimeStart      { get; set; }
	public                  DateTime?                     ValidTimeEnd        { get; set; }
	public                  string?                       ResultComplex       { get; set; }
	public                  string?                       ResultText          { get; set; }
	public                  Dictionary<Guid, Guid>?       Parameter           { get; set; }
	public                  Dictionary<string, string>?   Metadata            { get; set; }
	private static readonly NetTopologySuite.IO.WKTReader WktReader = new();

	public Observation()
	{
	}

	public Observation(Observation observation)
	{
		Id                  = observation.Id;
		ObservationType     = observation.ObservationType;
		PhenomenonStart     = observation.PhenomenonStart;
		PhenomenonEnd       = observation.PhenomenonEnd;
		ResultTime          = observation.ResultTime;
		FeatureOfInterestId = observation.FeatureOfInterestId;
		ValidTimeStart      = observation.ValidTimeStart;
		ValidTimeEnd        = observation.ValidTimeEnd;
	}

	public static Observation FromDynamic(dynamic item)
	{
		var observation = new Observation();
		observation.Count               = item.count;
		observation.Id                  = item.id;
		observation.ObservationType     = item.observation_type;
		observation.PhenomenonStart     = item.phenomenon_time_start;
		observation.PhenomenonEnd       = item.phenomenon_time_end;
		observation.ResultTime          = item.result_time;
		observation.FeatureOfInterestId = item.foi_id;
		observation.ResultUomId         = item.result_uom_id;
		observation.ResultMeasure       = item.result_measure;
		observation.ResultTruth         = item.result_truth;
		observation.ResultTerm          = item.result_term;
		observation.ResultVocab         = item.result_vocab;
		observation.ResultTimeseries    = item.result_timeseries;
		observation.ResultGeometry      = item.result_geometry == null ? null : WktReader.Read(item.result_geometry);
		observation.ResultCount         = item.result_count;
		observation.ValidTimeStart      = item.valid_time_start;
		observation.ValidTimeEnd        = item.valid_time_end;
		observation.ResultComplex       = item.result_complex;
		observation.ResultText          = item.result_text;
		observation.Parameter           = FromParameter(item.parameter);
		observation.Metadata            = FromMetadata(item.metadata);
		return observation;
	}

	private static Dictionary<Guid, Guid> FromParameter(dynamic o)
	{
		var result         = new Dictionary<Guid, Guid>();
		var databaseResult = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(o);
		databaseResult.Remove("$id");
		foreach (var item in databaseResult.Keys)
		{
			result.Add(Guid.Parse(item), Guid.Parse(databaseResult[item]));
		}

		return result;
	}
	
	private static Dictionary<string, string>? FromMetadata(dynamic o)
	{
		if (o == null)
		{
			return null;
		}
		
		var databaseResult = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(o);
		databaseResult.Remove("$id");
		
		return databaseResult;
	}
}
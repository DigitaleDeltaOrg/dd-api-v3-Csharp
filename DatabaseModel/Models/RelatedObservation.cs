namespace DatabaseModel.Models;
public class RelatedObservation : Observation
{
	public                  string                        Role                 { get; set; } = null!;
	public                  Guid                          RelatedObservationId { get; set; }
	private static readonly NetTopologySuite.IO.WKTReader WktReader = new();

	private RelatedObservation()
	{
	}

	public new static RelatedObservation FromDynamic(dynamic item)
	{
		var observation = new RelatedObservation();
		observation.Id                   = item.id;
		observation.ObservationType      = item.observation_type;
		observation.PhenomenonStart      = item.phenomenon_time_start;
		observation.PhenomenonEnd        = item.phenomenon_time_end;
		observation.ResultTime           = item.result_time;
		observation.FeatureOfInterestId  = item.foi_id;
		observation.ResultUomId          = item.result_uom_id;
		observation.ResultMeasure        = item.result_measure;
		observation.ResultTruth          = item.result_truth;
		observation.ResultTerm           = item.result_term;
		observation.ResultVocab          = item.result_vocab;
		observation.ResultTimeseries     = item.result_timeseries;
		observation.ResultGeometry       = item.result_geometry == null ? null : WktReader.Read(item.result_geometry);
		observation.ResultCount          = item.result_count;
		observation.ValidTimeStart       = item.valid_time_start;
		observation.ValidTimeEnd         = item.valid_time_end;
		observation.ResultComplex        = item.result_complex;
		observation.ResultText           = item.result_text;
		observation.Parameter            = DeserializeGuidDictionary(item.parameter);
		observation.Metadata             = DeserializeStringDictionary(item.metadata);
		observation.Role                 = item.role;
		observation.RelatedObservationId = item.related_observation_id;
		return observation;
	}
	
	private static Dictionary<Guid, Guid>? DeserializeGuidDictionary(string? json)
	{
		if (string.IsNullOrEmpty(json))
		{
			return null;
		}
		
		var dict    = new Dictionary<Guid, Guid>();
		var badDict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(json);
		if (badDict == null)
		{
			return dict;
		}
		
		foreach(var entry in badDict.Skip(1))
		{
			var key = Guid.TryParse(entry.Key, out var keyGuid) ? keyGuid : Guid.Empty;
			var value = Guid.TryParse(entry.Value, out var valueGuid) ? valueGuid : Guid.Empty;
			if (key != Guid.Empty && value != Guid.Empty)
			{
				dict.Add(key, value);
			}
			else
			{
				Console.WriteLine($"Failed to parse Guid: {entry}");
			}
		}
		return dict;
	}
	
	private static Dictionary<string, string>? DeserializeStringDictionary(string json)
	{
		if (string.IsNullOrEmpty(json))
		{
			return null;
		}
		
		var dict    = new Dictionary<string, string>();
		var badDict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(json);
		if (badDict == null)
		{
			return dict;
		}
		
		foreach(var entry in badDict.Skip(1))
		{
			dict.Add(entry.Key, entry.Value);
		}
		return dict;
	}
}
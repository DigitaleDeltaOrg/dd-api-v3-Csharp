namespace DigitaleDeltaExportFormats;

using DatabaseLayer.Utility;
using DatabaseModel.Models;
using DigitaleDelta;
using Microsoft.Spatial;
using NetTopologySuite.IO;
using Observation = DatabaseModel.Models.Observation;
using Reference = DatabaseModel.Models.Reference;
using RelatedObservation = DatabaseModel.Models.RelatedObservation;

public class DigitaleDeltaExport : IDigitaleDeltaExport<DigitaleDelta.Observation>
{
	private static readonly WKTReader WktReader = new();

	public async Task<IQueryable<DigitaleDelta.Observation>> GenerateExportDataAsync(List<Observation> observations, Dictionary<Guid, Reference> references, ILookup<Guid, RelatedObservation>? relatedObservations, 
		bool includeRelated, ILookup<Guid, GuidRole>? relatedReferences)
	{
		if (!observations.Any())
		{
			return new List<DigitaleDelta.Observation>().AsQueryable();
		}
		
		var result                   = new List<DigitaleDelta.Observation>(observations.Count);
		var referencesByExternalKey  = references.Values.ToDictionary(r => r.Id, r => r);
		
		result.AddRange(observations.Select(observation => DatabaseObservationToExportObservation(observation, referencesByExternalKey, relatedObservations, includeRelated, relatedReferences)));

		return await Task.FromResult(result.AsQueryable());
	}

	private static GeometryPoint? WktToGeometryPoint(string? wkt)
	{
		GeometryPoint? point = null;
		if (string.IsNullOrEmpty(wkt))
		{
			return point;
		}

		var geoPoint = WktReader.Read(wkt);
		point = GeometryPoint.Create(CoordinateSystem.Geometry(4258), geoPoint.Coordinate.X, geoPoint.Coordinate.Y, null, null);
		
		return point;
	}
	
	private static DigitaleDelta.Observation DatabaseObservationToExportObservation(Observation databaseObservation, IReadOnlyDictionary<Guid, Reference> referencesByExternalKey, ILookup<Guid, RelatedObservation>? relatedObservations, bool includeRelated, ILookup<Guid, GuidRole>? relatedReferences)
	{
		var resultObservation = new DigitaleDelta.Observation();
		resultObservation.Id                  = databaseObservation.Id.ToString();
		resultObservation.Type                = databaseObservation.ObservationType;
		resultObservation.ResultTime          = databaseObservation.ResultTime;
		resultObservation.PhenomenonTime      = databaseObservation.PhenomenonStart;
		resultObservation.ValidTime           = databaseObservation.ValidTimeStart;
		resultObservation.Foi                 = GetFoiById(databaseObservation.FeatureOfInterestId, referencesByExternalKey, null, relatedReferences);
		resultObservation.Parameter           = new Parameter { parameter = GetExportReferencesForParameter(databaseObservation.Parameter, referencesByExternalKey, relatedReferences) };
		resultObservation.Result              = GetExportForResult(databaseObservation, referencesByExternalKey);
		resultObservation.Metadata            = new Metadata { metadata = GetExportReferencesForMetadata(databaseObservation.Metadata) };
		resultObservation.RelatedObservations = new List<DigitaleDelta.RelatedObservation>();

		if (!includeRelated)
		{
			return resultObservation;
		}

		resultObservation.RelatedObservations = RelatedObservationsToExportObservation(databaseObservation.Id, referencesByExternalKey, relatedObservations, relatedReferences);
		
		return resultObservation;
	}

	private static List<DigitaleDelta.RelatedObservation>? RelatedObservationsToExportObservation(Guid observationId, IReadOnlyDictionary<Guid, Reference> referencesByExternalKey, ILookup<Guid, RelatedObservation>? relatedObservations, ILookup<Guid, GuidRole>? relatedReferences)
	{
		if (relatedObservations == null || !relatedObservations.Contains(observationId))
		{
			return null;
		}
		
		return relatedObservations[observationId].Select(a =>  new DigitaleDelta.RelatedObservation()
		{
			Id             = a.Id.ToString(),
			Type           = a.ObservationType,
			ResultTime     = a.ResultTime,
			PhenomenonTime = a.PhenomenonStart,
			ValidTime      = a.ValidTimeStart,
			Foi            = GetFoiById(a.FeatureOfInterestId, referencesByExternalKey, null, relatedReferences),
			Parameter      = new Parameter { parameter = GetExportReferencesForParameter(a.Parameter, referencesByExternalKey, relatedReferences) },
			Result         = GetExportForResult(a, referencesByExternalKey),
			Metadata       = new Metadata { metadata = GetExportReferencesForMetadata(a.Metadata) }
		}).ToList();
	}

	private static Result GetExportForResult(Observation observation, IReadOnlyDictionary<Guid, Reference> references)
	{
		var result = new Result
		{
			Id = observation.Id.ToString()
		};

		switch (observation.ObservationType)
		{
			case "measure":
				result.Measure = new Measure
				{
					Uom   = references[observation.ResultUomId!.Value].Code,
					Value = observation.ResultMeasure!.Value
				};
				break;
			case "geometry":
				var point = NtsGeometryToGeometryPoint(observation.ResultGeometry);
				result.Geometry = point;
				break;
			case "count":
				result.Count = observation.ResultCount!.Value;
				break;
			case "truth":
				result.Truth = observation.ResultTruth!.Value;
				break;
			case "term":
				result.Vocab = new CategoryVerb { Term = observation.ResultTerm!, Vocabulary = observation.ResultVocab! };
				break;
			case "timeseries":
				result.Timeseries = System.Text.Json.JsonSerializer.Deserialize<TimeseriesResult>(observation.ResultTimeseries!);
				break;
			case "complex":
				result.Complex = System.Text.Json.JsonSerializer.Deserialize<object>(observation.ResultComplex!);
				break;
		}

		return result;
	}

	private static Result GetExportForResult(RelatedObservation observation, IReadOnlyDictionary<Guid, Reference> references)
	{
		var result = new Result();

		switch (observation.ObservationType)
		{
			case "measure":
				result.Measure = new Measure
				{
					Uom   = references[observation.ResultUomId!.Value].Code,
					Value = observation.ResultMeasure!.Value
				};
				break;
			case "geography":
				var point = NtsGeometryToGeometryPoint(observation.ResultGeometry);
				result.Geometry = point;
				break;
			case "count":
				result.Count = observation.ResultCount!.Value;
				break;
			case "truth":
				result.Truth = observation.ResultTruth!.Value;
				break;
			case "term":
				result.Vocab = new CategoryVerb { Term = observation.ResultTerm!, Vocabulary = observation.ResultVocab! };
				break;
			case "timeseries":
				result.Timeseries = System.Text.Json.JsonSerializer.Deserialize<TimeseriesResult>(observation.ResultTimeseries!);
				break;
			case "complex":
				result.Complex = System.Text.Json.JsonSerializer.Deserialize<object>(observation.ResultComplex!);
				break;
		}

		return result;
	}

	private static IDictionary<string, object>? GetExportReferencesForMetadata(Dictionary<string, string>? databaseMetadata)
	{
		if (databaseMetadata == null)
		{
			return null;
		}

		var result = new Dictionary<string, object>();
		foreach (var (key, value) in databaseMetadata)
		{
			result.Add(key, value);
		}

		return result;
	}
	
	private static IDictionary<string, object>? GetExportReferencesForParameter(Dictionary<Guid, Guid>? databaseObservationParameter, IReadOnlyDictionary<Guid, Reference> referencesByExternalKey, ILookup<Guid, GuidRole>? relatedReferences)
	{
		if (databaseObservationParameter == null || databaseObservationParameter.Count == 0)
		{
			return null;
		}
		
		var dictionary = new Dictionary<string, object>();
		foreach (var parameter in databaseObservationParameter)
		{
			var reference = GetExportReference(parameter.Value, referencesByExternalKey, relatedReferences);
			if (reference == null)
			{
				continue;
			}

			var item = GetParameterReferenceById(parameter.Value, referencesByExternalKey, parameter.Key, relatedReferences);
			if (item != null)
			{
				var key = GetCodeFromReferenceByExternalKey(parameter.Key, referencesByExternalKey);
				if (key != null)
				{
					dictionary.Add(key, item.Code ?? string.Empty);
				}
			}
			
			if (reference.TaxonType != null)
			{
				dictionary.Add("taxontype", reference.TaxonType);
			}
			
			if (reference.TaxonGroup != null)
			{
				dictionary.Add("taxongroup", reference.TaxonGroup);
			}
		}

		return dictionary;
	}

	
	private static ParameterReference? GetExportReference(Guid? id, IReadOnlyDictionary<Guid, Reference> referencesByExternalKey, ILookup<Guid, GuidRole>? relatedReferences)
	{
		return id == null ? null : GetParameterReferenceById(id.Value, referencesByExternalKey, null, relatedReferences);
	}

	private static string? GetCodeFromReferenceByExternalKey(Guid? id, IReadOnlyDictionary<Guid, Reference> references)
	{
		var parameterCode = id == null ? null : ReferenceToExportReferenceByExternalKey(id, references)?.Code;
		return parameterCode == null? null : Definitions.ParameterTranslators.TryGetValue(parameterCode, out var translated) ? translated : parameterCode;
	}
	
	private static DigitaleDelta.Reference? ReferenceToExportReferenceByExternalKey(Guid? externalKey, IReadOnlyDictionary<Guid, Reference> referencesByExternalKey)
	{
		if (externalKey == null || !referencesByExternalKey.ContainsKey(externalKey.Value))
		{
			return null;
		}
		
		var exportReference = new DigitaleDelta.Reference
		{
			Id            = referencesByExternalKey[externalKey.Value].Id.ToString(),
			Type          = referencesByExternalKey[externalKey.Value].Description,
			Organisation  = referencesByExternalKey[externalKey.Value].Organisation ?? string.Empty,
			Code          = referencesByExternalKey[externalKey.Value].Code,
			Href          = referencesByExternalKey[externalKey.Value].Uri ?? string.Empty,
			Description   = referencesByExternalKey[externalKey.Value].Description,
			ExternalKey   = referencesByExternalKey[externalKey.Value].Id.ToString(),
			TaxonRank     = referencesByExternalKey[externalKey.Value].TaxonRank,
			TaxonAuthors  = referencesByExternalKey[externalKey.Value].TaxonAuthor, 
			TaxonNameNl   = referencesByExternalKey[externalKey.Value].TaxonNameNL, 
			ParameterType = referencesByExternalKey[externalKey.Value].ParameterType,
			Geometry      = WktToGeometryPoint(referencesByExternalKey[externalKey.Value].Geometry)
		};

		return exportReference;
	}
	
	private static GeometryPoint? NtsGeometryToGeometryPoint(NetTopologySuite.Geometries.Geometry? geometry)
	{
		return geometry == null ? null : GeometryPoint.Create(CoordinateSystem.Geometry(4258), geometry.Centroid.Coordinate.X, geometry.Centroid.Coordinate.Y, null, null);
	}
	
	private static ParameterReference? GetParameterReferenceById(Guid? id, IReadOnlyDictionary<Guid, Reference> referencesByExternalKey, Guid? role, ILookup<Guid, GuidRole>? relatedReferences)
	{
		if (id == null || id.Value == Guid.Empty || relatedReferences == null)
		{
			return null;
		}
		
		if (role != null && !referencesByExternalKey.ContainsKey(role.Value))
		{
			return null;
		}
		
		var reference     = referencesByExternalKey[id.Value];
		var taxonTypeId   = relatedReferences[id.Value].FirstOrDefault(a => a.Role == "taxontype")?.Guid;
		var taxonGroupId  = relatedReferences[id.Value].FirstOrDefault(a => a.Role == "taxongroup")?.Guid;
		var taxonParentId = relatedReferences[id.Value].FirstOrDefault(a => a.Role == "taxonparent")?.Guid;
		var byId      = new ParameterReference
		{
			Id              = reference.Id.ToString(),
			Type            = GetCodeFromReferenceByExternalKey(reference.ReferenceTypeId, referencesByExternalKey),
			Organisation    = reference.Organisation,
			Description     = reference.Description,
			Code            = reference.Code,
			Role            = GetCodeFromReferenceByExternalKey(role, referencesByExternalKey),
			TaxonRank       = reference.TaxonRank,
			TaxonAuthors    = reference.TaxonAuthor,
			TaxonNameNl     = reference.TaxonNameNL,
			ParameterType   = reference.ParameterType,
			TaxonStatusCode = reference.TaxonStatusCode,
			CasNumber       = reference.CasNumber,
			TaxonType       = GetCodeFromReferenceByExternalKey(taxonTypeId, referencesByExternalKey),
			TaxonGroup      = GetCodeFromReferenceByExternalKey(taxonGroupId, referencesByExternalKey),
			TaxonParent     = GetCodeFromReferenceByExternalKey(taxonParentId, referencesByExternalKey),
			TaxonTypeId     = taxonTypeId == Guid.Empty ? null: taxonTypeId.ToString(),
			TaxonGroupId    = taxonGroupId == Guid.Empty ? null: taxonGroupId.ToString(),
			TaxonParentId   = taxonParentId == Guid.Empty ? null: taxonParentId.ToString()
		};

		return byId;
	}
	
	private static ParameterReference? GetFoiById(Guid? id, IReadOnlyDictionary<Guid, Reference> referencesByExternalKey, Guid? role, ILookup<Guid, GuidRole>? relatedReferences)
	{
		if (id == null || id.Value == Guid.Empty || relatedReferences == null)
		{
			return null;
		}
		
		if (role != null && !referencesByExternalKey.ContainsKey(role.Value))
		{
			return null;
		}
		
		var reference     = referencesByExternalKey[id.Value];
		var byId      = new ParameterReference
		{
			Id              = reference.Id.ToString(),
			Type            = GetCodeFromReferenceByExternalKey(reference.ReferenceTypeId, referencesByExternalKey),
			Organisation    = reference.Organisation,
			Description     = reference.Description,
			Code            = reference.Code,
			Role            = GetCodeFromReferenceByExternalKey(role, referencesByExternalKey),
			Geometry        = WktToGeometryPoint(reference.Geometry)
		};

		return byId;
	}
}
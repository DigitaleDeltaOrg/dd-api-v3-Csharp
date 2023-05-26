namespace DatabaseLayer.Services;

using Microsoft.OData.UriParser;
using Dapper;
using Dapper.Json;
using DatabaseModel.Models;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.Extensions.Caching.Memory;
using Npgsql;
using Utility;
using Utility.Models;

public class ObservationDataLayerService
{
	private readonly NpgsqlConnection  _connection;
	private readonly FilterFieldMapper _filterFieldMapper;
	private readonly IMemoryCache      _memoryCache;
	private readonly OrderByMapper     _orderByMapper;

	public ObservationDataLayerService(NpgsqlConnection connection, IMemoryCache memoryCache, ILookup<Guid, Reference> references)
	{
		_connection  = connection;
		_memoryCache = memoryCache;
		var mappings = new List<FieldMap?>
		{
			new("Id", "id", MapTypeEnum.Value, false),
			new("Type", "observation_type", MapTypeEnum.Value, false),
			new("ResultTime", "result_time", MapTypeEnum.Value, false),
			new("PhenomenonTime", "phenomenon_time_start", MapTypeEnum.Value, false),
			new("ValidTime", "valid_time", MapTypeEnum.Value, false),
			new("Foi/Code", "foi_id", MapTypeEnum.Reference, true, "foi_id in (select id from reference where code"),
			new("Foi/Geometry", "foi_id", MapTypeEnum.Reference, true, "foi_id in (select id from reference where geometry"),
			new("Foi/Description", "foi_id", MapTypeEnum.Reference, true, "foi_id in (select id from reference where description"),
			new("Result/Truth", "result_truth", MapTypeEnum.Value, false),
			new("Result/Count", "result_count", MapTypeEnum.Value, false),
			new("Result/Measure/Uom/Code", "result_uom_id", MapTypeEnum.Reference, true, "result_uom_id in (select id from reference where code"),
			new("Parameter/Type", "id", MapTypeEnum.Reference, true, $"cast(parameter->>'{Definitions.Parameter}' as uuid) in (select id from reference where parameter_type"),
			new("Parameter/Taxontype", "id", MapTypeEnum.Reference, true, $"cast(parameter->>'{Definitions.TaxonType}' as uuid) in (select id from reference where taxon_type"),
			new("Parameter/Taxongroup", "id", MapTypeEnum.Reference, true, $"cast(parameter->>'{Definitions.TaxonGroup}' as uuid) in (select id from reference where taxon_group"),
			new("Parameter/Organisation", "id", MapTypeEnum.Reference, true, $"cast(parameter->>'{Definitions.Organisation}' as uuid) in (select id from reference where organisation"),
			new("Parameter/lifeform", "id", MapTypeEnum.Reference, true, $"cast(parameter->>'{references[Definitions.AttributeType].FirstOrDefault(a => a.Code == "LV")?.Id}' as uuid) in (select id from reference where code"),
			new("Parameter/Gender", "id", MapTypeEnum.Reference, true, $"cast(parameter->>'{references[Definitions.AttributeType].FirstOrDefault(a => a.Code == "GS")?.Id}' as uuid) in (select id from reference where code"),
			new("Parameter/Appearance", "id", MapTypeEnum.Reference, true, $"cast(parameter->>'{references[Definitions.AttributeType].FirstOrDefault(a => a.Code == "VV")?.Id}' as uuid) in (select id from reference where code"),
			new("Parameter/Compartment", "id", MapTypeEnum.Reference, true, $"cast(parameter->>'{Definitions.Compartment}' as uuid) in (select id from reference where code"),
			new("Parameter/Measurementpackage", "id", MapTypeEnum.Reference, true, $"cast(parameter->>'{Definitions.MeasurementPackage}' as uuid) in (select id from reference where code"),
			new("Parameter/Analysispackage", "id", MapTypeEnum.Reference, true, $"cast(parameter->>'{Definitions.AnalysisPackage}' as uuid) in (select id from reference where code"),
			new("Parameter/Condition", "id", MapTypeEnum.Reference, true, $"cast(parameter->>'{references[Definitions.AttributeType].FirstOrDefault(a => a.Code == "CD")?.Id}' as uuid) in (select id from reference where code"),
			new("Parameter/Quality", "id", MapTypeEnum.Reference, true, $"cast(parameter->>'{references[Definitions.AttributeType].FirstOrDefault(a => a.Code == "HD")?.Id}' as uuid) in (select id from reference where code"),
			new("Parameter/Habitat", "id", MapTypeEnum.Reference, true, $"cast(parameter->>'{references[Definitions.AttributeType].FirstOrDefault(a => a.Code == "HT")?.Id}' as uuid) in (select id from reference where code"),
			new("Parameter/Individuals", "id", MapTypeEnum.Reference, true, $"cast(parameter->>'{references[Definitions.AttributeType].FirstOrDefault(a => a.Code == "ID")?.Id}' as uuid) in (select id from reference where code"),
			new("Parameter/Graindiameter", "id", MapTypeEnum.Reference, true, $"cast(parameter->>'{references[Definitions.AttributeType].FirstOrDefault(a => a.Code == "KD")?.Id}' as uuid) in (select id from reference where code"),
			new("Parameter/Grainsizefraction", "id", MapTypeEnum.Reference, true, $"cast(parameter->>'{references[Definitions.AttributeType].FirstOrDefault(a => a.Code == "KG")?.Id}' as uuid) in (select id from reference where code"),
			new("Parameter/Qualityassessment", "id", MapTypeEnum.Reference, true, $"cast(parameter->>'{references[Definitions.AttributeType].FirstOrDefault(a => a.Code == "KO")?.Id}' as uuid) in (select id from reference where code"),
			new("Parameter/Lengthclass", "id", MapTypeEnum.Reference, true, $"cast(parameter->>'{references[Definitions.AttributeType].FirstOrDefault(a => a.Code == "LK")?.Id}' as uuid) in (select id from reference where code"),
			new("Parameter/Measurementposition", "id", MapTypeEnum.Reference, true, $"cast(parameter->>'{references[Definitions.AttributeType].FirstOrDefault(a => a.Code == "MP")?.Id}' as uuid) in (select id from reference where code"),
			new("Parameter/Sediment", "id", MapTypeEnum.Reference, true, $"cast(parameter->>'{references[Definitions.AttributeType].FirstOrDefault(a => a.Code == "SD")?.Id}' as uuid) in (select id from reference where code"),
			new("Parameter/Statistic", "id", MapTypeEnum.Reference, true, $"cast(parameter->>'{references[Definitions.AttributeType].FirstOrDefault(a => a.Code == "ST")?.Id}' as uuid) in (select id from reference where code"),
			new("Parameter/Valuationtechnique", "id", MapTypeEnum.Reference, true, $"cast(parameter->>'{references[Definitions.AttributeType].FirstOrDefault(a => a.Code == "WT")?.Id}' as uuid) in (select id from reference where code"),
			new("Parameter/Widthclasscm", "id", MapTypeEnum.Reference, true, $"cast(parameter->>'{references[Definitions.AttributeType].FirstOrDefault(a => a.Code == "MB.CM")?.Id}' as uuid) in (select id from reference where code"),
			new("Parameter/Widthclassmm", "id", MapTypeEnum.Reference, true, $"cast(parameter->>'{references[Definitions.AttributeType].FirstOrDefault(a => a.Code == "MB.MM")?.Id}' as uuid) in (select id from reference where code"),
			new("Parameter/Wengthclasscm", "id", MapTypeEnum.Reference, true, $"cast(parameter->>'{references[Definitions.AttributeType].FirstOrDefault(a => a.Code == "ML.CM")?.Id}' as uuid) in (select id from reference where code"),
			new("Parameter/Wengthclassmm", "id", MapTypeEnum.Reference, true, $"cast(parameter->>'{references[Definitions.AttributeType].FirstOrDefault(a => a.Code == "ML.MM")?.Id}' as uuid) in (select id from reference where code"),
			new("Parameter/Quantity", "id", MapTypeEnum.Reference, true, $"cast(parameter->>'{Definitions.Quantity}' as uuid) in (select id from reference where code")
		}.ToDictionary(a => a?.FieldName ?? string.Empty, a => a);
		// Initialize the ODataToSqlMapper
		_filterFieldMapper = new FilterFieldMapper(mappings);
		var orderMap = new Dictionary<string, string>
		{
			{ "Id", "external_key" },
			{ "Type", "reference_type" },
			{ "Code", "code" },
			{ "Organisation", "organisation" },
			{ "Geometry", "geometry" },
			{ "TaxonType/Code", "taxon_type" },
			{ "TaxonGroup/Code", "taxon_group" },
			{ "TaxonStatusCode", "taxon_status_code" },
			{ "ParameterType", "parameter_type" },
			{ "Description", "description" },
			{ "TaxonRank", "taxon_rank" },
			{ "TaxonAuthor", "taxon_author" },
			{ "CasNumber", "cas_number" },
			{ "TaxonNameNl", "taxon_name_nl" },
		};
		_orderByMapper = new OrderByMapper(orderMap);
	}

	public async Task<(bool moreData, long? count, List<Observation> data, List<RelatedObservation>? relatedObservations)> GetDataAsync(
		FilterClause? filterClause, SkipQueryOption? skipQueryOption, TopQueryOption? top, int maxPageSize, OrderByQueryOption? orderByQueryOption, CountQueryOption? count)
	{
		var whereStatement = filterClause == null ? " 1 = 1 " : _filterFieldMapper.TranslateFilterClause(filterClause);
		var take           = top == null ? maxPageSize : Math.Min(top.Value, maxPageSize);
		var topClause      = $" {take + 1} ";
		var orderByClause  = _orderByMapper.CreateOrderByClause(orderByQueryOption);
		var query = @$"
			SELECT id, observation_type, phenomenon_time_start, phenomenon_time_end, valid_time_start, valid_time_end, result_uom_id, ST_AsText(result_geometry) as result_geometry, 
						 result_count, result_measure, result_term, result_vocab, result_timeseries, result_complex, parameter, metadata, result_time, foi_id, result_truth, result_text,
						 array_to_string(array(SELECT related_observation_id FROM observation_related_observation WHERE observation_related_observation.related_observation_id = id), ',') as related
			FROM observation 
			WHERE {whereStatement} 
			ORDER BY {orderByClause}
			OFFSET {skipQueryOption?.Value ?? 0}
			LIMIT {topClause}
			";
		var countQuery = @$"
			SELECT count(*)
			FROM observation 
			WHERE {whereStatement} 
			";
		var key = filterClause.AsJson();
		// Count takes a long time. Cache the count for the specified query, without taking skip and top into account.
		var recordCount = 0;
		if ((count?.Value ?? true))
		{
			recordCount = await _memoryCache.GetOrCreateAsync(key, async _ =>
			{
				_.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
				return await _connection.QuerySingleAsync<int>(countQuery).ConfigureAwait(false);
			});
		}
		
		var data        = (await _connection.QueryAsync<dynamic>(query).ConfigureAwait(false)).Select(Observation.FromDynamic).ToList();
		if ((count?.Value ?? false) == false)
		{
			recordCount = data.Count;
		}
		return (recordCount > take, recordCount, data.Take(take).ToList(), null);
	}
	
	public async Task<ILookup<Guid, RelatedObservation>> GetRelatedObservationsOfAsync(List<Guid> uuids)
	{
		var       result   = new List<RelatedObservation>(uuids.Count * 2);
		var       offset   = 0;
		const int pageSize = 10000;
		while (offset < uuids.Count)
		{
			var query = @$"
				SELECT id, observation_type, phenomenon_time_start, phenomenon_time_end, valid_time_start, valid_time_end, result_uom_id, ST_AsText(result_geometry) as result_geometry, 
							 result_count, result_measure, result_term, result_vocab, result_timeseries, result_complex, parameter, metadata, result_time, foi_id, result_truth, result_text, related_observation_id, role
				FROM observation, LATERAL(SELECT related_observation_id, role FROM observation_related_observation WHERE observation_id = id) AS related
				WHERE id in ({string.Join(',', uuids.Skip(offset).Take(pageSize).Select(a => "'" + a + "'").ToList())}) 
			";

			result.AddRange((await _connection.QueryAsync<dynamic>(query).ConfigureAwait(false)).Select(RelatedObservation.FromDynamic));
			offset += pageSize;
		}

		return result.ToLookup(a => a.Id, a => a);
	}
}


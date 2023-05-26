namespace DatabaseLayer.Services;

using Dapper;
using Dapper.Json;
using DatabaseModel.Models;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.OData.UriParser;
using NetTopologySuite.Index.HPRtree;
using Npgsql;
using Utility;
using Utility.Models;

public class ReferenceDataLayerService
{
	private readonly NpgsqlConnection  _connection;
	private readonly FilterFieldMapper _filterFieldMapper;
	private readonly IMemoryCache      _memoryCache;
	private const    string            SelectFields = @"id, reference_type_id, organisation, code, uri, ST_AsText(geometry) AS geometry, display, description, taxon_rank, taxon_author, cas_number, parameter_type, taxon_name_nl, taxon_status_code";
	private readonly OrderByMapper     _orderByMapper;
	
	public ReferenceDataLayerService(NpgsqlConnection connection, IMemoryCache memoryCache)
	{
		_connection  = connection;
		_memoryCache = memoryCache;
		var filterMappings = new List<FieldMap?> // TODO: Remap and move to configuration.
		{
			new("Id", "id", MapTypeEnum.Value, false),
			new("Type", "reference_type_id", MapTypeEnum.Reference, true, "reference_type_id in (select id from reference where reference_type_id is null and code "),
			new("Code", "code", MapTypeEnum.Value, false),
			new("Organisation", "organisation", MapTypeEnum.Value, false),
			new("Geometry", "geometry", MapTypeEnum.Value, false),
			new("TaxonType/Code", "taxon_type_id", MapTypeEnum.Reference, true, "taxon_type_id in (select id from reference where reference_type_id is not null and code "),
			new("TaxonGroup/Code", "taxon_group_id", MapTypeEnum.Reference, true, "taxon_group_id in (select id from reference where reference_type_id is not null and code "),
			new("TaxonStatusCode", "taxon_status_code", MapTypeEnum.Value, false),
			new("ParameterType", "parameter_type", MapTypeEnum.Value, false),
			new("Description", "description", MapTypeEnum.Value, false),
			new("TaxonRank", "taxon_rank", MapTypeEnum.Value, false),
			new("TaxonAuthor", "taxon_author", MapTypeEnum.Value, false),
			new("CasNumber", "cas_number", MapTypeEnum.Value, false),
			new("TaxonNameNl", "taxon_name_nl", MapTypeEnum.Value, false),
		}.ToDictionary(a => a?.FieldName ?? string.Empty, a => a);
		_filterFieldMapper = new FilterFieldMapper(filterMappings);
		var orderMap = new Dictionary<string, string>
		{
			{ "Id", "id" },
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

	public async Task<Dictionary<Guid, Reference>> GetReferencesByIdAsync()
	{
		var dynamicReferences = await _connection.QueryAsync<dynamic>($@"select {SelectFields} from reference ");
		var references        = dynamicReferences.Select(Reference.FromDynamic).ToDictionary(item => item.Id, item => item);
		return references;
	}

	
	public async Task<ILookup<Guid, Reference>> GetReferencesByTypeAndCodeFromCacheAsync()
	{
		var dynamicReferences = await _connection.QueryAsync<dynamic>($@"select {SelectFields} from reference where reference_type_id is not null");
		var references        = dynamicReferences.Select(Reference.FromDynamic).ToLookup(item => item.ReferenceTypeId!.Value, item => item);
		return references;
	}
	
	public async Task<ILookup<Guid, GuidRole>> GetReferenceRelationsAsync()
	{
		var dynamicReferences = await _connection.QueryAsync<dynamic>($@"select * from reference_related_reference ");
		var references        = dynamicReferences.Select(ReferenceRelatedReference.FromDynamic).ToLookup(item => item.ReferenceId, item => new GuidRole(item.RelatedReferenceId, item.Role ));
		return references;
	}
	
	public async Task<(bool moreData, long? count, List<Reference> data)> GetDataAsync(FilterClause? filterClause, SkipQueryOption? skipQueryOption, TopQueryOption? topQueryOption, int maxPageSize, OrderByQueryOption? orderByQueryOption)
	{
		var whereStatement = filterClause == null ? " 1 = 1 " : _filterFieldMapper.TranslateFilterClause(filterClause); 
		var take           = topQueryOption == null ? maxPageSize : Math.Min(topQueryOption.Value, maxPageSize);
		var topClause      = $" {take} ";
		var orderByClause  = _orderByMapper.CreateOrderByClause(orderByQueryOption);
		var dataQuery          = @$"
			SELECT 
				id, reference_type_id, organisation, reference.code, uri, ST_AsText(geometry) AS geometry, display, description, taxon_rank, taxon_author, cas_number, 
				parameter_type, taxon_name_nl, taxon_status_code, reference_type.code AS reference_type
			FROM reference
			LEFT JOIN LATERAL (SELECT code FROM reference AS reference_type WHERE reference.reference_type_id = reference_type.id FETCH FIRST 1 ROW ONLY) reference_type ON true
			WHERE {whereStatement} 
			ORDER BY {orderByClause}
			OFFSET {skipQueryOption?.Value ?? 0}
			LIMIT {topClause}
			";
		var countQuery = @$"
			SELECT count(*)
			FROM reference 
			WHERE {whereStatement} 
			";
		var key           = filterClause.AsJson();
		// Count takes a long time. Cache the count for the specified query, without taking skip and topQueryOption into account.
		var recordCount = await _memoryCache.GetOrCreateAsync(key, async _ => { _.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5); return await _connection.QuerySingleAsync<int>(countQuery).ConfigureAwait(false); });
		var data        = (await _connection.QueryAsync<dynamic>(dataQuery).ConfigureAwait(false)).Select(Reference.FromDynamic).ToList();
		return (recordCount > take, recordCount, data);
	}
}

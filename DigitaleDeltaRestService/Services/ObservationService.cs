namespace DigitaleDeltaRestService.Services;

using DigitaleDelta;
using DigitaleDeltaExportFormats;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.OData.UriParser;
using Npgsql;

public class ObservationService
{
	private readonly NpgsqlConnection _connection;
	private readonly  IMemoryCache     _memoryCache;

	/// <summary>
	/// Constructor
	/// </summary>
	/// <param name="configuration"></param>
	/// <param name="memoryCache"></param>
	public ObservationService([FromServices] IConfiguration configuration, [FromServices] IMemoryCache memoryCache)
	{
		_connection = new NpgsqlConnection(configuration.GetConnectionString("postgres"));
		_memoryCache = memoryCache;
	}

	/// <summary>
	/// Query data. Since data is disconnected from IQueryable/Entity Framework and there is a lot of data, handle processing ourselves.
	/// </summary>
	/// <param name="filter">OData filter query option</param>
	/// <param name="skip">OData skip query option</param>
	/// <param name="top">OData top query option</param>
	/// <param name="maxPageSize">Maximum page size query option</param>
	/// <param name="expandRelated">Expand related observations query option</param>
	/// <param name="order">OData order query option</param>
	/// <param name="count">OData count query option</param>
	/// <returns></returns>
	public async Task<(bool moreData, long count, List<Observation> data)> QueryDataAsync(FilterQueryOption? filter, SkipQueryOption? skip, TopQueryOption? top, int maxPageSize, bool expandRelated, OrderByQueryOption? order, CountQueryOption? count)
	{
		var references = await ReferenceCache.GetReferencesFromCacheAsync(_memoryCache, _connection).ConfigureAwait(false);
		if (references == null)
		{
			return (false, 0, Array.Empty<Observation>().ToList());
		}

		var relatedReferences       = await ReferenceCache.GetReferenceRelationsFromCacheAsync(_memoryCache, _connection).ConfigureAwait(false);
		var referencesByTypeAndCode = await ReferenceCache.GetReferencesFromCacheByTypeAndCodeAsync(_memoryCache, _connection).ConfigureAwait(false);
		if (referencesByTypeAndCode == null)
		{
			return (false, 0, Array.Empty<Observation>().ToList());
		}
		
		var data     = (await new DatabaseLayer.Services.ObservationDataLayerService(_connection, _memoryCache, referencesByTypeAndCode).GetDataAsync(filter?.FilterClause, skip, top, maxPageSize, order, count));
		var related  = !expandRelated ? null : (await new DatabaseLayer.Services.ObservationDataLayerService(_connection, _memoryCache, referencesByTypeAndCode).GetRelatedObservationsOfAsync(data.data.Select(a => a.Id).ToList()));
		var response = await new DigitaleDeltaExport().GenerateExportDataAsync(data.data, references, related, expandRelated, relatedReferences).ConfigureAwait(false);
		return (data.moreData, data.count ?? 0, response.ToList());
	}
	
}
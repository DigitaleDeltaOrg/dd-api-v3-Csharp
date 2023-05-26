namespace DigitaleDeltaRestService.Services;

using DatabaseModel.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Spatial;
using Npgsql;

/// <summary>
/// Handle references
/// </summary>
public class ReferenceService
{
	private  readonly       NpgsqlConnection              _connection;
	private readonly        IMemoryCache                  _memoryCache;
	private static readonly NetTopologySuite.IO.WKTReader WktReader = new();

	/// <summary>
	/// Constructor
	/// </summary>
	/// <param name="configuration"></param>
	/// <param name="memoryCache"></param>
	public ReferenceService([FromServices] IConfiguration configuration, [FromServices] IMemoryCache memoryCache)
	{
		_connection  = new NpgsqlConnection(configuration.GetConnectionString("postgres"));
		_memoryCache = memoryCache;
	}

	/// <summary>
	/// Query data
	/// </summary>
	/// <param name="filter">OData filter query option</param>
	/// <param name="skip">OData skip query option</param>
	/// <param name="top">OData top query option</param>
	/// <param name="maxPageSize">Maximum page size</param>
	/// <param name="order">OData order query option</param>
	/// <returns></returns>
	public async Task<(bool moreData, long count, List<DigitaleDelta.Reference> data)> QueryDataAsync(FilterQueryOption? filter, SkipQueryOption? skip, TopQueryOption? top, int maxPageSize, OrderByQueryOption? order)
	{
		var references         = await ReferenceCache.GetReferencesFromCacheAsync(_memoryCache, _connection).ConfigureAwait(false);
		var referenceRelations = await ReferenceCache.GetReferenceRelationsFromCacheAsync(_memoryCache, _connection).ConfigureAwait(false);
		var data               = (await new DatabaseLayer.Services.ReferenceDataLayerService(_connection, _memoryCache).GetDataAsync(filter?.FilterClause, skip, top, maxPageSize, order));
		return (data.moreData, data.count ?? 0, data.data.Select(reference => DatabaseReferenceToODataReference(reference, references, referenceRelations)!).ToList());
	}

	private static DigitaleDelta.Reference? DatabaseReferenceToODataReference(Reference? databaseReference, IReadOnlyDictionary<Guid, Reference>? references, ILookup<Guid, GuidRole>? relatedReferences)
{
		if (databaseReference == null || databaseReference.Id == Guid.Empty || relatedReferences == null)
		{
			return null;
		}
		
		var taxonTypeId = relatedReferences[databaseReference.Id].FirstOrDefault(a => a.Role == "taxontype")?.Guid;
		var taxonGroupId = relatedReferences[databaseReference.Id].FirstOrDefault(a => a.Role == "taxongroup")?.Guid;
		var taxonParentId = relatedReferences[databaseReference.Id].FirstOrDefault(a => a.Role == "taxonparent")?.Guid;
		
		var point         = SetPoint(databaseReference);
		var reference = new DigitaleDelta.Reference();
		reference.Id              = databaseReference.Id.ToString();
		reference.Type            = DatabaseReferenceToODataReference(GetReferenceFromCache(databaseReference.ReferenceTypeId, references), references, relatedReferences)?.Code;
		reference.Organisation    = databaseReference.Organisation;
		reference.Code            = databaseReference.Code;
		reference.Href            = databaseReference.Uri ?? string.Empty;
		reference.Geometry        = point;
		reference.Description     = databaseReference.Description;
		reference.TaxonType       = DatabaseReferenceToODataReference(GetReferenceFromCache(taxonTypeId, references), references, relatedReferences);
		reference.TaxonGroup      = DatabaseReferenceToODataReference(GetReferenceFromCache(taxonGroupId, references), references, relatedReferences);
		reference.TaxonParent     = DatabaseReferenceToODataReference(GetReferenceFromCache(taxonParentId, references), references, relatedReferences);
		reference.TaxonRank       = databaseReference.TaxonRank;
		reference.TaxonAuthors    = databaseReference.TaxonAuthor;
		reference.CasNumber       = databaseReference.CasNumber;
		reference.ParameterType   = databaseReference.ParameterType;
		reference.TaxonNameNl     = databaseReference.TaxonNameNL;
		reference.TaxonStatusCode = databaseReference.TaxonStatusCode;
		reference.TaxonTypeId     = GuidEmptyOrNull(taxonTypeId) ? null: taxonTypeId.ToString();
		reference.TaxonGroupId    = GuidEmptyOrNull(taxonGroupId) ? null: taxonGroupId.ToString();
		reference.TaxonParentId   = GuidEmptyOrNull(taxonParentId) ? null: taxonParentId.ToString();
		
		return reference;
	}

	private static GeometryPoint? SetPoint(Reference reference)
	{
		// Geometry needs to be converted to SQL geometry, not NetTopologySuite geometry.
		if (string.IsNullOrEmpty(reference.Geometry))
		{
			return null;
		}

		var geoPoint = WktReader.Read(reference.Geometry);
		return GeometryPoint.Create(CoordinateSystem.Geometry(4258), geoPoint.Coordinate.X, geoPoint.Coordinate.Y, null, null);
	}

	private static Reference? GetReferenceFromCache(Guid? reference, IReadOnlyDictionary<Guid, Reference>? references)
	{
		if (reference == null || references == null)
		{
			return null;
		}
		
		return references.TryGetValue(reference.Value, out var value) ? value : null;
	}

	private static bool GuidEmptyOrNull(Guid? guid)
	{
		return guid == null || guid == Guid.Empty;
	}
}
namespace DigitaleDeltaRestService.Services;

using DatabaseModel.Models;
using Microsoft.Extensions.Caching.Memory;
using Npgsql;

public abstract record ReferenceCache
{
	public static async Task<Dictionary<Guid, Reference>?> GetReferencesFromCacheAsync(IMemoryCache cache, NpgsqlConnection connection)
	{
		var references = await cache.GetOrCreateAsync<Dictionary<Guid, Reference>>("referencesById", async cacheEntry =>
		{
			cacheEntry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
			return await new DatabaseLayer.Services.ReferenceDataLayerService(connection, cache).GetReferencesByIdAsync().ConfigureAwait(false);
		});
		return references;
	}
	
	public static async Task<ILookup<Guid, Reference>?> GetReferencesFromCacheByTypeAndCodeAsync(IMemoryCache cache, NpgsqlConnection connection)
	{
		var references = await cache.GetOrCreateAsync<ILookup<Guid, Reference>>("referencesByTypeAndCode", async cacheEntry =>
		{
			cacheEntry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
			return await new DatabaseLayer.Services.ReferenceDataLayerService(connection, cache).GetReferencesByTypeAndCodeFromCacheAsync().ConfigureAwait(false);
		});
		return references;
	}

	public static async Task<ILookup<Guid, GuidRole>?> GetReferenceRelationsFromCacheAsync(IMemoryCache cache, NpgsqlConnection connection)
	{
		var references = await cache.GetOrCreateAsync<ILookup<Guid, GuidRole>>("relatedReferences", async cacheEntry =>
		{
			cacheEntry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
			return await new DatabaseLayer.Services.ReferenceDataLayerService(connection, cache).GetReferenceRelationsAsync().ConfigureAwait(false);
		});
		return references;
	}
}
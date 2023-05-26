namespace DigitaleDeltaRestService.Controllers;

using System.Web;
using DigitaleDelta;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Attributes;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Services;

// API-20: Include the major version number in the URI

/// <summary>
/// 
/// </summary>
[ODataAttributeRouting]
[Route("v3/odata/references")]
public class ODataReferenceController : ODataController
{
	private const            int              MaxPageSize = 10000;
	private readonly ReferenceService _referenceService;

	/// <summary>
	/// 
	/// </summary>
	/// <param name="referenceService"></param>
	public ODataReferenceController([FromServices] ReferenceService referenceService)
	{
		_referenceService = referenceService;
	}
	
	// GET
	/// <summary>
	/// Retrieve references
	/// </summary>
	/// <param name="oDataQueryOptions"></param>
	/// <returns></returns>
	[HttpGet]
	public async Task<IQueryable<Reference>> Get(ODataQueryOptions<Reference> oDataQueryOptions)
	{
		const AllowedQueryOptions doNotLetLinqHandle = AllowedQueryOptions.Skip | AllowedQueryOptions.Top | AllowedQueryOptions.Top | AllowedQueryOptions.Select | AllowedQueryOptions.Expand | AllowedQueryOptions.OrderBy;
		var                       filter             = oDataQueryOptions?.Filter;
		var                       skip               = oDataQueryOptions?.Skip;
		var                       top                = oDataQueryOptions?.Top;
		var                       order              = oDataQueryOptions?.OrderBy;
		var                       data               = await _referenceService.QueryDataAsync(filter, skip, top, MaxPageSize, order).ConfigureAwait(false);
		var                       uriBuilder         = new UriBuilder(Request.Scheme, Request.Host.Host, Request.Host.Port ?? 443, Request.Path.Value);
		var                       results            = oDataQueryOptions?.ApplyTo(data.data.AsQueryable(), new ODataQuerySettings { IgnoredQueryOptions = doNotLetLinqHandle }) ?? new List<Reference>().AsQueryable();
		
		HandleNextLink(Request, data, skip, top, uriBuilder); // Handle next link, since the database is doing the filtering, not IQueryable.
		if (oDataQueryOptions?.Count != null)
		{
			Request.ODataFeature().TotalCount = data.count; // Handle the total count, since the database is doing the filtering, not IQueryable.
		}

		return results.Cast<Reference>();
	}

	private static void HandleNextLink(HttpRequest request, (bool moreData, long count, List<Reference> data) data, SkipQueryOption? skip, TopQueryOption? top, UriBuilder uriBuilder)
	{
		var queryAttributes = HttpUtility.ParseQueryString(request.QueryString.Value ?? string.Empty);
		queryAttributes.Remove("$skip");
		if (data.moreData)
		{
			queryAttributes.Add("$skip", skip?.Value + (top?.Value ?? 0).ToString());
		}

		uriBuilder.Query                = queryAttributes.ToString();
		request.ODataFeature().NextLink = uriBuilder.Uri;
	}
}
namespace DigitaleDeltaRestService.OData;

public static class RequestExtensions
{
	public static bool ShouldOmitNullValues(this HttpRequest request)
	{
		string?       preferHeader = null;
		
		if (request.Headers.TryGetValue("Prefer", out var values))
		{
			preferHeader = values.FirstOrDefault();
		}

		return preferHeader != null && preferHeader.Contains("omit-values=nulls", StringComparison.OrdinalIgnoreCase);
	}

	public static void SetPreferenceAppliedResponseHeader(this HttpRequest httpRequest)
	{
		var     response      = httpRequest.HttpContext.Response;
		string? preferApplied = null;

		if (response.Headers.TryGetValue("Preference-Applied", out var values))
		{
			preferApplied = values.FirstOrDefault();
		}

		if (preferApplied == null)
		{
			response.Headers["Preference-Applied"] = "omit-values=nulls";
		}
		else 
		{
			if (!preferApplied.Contains("omit-values=nulls"))
			{
				response.Headers["Preference-Applied"] = $"{preferApplied},omit-values=nulls";
			}
		}
	}
}

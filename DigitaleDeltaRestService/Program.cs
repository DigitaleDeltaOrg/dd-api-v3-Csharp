using System.Text.Json.Serialization;
using Dapper.Json;
using DigitaleDeltaRestService;
using DigitaleDeltaRestService.OData;
using DigitaleDeltaRestService.Services;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.OData.Formatter.Serialization;
using Microsoft.AspNetCore.OData.Query.Expressions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.OData.Edm;
using static Dapper.SqlMapper;

var          builder         = WebApplication.CreateBuilder(args);
var          configuration   = builder.Configuration;
const string allowAllOrigins = "_allowAllOrigins";

AddTypeHandler(new JsonTypeHandler<Dictionary<string, string>>());
var edmModel = DigitaleDeltaModelBuilder.GetEdmModel();
SetBuilderServices(builder, edmModel, configuration);
SetAppProperties(builder).Run();

void SetBuilderServices(WebApplicationBuilder webApplicationBuilder, IEdmModel model, IConfiguration configurationManager)
{
	var cache                    = new MemoryCache(new MemoryCacheOptions { ExpirationScanFrequency = TimeSpan.FromHours(1) });
	webApplicationBuilder.Services.AddSingleton<IEdmModel>(_ => model);
	webApplicationBuilder.Services.AddSingleton<IMemoryCache>(cache);
	webApplicationBuilder.Services.AddSingleton(_ => configurationManager);
	webApplicationBuilder.Services.AddScoped(_ => new ReferenceService(configurationManager, cache));
	webApplicationBuilder.Services.AddScoped(_ => new ObservationService(configurationManager, cache));
	webApplicationBuilder.Services.AddControllers(_ => _.ModelMetadataDetailsProviders.Add(new SystemTextJsonValidationMetadataProvider())).AddOData(opt =>
	{
		opt.AddRouteComponents("v3/odata", model, services =>
		{
			services.AddSingleton<IFilterBinder, GeometryFilterBinder>();
			services.AddSingleton<ODataResourceSerializer, OmitNullResourceSerializer>();
		}).EnableQueryFeatures(10000);
	}).AddJsonOptions(jsonOptions =>
	{
		jsonOptions.JsonSerializerOptions.NumberHandling         = JsonNumberHandling.AllowNamedFloatingPointLiterals;
		jsonOptions.JsonSerializerOptions.PropertyNamingPolicy   = null;
		jsonOptions.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
	});
	webApplicationBuilder.Services.AddCors(options => {
    options.AddPolicy(name: allowAllOrigins, policy  => { policy.WithOrigins("*"); });
  });
	webApplicationBuilder.Services.AddEndpointsApiExplorer();
	webApplicationBuilder.Services.AddSwaggerGen(swagger =>
	{
		swagger.ResolveConflictingActions(act => act.First());
	});
	webApplicationBuilder.Services.AddRequestDecompression();
	webApplicationBuilder.Services.AddResponseCompression();
}

WebApplication SetAppProperties(WebApplicationBuilder applicationBuilder)
{
	var webApplication = applicationBuilder.Build();
	webApplication.UseCors(allowAllOrigins);
	webApplication.UseStaticFiles();
	webApplication.UseDeveloperExceptionPage();
	webApplication.UseODataRouteDebug();
	webApplication.UseODataQueryRequest();
	webApplication.UseSwagger();

	// API-16: Use OpenAPI Specification for documentation
	webApplication.UseSwaggerUI(swagger =>
	{
		swagger.SwaggerEndpoint("/v3/openapi.json", "OData raw OpenAPI");
		swagger.RoutePrefix   = string.Empty;
		swagger.DocumentTitle = "DD-API V3 Reference ";
	});

	// API-16: Use OpenAPI Specification for documentation
	webApplication.UseReDoc(redoc =>
	{
		redoc.RoutePrefix   = "redoc";
		redoc.DocumentTitle = "DD-API V3 Proof-of-Concept";
		redoc.SpecUrl       = "/v3/openapi.json";
	});

	webApplication.UseRouting();
	webApplication.MapControllers();
	webApplication.UseRequestDecompression();

	async Task FixKennisplatformApiCompliance(HttpContext context, Func<Task> next)
	{
		// API-57: Return the full version number in a response header
		context.Response.Headers.Add("API-Version", "1.0.1");

		// API-03: Only apply standard HTTP methods
		var badVerbs = new List<string> { "PROPFIND", "ACL", "MKCALENDAR", "LINK", "BREW", "WHEN" };
		if (badVerbs.Contains(context.Request.Method))
		{
			context.Response.StatusCode = 405;
			return;
		}

		// API-48: Leave off trailing slashes from URIs
		if (context.Request.Path.ToString().EndsWith("/"))
		{
			context.Response.StatusCode = 404;
			return;
		}

		await next.Invoke();
	}

	webApplication.Use(FixKennisplatformApiCompliance);
	return webApplication;
}

// https://devblogs.microsoft.com/odata/tutorial-build-grpc-odata-in-asp-net-core/
// https://devblogs.microsoft.com/odata/customizing-filter-for-spatial-data-in-asp-net-core-odata-8/
// https://github.com/xuzhg/WebApiSample/tree/main/ODataSpatialSample
// https://medium.com/@nirinchev/dealing-with-spatial-data-in-odata-4e4051434ddb
// https://github.com/OData/WebApi/issues/438
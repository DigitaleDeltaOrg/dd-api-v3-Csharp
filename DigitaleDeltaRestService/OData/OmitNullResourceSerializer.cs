// https://devblogs.microsoft.com/odata/extension-omit-null-value-properties-in-asp-net-core-odata/
namespace DigitaleDeltaRestService.OData;

using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Formatter.Serialization;
using Microsoft.AspNetCore.OData.Formatter.Value;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

/// <summary>
/// </summary>
public class OmitNullResourceSerializer : ODataResourceSerializer
{
  /// <summary>
  /// </summary>
  /// <param name="serializerProvider"></param>
  public OmitNullResourceSerializer(IODataSerializerProvider serializerProvider) : base(serializerProvider)
	{
	}

  /// <summary>
  /// </summary>
  /// <param name="structuralProperty"></param>
  /// <param name="resourceContext"></param>
  /// <returns></returns>
  public override ODataProperty? CreateStructuralProperty(IEdmStructuralProperty structuralProperty, ResourceContext resourceContext)
	{
		var isOmitNulls = resourceContext.Request.ShouldOmitNullValues();
		if (!isOmitNulls)
		{
			return base.CreateStructuralProperty(structuralProperty, resourceContext);
		}

		var propertyValue = resourceContext.GetPropertyValue(structuralProperty.Name);
		if (propertyValue != null)
		{
			return base.CreateStructuralProperty(structuralProperty, resourceContext);
		}

		// it MUST specify the Preference-Applied response header with omit-values=nulls
		resourceContext.Request.SetPreferenceAppliedResponseHeader();
		
		return null;
	}

  /// <summary>
  /// </summary>
  /// <param name="complexProperty"></param>
  /// <param name="pathSelectItem"></param>
  /// <param name="resourceContext"></param>
  /// <returns></returns>
  public override ODataNestedResourceInfo? CreateComplexNestedResourceInfo(IEdmStructuralProperty complexProperty, PathSelectItem pathSelectItem, ResourceContext resourceContext)
	{
		var isOmitNulls = resourceContext.Request.ShouldOmitNullValues();
		if (!isOmitNulls)
		{
			return base.CreateComplexNestedResourceInfo(complexProperty, pathSelectItem, resourceContext);
		}

		var propertyValue = resourceContext.GetPropertyValue(complexProperty.Name);
		if (propertyValue != null && propertyValue is not NullEdmComplexObject)
		{
			return base.CreateComplexNestedResourceInfo(complexProperty, pathSelectItem, resourceContext);
		}

		resourceContext.Request.SetPreferenceAppliedResponseHeader();
		
		return null;
	}

  /// <summary>
  /// </summary>
  /// <param name="navigationProperty"></param>
  /// <param name="resourceContext"></param>
  /// <returns></returns>
  public override ODataNestedResourceInfo? CreateNavigationLink(IEdmNavigationProperty navigationProperty, ResourceContext resourceContext)
	{
		var isOmitNulls = resourceContext.Request.ShouldOmitNullValues();
		if (!isOmitNulls)
		{
			return base.CreateNavigationLink(navigationProperty, resourceContext);
		}

		var propertyValue = resourceContext.GetPropertyValue(navigationProperty.Name);
		if (propertyValue != null && propertyValue is not NullEdmComplexObject)
		{
			return base.CreateNavigationLink(navigationProperty, resourceContext);
		}

		resourceContext.Request.SetPreferenceAppliedResponseHeader();
		
		return null;
	}
}

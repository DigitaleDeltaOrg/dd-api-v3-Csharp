namespace DigitaleDeltaRestService;

using DigitaleDelta;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;

public static class DigitaleDeltaModelBuilder
{
	public static IEdmModel GetEdmModel()
	{
		var modelBuilder = new ODataConventionModelBuilder();
		modelBuilder.EntityType<Reference>().HasKey(_ => _.Id);
		modelBuilder.EntityType<Reference>().HasOptional(a => a.TaxonGroup, (a,  b) => a.TaxonGroupId == b!.Id);
		modelBuilder.EntityType<Reference>().HasOptional(a => a.TaxonType, (a,   b) => a.TaxonTypeId == b!.Id);
		modelBuilder.EntityType<Reference>().HasOptional(a => a.TaxonParent, (a, b) => a.TaxonParentId == b!.Id);
		modelBuilder.EntityType<Observation>().HasKey(_ => _.Id);
		modelBuilder.EntityType<Observation>().HasMany(a => a.RelatedObservations);
		modelBuilder.EntityType<RelatedObservation>().HasKey(_ => _.Id);
		
		// API-54: Use plural nouns to name collection resources
		// API-05: Use nouns to name resources
		modelBuilder.EntitySet<Reference>("references");
		modelBuilder.EntitySet<Observation>("observations");

		return modelBuilder.GetEdmModel();
	}

}
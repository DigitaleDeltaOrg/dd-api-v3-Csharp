namespace DatabaseModel.Models;

using System.ComponentModel.DataAnnotations.Schema;
using Dapper.Contrib.Extensions;

[Dapper.Contrib.Extensions.Table("reference")]
public class Reference
{
	public Guid    Id                { set; get; }
	public Guid?   ReferenceTypeId   { set; get; }
	public string? Organisation      { set; get; }
	public string  Code              { set; get; }
	public string? Uri               { set; get; }
	public string? Geometry          { set; get; }
	public string? Display           { set; get; }
	public string? Description       { set; get; }
	public string? TaxonRank         { set; get; }
	public string? TaxonAuthor       { set; get; }
	public string? CasNumber         { set; get; }
	public string? ParameterType     { set; get; }
	public string? TaxonNameNL       { set; get; }
	public string? TaxonStatusCode   { set; get; }

	public static Reference FromDynamic(dynamic item)
	{
		return new Reference()
		{
			Id                = item.id,
			ReferenceTypeId   = item.reference_type_id,
			Organisation      = item.organisation,
			Code              = item.code,
			Uri               = item.uri,
			Geometry          = item.geometry,
			Display           = item.display,
			Description       = item.description,
			TaxonRank         = item.taxon_rank,
			TaxonAuthor       = item.taxon_author,
			CasNumber         = item.cas_number,
			ParameterType     = item.parameter_type,
			TaxonNameNL       = item.taxon_name_nl,
			TaxonStatusCode   = item.taxon_status_code
		};
	}
}


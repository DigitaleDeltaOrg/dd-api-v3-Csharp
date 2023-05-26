namespace DigitaleDelta;

using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Microsoft.Spatial;

public class Reference
{
	[Key] public              string?        Id              { get; set; } = null!; // Id is the primary key
	public                    string?        Type            { get; set; }
	public                    string?        Organisation    { get; set; }
	public                    string?        Code            { get; set; } = null!;
	[IgnoreDataMember] public string?        Href            { get; set; } = null!;
	public                    GeometryPoint? Geometry        { get; set; }
	public                    string?        Description     { get; set; }
	public                    string?        ExternalKey     { get; set; }
	public                    string?        TaxonRank       { get; set; }
	public                    string?        TaxonAuthors    { get; set; }
	public                    string?        TaxonNameNl     { get; set; }
	public                    string?        ParameterType   { get; set; }
	public                    string?        TaxonStatusCode { get; set; }
	public                    string?        CasNumber       { get; set; }
	public                    string?        TaxonTypeId     { get; set; }
	public                    string?        TaxonGroupId    { get; set; }
	public                    string?        TaxonParentId   { get; set; }
	public                    Reference?     TaxonType       { get; set; }
	public                    Reference?     TaxonGroup      { get; set; }
	public                    Reference?     TaxonParent     { get; set; }

	public string DisplayName => $"{Type}/{Organisation}/{Code}/{Description}";
}

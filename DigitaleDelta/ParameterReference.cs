namespace DigitaleDelta;

using Microsoft.OData.ModelBuilder;

[AutoExpand]
public class ParameterReference
{
	public string                           Id              { get; set; } = null!; // Id is the primary key
	public string?                          Type            { get; set; } = null!;
	public string?                          Organisation    { get; set; } = null!;
	public string?                          Code            { get; set; } = null!;
	public string?                          Description     { get; set; }
	public string?                          Role            { get; set; }
	public string?                          TaxonRank       { get; set; }
	public string?                          TaxonAuthors    { get; set; }
	public string?                          TaxonNameNl     { get; set; }
	public string?                          ParameterType   { get; set; }
	public string?                          TaxonStatusCode { get; set; }
	public string?                          CasNumber       { get; set; }
	public string?                          TaxonTypeId     { get; set; }
	public string?                          TaxonGroupId    { get; set; }
	public string?                          TaxonParentId   { get; set; }
	public string?                          TaxonType       { get; set; }
	public string?                          TaxonGroup      { get; set; }
	public string?                          TaxonParent     { get; set; }
	public Microsoft.Spatial.GeometryPoint? Geometry        { set; get; } = null!;
	public string?                          Display         => Organisation == Code ? Code : $"{Organisation}/{Code}";
}
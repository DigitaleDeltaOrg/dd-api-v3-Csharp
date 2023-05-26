namespace DigitaleDelta;

using System.ComponentModel.DataAnnotations;

public class CategoryVerb
{
	[Required] public string Vocabulary { get; set; } = null!;
	[Required] public string Term       { get; set; } = null!;
}
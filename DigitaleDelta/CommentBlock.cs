namespace DigitaleDelta;

using System.ComponentModel.DataAnnotations;

public class CommentBlock
{
	public            string ApplicablePeriod { get; set; } = null!;
	[Required] public string Comment          { get; set; } = null!;
}
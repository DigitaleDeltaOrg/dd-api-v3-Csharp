namespace DatabaseModel.Models;

public class ReferenceRelatedReference
{
	public Guid   ReferenceId        { set; get; }
	public Guid   RelatedReferenceId { set; get; }
	public string Role               { set; get; } = null!;

	public static ReferenceRelatedReference FromDynamic(dynamic item)
	{
		return new ReferenceRelatedReference()
		{
			ReferenceId        = item.reference_id,
			RelatedReferenceId = item.related_reference_id,
			Role               = item.role
		};
	}
}
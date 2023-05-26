namespace DatabaseModel.Models;

public class ObservationRelatedObservation
{
	public Guid   Id                   { set; get; }
	public Guid   RelatedObservationId { set; get; }
	public string Role                 { set; get; } = null!;
}
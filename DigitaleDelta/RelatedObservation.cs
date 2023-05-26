namespace DigitaleDelta;

public class RelatedObservation : Observation
{
	public string? Role { set; get; }

	public static RelatedObservation? FromObservation(Observation observation)
	{
		var serialized   = System.Text.Json.JsonSerializer.Serialize(observation);
		var deserialized = System.Text.Json.JsonSerializer.Deserialize<RelatedObservation>(serialized);
		// Deserialize that base class string into the child class.
		return deserialized;
	}
}
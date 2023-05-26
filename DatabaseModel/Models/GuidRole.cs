namespace DatabaseModel.Models;

public class GuidRole
{
	public Guid   Guid { set; get; }
	public string Role { set; get; }

	public GuidRole(Guid guid, string role)
	{
		Guid = guid;
		Role = role;
	}
}
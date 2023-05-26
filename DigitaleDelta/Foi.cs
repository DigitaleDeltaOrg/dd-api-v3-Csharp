namespace DigitaleDelta;

public class Foi
{
	public string                          Id           { set; get; } = null!;
	public string                          Code         { set; get; } = null!;
	public string                          Description  { set; get; } = null!;
	public string                          Organisation { set; get; } = null!;
	public Microsoft.Spatial.GeometryPoint Geometry     { set; get; } = null!;
}
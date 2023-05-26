namespace DatabaseLayer.Utility.Models;

public class FieldMap
{
	public string      FieldName          { get; }
	public string      MappedFieldName    { get; }
	public MapTypeEnum MapType            { get; }
	public bool        RequireTermination { get; }
	public string?     ReferenceFieldName { get; }
	public string?     ReplacementQuery   { get; }
	public string?     OperatorOverrule   { get; }
	public Guid?       Guid               { get;  }

	public FieldMap(string fieldName, string mappedFieldName, MapTypeEnum mapType, bool requireTermination, string? replacementQuery = null, string? referenceFieldName = null, string? operatorOverrule = null, Guid? guid = null)
	{
		FieldName          = fieldName;
		MappedFieldName    = mappedFieldName;
		MapType            = mapType;
		ReferenceFieldName = referenceFieldName;
		ReplacementQuery   = replacementQuery;
		RequireTermination = requireTermination;
		OperatorOverrule   = operatorOverrule;
		Guid               = guid;
	}
}
namespace DatabaseLayer.Utility;

using System.Globalization;
using System.Text.RegularExpressions;
using Models;
using Microsoft.OData.UriParser;

public class FilterFieldMapper : QueryNodeVisitor<string>
{
	private const           DateTimeStyles                DateTimeStyles = System.Globalization.DateTimeStyles.AssumeUniversal | System.Globalization.DateTimeStyles.AdjustToUniversal | System.Globalization.DateTimeStyles.AllowWhiteSpaces;
	private readonly        Dictionary<string, FieldMap?> _map;
	private static          string                        _suffix            = string.Empty;
	private static readonly Regex                         InvalidWordPattern = new(@"([^\p{L}\p{Nl}])");

	public FilterFieldMapper(Dictionary<string, FieldMap?> map)
	{
		_map        = map;
	}
	
	/// <summary>
	/// Translates a <see cref="AllNode"/> into a corresponding <see cref="String"/>.
	/// </summary>
	/// <param name="node">The node to translate.</param>
	/// <returns>The translated String.</returns>
	public override string Visit(AllNode node)
	{
		var source = TranslateNode(node.Source);
		var result = $"{source}/ALL({node.CurrentRangeVariable.Name}:{TranslateNode(node.Body)})";
		return result;
	}

	/// <summary>
	/// Translates a <see cref="AnyNode"/> into a corresponding <see cref="String"/>.
	/// </summary>
	/// <param name="node">The node to translate.</param>
	/// <returns>The translated String.</returns>
	public override string Visit(AnyNode node)
	{
		switch (node.CurrentRangeVariable)
		{
			case null when node.Body.Kind == QueryNodeKind.Constant:
				return $"{TranslateNode(node.Source)}/ANY()";
			case null:
				return string.Empty;
		}

		_suffix = string.Empty;
		var body = TranslateNode(node.Body);
		return body;
	}

	/// <summary>
	/// Translates a <see cref="BinaryOperatorNode"/> into a corresponding <see cref="String"/>.
	/// </summary>
	/// <param name="node">The node to translate.</param>
	/// <returns>The translated String.</returns>
	public override string Visit(BinaryOperatorNode node)
	{
		var left  = TranslateNode(node.Left);
		var right = TranslateNode(node.Right);

		if (node.Left.Kind == QueryNodeKind.BinaryOperator && DetermineBinaryOperatorPriority(((BinaryOperatorNode)node.Left).OperatorKind) < DetermineBinaryOperatorPriority(node.OperatorKind) ||
		    node.Left.Kind == QueryNodeKind.Convert && ((ConvertNode)node.Left).Source.Kind == QueryNodeKind.BinaryOperator && DetermineBinaryOperatorPriority(((BinaryOperatorNode)((ConvertNode)node.Left).Source).OperatorKind) < DetermineBinaryOperatorPriority(node.OperatorKind))
		{
			left = $"{left}";
		}
		
		if (node.Right.Kind == QueryNodeKind.BinaryOperator && DetermineBinaryOperatorPriority(((BinaryOperatorNode)node.Right).OperatorKind) < DetermineBinaryOperatorPriority(node.OperatorKind) ||
		    node.Right.Kind == QueryNodeKind.Convert && ((ConvertNode)node.Right).Source.Kind == QueryNodeKind.BinaryOperator && DetermineBinaryOperatorPriority(((BinaryOperatorNode)((ConvertNode)node.Right).Source).OperatorKind) < DetermineBinaryOperatorPriority(node.OperatorKind))
		{
			right = $"{right}";
		}
		
		var map = _map.TryGetValue(left, out var mapValue) ? mapValue : null;
		return map?.OperatorOverrule != null 
			? $"{left}{map.OperatorOverrule} {right}" 
			: $"{left} {BinaryOperatorNodeToString(node.OperatorKind)} {right}";
	}

	/// <summary>
	/// Translates a <see cref="InNode"/> into a corresponding <see cref="String"/>.
	/// </summary>
	/// <param name="node">The node to translate.</param>
	/// <returns>The translated String.</returns>
	public override string Visit(InNode node)
	{
		var left  = TranslateNode(node.Left);
		var right = TranslateNode(node.Right);
		return $"{left} IN {right.Replace("[", "(").Replace("]", ")")}";
	}

	/// <summary>
	/// Translates a <see cref="CountNode"/> into a corresponding <see cref="String"/>.
	/// </summary>
	/// <param name="node">The node to translate.</param>
	/// <returns>The translated String.</returns>
	public override string Visit(CountNode node)
	{
		var source = TranslateNode(node.Source);
		return $"{source}/$count";
	}

	/// <summary>
	/// Translates a <see cref="CollectionNavigationNode"/> into a corresponding <see cref="String"/>.
	/// </summary>
	/// <param name="node">The node to translate.</param>
	/// <returns>The translated String.</returns>
	public override string Visit(CollectionNavigationNode node)
	{
		return TranslatePropertyAccess(node.Source, node.NavigationProperty.Name);
	}

	/// <summary>
	/// Translates a <see cref="CollectionPropertyAccessNode"/> into a corresponding <see cref="String"/>.
	/// </summary>
	/// <param name="node">The node to translate.</param>
	/// <returns>The translated String.</returns>
	public override string Visit(CollectionPropertyAccessNode node)
	{
		return TranslatePropertyAccess(node.Source, node.Property.Name);
	}

	/// <summary>
	/// Translates a <see cref="CollectionComplexNode"/> into a corresponding <see cref="String"/>.
	/// </summary>
	/// <param name="node">The node to translate.</param>
	/// <returns>The translated String.</returns>
	public override string Visit(CollectionComplexNode node)
	{
		return TranslatePropertyAccess(node.Source, node.Property.Name);
	}

	/// <summary>
	/// Translates a <see cref="ConstantNode"/> into a corresponding <see cref="String"/>.
	/// </summary>
	/// <param name="node">The node to translate.</param>
	/// <returns>The translated String.</returns>
	public override string Visit(ConstantNode node)
	{
		var value = TranslateValue(node, node.LiteralText + _suffix);
		_suffix = string.Empty;
		return $"{value ?? "null"}";
	}

	private static string? TranslateValue(ConstantNode node, string nodeLiteralText)
	{
		var value = node.Value switch
		{
			null => null,
			string _ => $"{nodeLiteralText}",
			bool _ => nodeLiteralText.ToLower(),
			DateTimeOffset _ => $"datetimeoffset'{nodeLiteralText}'",
			DateTime _ => $"datetime'{nodeLiteralText}'",
			TimeSpan _ => $"time'{nodeLiteralText}'",
			Guid _ => $"guid'{nodeLiteralText}'",
			byte[] _ => $"binary'{nodeLiteralText}'",
			decimal _ => $"{nodeLiteralText}M",
			float _ => $"{nodeLiteralText}",
			double _ => $"{nodeLiteralText}",
			long _ => $"{nodeLiteralText}",
			int _ => $"{nodeLiteralText}",
			_ => nodeLiteralText
		};
		_suffix = string.Empty;
		return value;
	}

	/// <summary>
	/// Translates a <see cref="CollectionConstantNode"/> into a corresponding <see cref="String"/>.
	/// </summary>
	/// <param name="node">The node to translate.</param>
	/// <returns>The translated String.</returns>
	public override string Visit(CollectionConstantNode node)
	{
		return $"{(string.IsNullOrEmpty(node.LiteralText) ? "null" : node.LiteralText)})";
	}

	/// <summary>
	/// Translates a <see cref="ConvertNode"/> into a corresponding <see cref="String"/>.
	/// </summary>
	/// <param name="node">The node to translate.</param>
	/// <returns>The translated String.</returns>
	public override string Visit(ConvertNode node)
	{
		return TranslateNode(node.Source);
	}

	/// <summary>
	/// Translates a <see cref="CollectionResourceCastNode"/> into a corresponding <see cref="String"/>.
	/// </summary>
	/// <param name="node">The node to translate.</param>
	/// <returns>The translated String of CollectionResourceCastNode.</returns>
	public override string Visit(CollectionResourceCastNode node)
	{
		return TranslatePropertyAccess(node.Source, node.ItemStructuredType?.Definition?.ToString());
	}

	/// <summary>
	/// Translates a <see cref="ResourceRangeVariableReferenceNode"/> into a corresponding <see cref="String"/>.
	/// </summary>
	/// <param name="node">The node to translate.</param>
	/// <returns>The translated String.</returns>
	public override string Visit(ResourceRangeVariableReferenceNode node)
	{
		return node.Name == "$it" ? string.Empty : node.NavigationSource.Name;
	}

	/// <summary>
	/// Translates a <see cref="NonResourceRangeVariableReferenceNode"/> into a corresponding <see cref="String"/>.
	/// </summary>
	/// <param name="node">The node to translate.</param>
	/// <returns>The translated String.</returns>
	public override string Visit(NonResourceRangeVariableReferenceNode node)
	{
		return node.Name;
	}

	/// <summary>
	/// Translates a <see cref="SingleResourceCastNode"/> into a corresponding <see cref="String"/>.
	/// </summary>
	/// <param name="node">The node to translate.</param>
	/// <returns>The translated String.</returns>
	public override string Visit(SingleResourceCastNode node)
	{
		return TranslatePropertyAccess(node.Source, node.StructuredTypeReference.Definition.ToString());
	}

	/// <summary>
	/// Translates a <see cref="SingleNavigationNode"/> into a corresponding <see cref="String"/>.
	/// </summary>
	/// <param name="node">The node to translate.</param>
	/// <returns>The translated String.</returns>
	public override string Visit(SingleNavigationNode node)
	{
		return TranslatePropertyAccess(node.Source, node.NavigationProperty.Name);
	}

	/// <summary>
	/// Translates a <see cref="SingleResourceFunctionCallNode"/> into a corresponding <see cref="String"/>.
	/// </summary>
	/// <param name="node">The node to translate.</param>
	/// <returns>The translated String.</returns>
	public override string Visit(SingleResourceFunctionCallNode node)
	{
		var result = node.Name;
		if (node.Source != null)
		{
			result = TranslatePropertyAccess(node.Source, result);
		}

		return TranslateFunctionCall(result, node.Parameters);
	}

	/// <summary>
	/// Translates a <see cref="SingleValueFunctionCallNode"/> into a corresponding <see cref="String"/>.
	/// </summary>
	/// <param name="node">The node to translate.</param>
	/// <returns>The translated String.</returns>
	public override string Visit(SingleValueFunctionCallNode node)
	{
		var result = node.Name;
		if (node.Source != null)
		{
			result = TranslatePropertyAccess(node.Source, result);
		}

		return TranslateFunctionCall(result, node.Parameters);
	}

	/// <summary>
	/// Translates a <see cref="CollectionFunctionCallNode"/> into a corresponding <see cref="String"/>.
	/// </summary>
	/// <param name="node">The node to translate.</param>
	/// <returns>The translated String of CollectionFunctionCallNode.</returns>
	public override string Visit(CollectionFunctionCallNode node)
	{
		var result = node.Name;
		if (node.Source != null)
		{
			result = TranslatePropertyAccess(node.Source, result);
		}

		return TranslateFunctionCall(result, node.Parameters);
	}

	/// <summary>
	/// Translates a <see cref="CollectionResourceFunctionCallNode"/> into a corresponding <see cref="String"/>.
	/// </summary>
	/// <param name="node">The node to translate.</param>
	/// <returns>The translated String of CollectionResourceFunctionCallNode.</returns>
	public override string Visit(CollectionResourceFunctionCallNode node)
	{
		var result = node.Name;
		if (node.Source != null)
		{
			result = TranslatePropertyAccess(node.Source, result);
		}

		return TranslateFunctionCall(result, node.Parameters);
	}

	/// <summary>
	/// Translates a <see cref="SingleValueOpenPropertyAccessNode"/> into a corresponding <see cref="String"/>.
	/// </summary>
	/// <param name="node">The node to translate.</param>
	/// <returns>The translated String.</returns>
	public override string Visit(SingleValueOpenPropertyAccessNode node)
	{
		return TranslatePropertyAccess(node.Source, node.Name);
	}

	/// <summary>
	/// Translates an <see cref="CollectionOpenPropertyAccessNode"/> into a corresponding <see cref="String"/>.
	/// </summary>
	/// <param name="node">The node to translate.</param>
	/// <returns>The translated String.</returns>
	public override string Visit(CollectionOpenPropertyAccessNode node)
	{
		return TranslatePropertyAccess(node.Source, node.Name);
	}

	/// <summary>
	/// Translates a <see cref="SingleValuePropertyAccessNode"/> into a corresponding <see cref="String"/>.
	/// </summary>
	/// <param name="node">The node to translate.</param>
	/// <returns>The translated String.</returns>
	public override string Visit(SingleValuePropertyAccessNode node)
	{
		return TranslatePropertyAccess(node.Source, node.Property.Name);
	}

	/// <summary>
	/// Translates a <see cref="SingleComplexNode"/> into a corresponding <see cref="String"/>.
	/// </summary>
	/// <param name="node">The node to translate.</param>
	/// <returns>The translated String.</returns>
	public override string Visit(SingleComplexNode node)
	{
		return TranslatePropertyAccess(node.Source, node.Property.Name);
	}

	/// <summary>
	/// Translates a <see cref="ParameterAliasNode"/> into a corresponding <see cref="String"/>.
	/// </summary>
	/// <param name="node">The node to translate.</param>
	/// <returns>The translated String.</returns>
	public override string Visit(ParameterAliasNode node)
	{
		return node.Alias;
	}

	/// <summary>
	/// Translates a <see cref="NamedFunctionParameterNode"/> into a corresponding <see cref="String"/>.
	/// </summary>
	/// <param name="node">The node to translate.</param>
	/// <returns>The translated String of NamedFunctionParameterNode.</returns>
	public override string Visit(NamedFunctionParameterNode node)
	{
		return $"{node.Name}={TranslateNode(node.Value)}";
	}

	/// <summary>
	/// Translates a <see cref="NamedFunctionParameterNode"/> into a corresponding <see cref="String"/>.
	/// </summary>
	/// <param name="node">The node to translate.</param>
	/// <returns>The translated String of SearchTermNode.</returns>
	public override string Visit(SearchTermNode node)
	{
		return IsValidSearchWord(node.Text) == false ? $"\"{node.Text}\"" : node.Text;
	}

	/// <summary>
	/// Translates a <see cref="UnaryOperatorNode"/> into a corresponding <see cref="String"/>.
	/// </summary>
	/// <param name="node">The node to translate.</param>
	/// <returns>The translated String.</returns>
	public override string Visit(UnaryOperatorNode node)
	{
		var result = node.OperatorKind switch
		{
			UnaryOperatorKind.Negate => "-",
			UnaryOperatorKind.Not => "not",
			_ => null
		};

		return node.Operand.Kind is QueryNodeKind.Constant or QueryNodeKind.SearchTerm
			? $"{result} {TranslateNode(node.Operand)}"
			: $"{result}({TranslateNode(node.Operand)})";
	}

	/// <summary>
	/// Main dispatching visit method for translating query-nodes into expressions.
	/// </summary>
	/// <param name="node">The node to visit/translate.</param>
	/// <returns>The LINQ String resulting from visiting the node.</returns>
	private string TranslateNode(QueryNode node)
	{
		return node.Accept(this);
	}

	/// <summary>Translates a <see cref="FilterClause"/> into a string.</summary>
	/// <param name="filterClause">The filter clause to translate.</param>
	/// <returns>The translated String.</returns>
	public string TranslateFilterClause(FilterClause? filterClause)
	{
		if (filterClause?.Expression == null)
		{
			return string.Empty;
		}

		var query = TranslateNode(filterClause.Expression);
		return query;
	}

	/// <summary>
	/// Helper for translating an access to a metadata-defined property or navigation.
	/// </summary>
	/// <param name="sourceNode">The source of the property access.</param>
	/// <param name="edmPropertyName">The structural or navigation property being accessed.</param>
	/// <returns>The translated String.</returns>
	private string TranslatePropertyAccess(QueryNode? sourceNode, string? edmPropertyName)
	{
		if (sourceNode == null || edmPropertyName == null)
		{
			return string.Empty;
		}
		
		var source = TranslateNode(sourceNode);
		var query  = string.IsNullOrEmpty(source) ? edmPropertyName : $"{source}/{edmPropertyName}";
		var mapped = _map.TryGetValue(query, out var mappedQuery) ? mappedQuery : null;
		if (mapped == null)
		{
			return query;
		}
		
		_suffix = mapped.RequireTermination ? ")" : string.Empty;
		query   = mapped.ReplacementQuery ?? mapped.MappedFieldName;
		
		return $"{query}";
	}

	/// <summary>
	/// Translates a function call into a corresponding <see cref="String"/>.
	/// </summary>
	/// <param name="functionName">Name of the function.</param>
	/// <param name="argumentNodes">The argument nodes.</param>
	/// <returns>
	/// The translated String.
	/// </returns>
	// ReSharper disable once CyclomaticComplexity
	private string TranslateFunctionCall(string functionName, IEnumerable<QueryNode> argumentNodes)
	{
		argumentNodes = argumentNodes.ToList();
		var arguments = argumentNodes.Select(TranslateNode).ToList();
		var map       = _map.TryGetValue(arguments[0], out var mapValue) ? mapValue : null;
		return map?.MapType switch
		{
			null => HandleFunction(functionName.ToLowerInvariant(), arguments, argumentNodes),
			MapTypeEnum.Value => HandleNameMappedFunction(map, functionName.ToLowerInvariant(), arguments, argumentNodes),
			MapTypeEnum.Reference => HandleReferenceMappedFunction(map, functionName.ToLowerInvariant(), arguments, argumentNodes),
			MapTypeEnum.Metadata => string.Empty,
			var _ => string.Empty
		};
	}

	// ReSharper disable once CyclomaticComplexity
	private string HandleFunction(string functionName, IReadOnlyList<string> arguments, IEnumerable<QueryNode> argumentNodes) =>
		functionName.ToLowerInvariant() switch
		{
			"startswith" => $" {arguments[0]} LIKE {arguments[1].Replace(arguments[1].Split("'")[1], $"{arguments[1].Split("'")[1]}%")}",
			"endswith" => $" {arguments[0]} LIKE {arguments[1].Replace(arguments[1].Split("'")[1], $"%{arguments[1].Split("'")[1]}")}",
			"contains" => $" {arguments[0]} LIKE {arguments[1].Replace(arguments[1].Split("'")[1], $"%{arguments[1].Split("'")[1]}%")}",
			"trim" => $" TRIM({arguments[0]})",
			"date" => $" CAST({arguments[0]} AS DATE)",
			"time" => $" {arguments[0]}::time",
			"year" => $" DATE_PART('year', {arguments[0]})",
			"month" => $" DATE_PART('month', {arguments[0]})",
			"day" => $" DATE_PART('day', {arguments[0]})",
			"hour" => $" DATE_PART('hour', {arguments[0]})",
			"minute" => $" DATE_PART('minute', {arguments[0]})",
			"second" => $" DATE_PART('second', {arguments[0]})",
			"totaloffsetminutes" => $" DATE_PART('timezone_minute', {arguments[0]})",
			"totalseconds" => $" EXTRACT(EPOCH FROM {arguments[0]}::time",
			"ceiling" => $" ceiling({arguments[0]})",
			"floor" => $" floor({arguments[0]})",
			"round" => $" round({arguments[0]})",
			"tolower" => $" lower({arguments[0]})",
			"toupper" => $" upper({arguments[0]})",
			"length" => $" length({arguments[0]})",
			"geo.distance" => $" ST_Distance({arguments[0]}, {arguments[1].Replace("SRID=4326;", string.Empty)})",
			"geo.intersects" => $" ST_Intersects({arguments[0]}, {arguments[1]}::boolean)",
			_ => $"{functionName}({argumentNodes.Aggregate(string.Empty, (current, queryNode) => string.Concat(current, string.IsNullOrEmpty(current) ? null : ",", TranslateNode(queryNode)))})"
		};

	private string HandleNameMappedFunction(FieldMap map, string functionName, List<string> arguments, IEnumerable<QueryNode> argumentNodes)
	{
		arguments[0] = map.MappedFieldName;
		return HandleFunction(functionName, arguments, argumentNodes);
	}
	
	// ReSharper disable once CyclomaticComplexity
	private string HandleReferenceMappedFunction(FieldMap map, string functionName, List<string> arguments, IEnumerable<QueryNode> argumentNodes)
	{
		arguments[0] = map.MappedFieldName;
		return functionName.ToLowerInvariant() switch
		{
			"startswith" => $" {arguments[0]} in (select id from reference where {map.ReferenceFieldName} LIKE {arguments[1].Replace(arguments[1].Split("'")[1], $"{arguments[1].Split("'")[1]}%")})",
			"endswith" => $" {arguments[0]} in (select id from reference where {map.ReferenceFieldName} LIKE {arguments[1].Replace(arguments[1].Split("'")[1], $"%{arguments[1].Split("'")[1]}")})",
			"contains" => $" {arguments[0]} in (select id from reference where {map.ReferenceFieldName} LIKE {arguments[1].Replace(arguments[1].Split("'")[1], $"%{arguments[1].Split("'")[1]}%")})",
			"trim" => $" {arguments[0]} in (select id from reference where TRIM({map.ReferenceFieldName}))",
			"date" => $" {arguments[0]} in (select id from reference where CAST({map.ReferenceFieldName} AS DATE))",
			"time" => $" {arguments[0]} in (select id from reference where{map.ReferenceFieldName}::time)",
			"year" => $" {arguments[0]} in (select id from reference where DATE_PART(year, {map.ReferenceFieldName}))",
			"month" => $" {arguments[0]} in (select id from reference where DATE_PART(month, {map.ReferenceFieldName}))",
			"day" => $" {arguments[0]} in (select id from reference where DATE_PART(day, {map.ReferenceFieldName}))",
			"hour" => $" {arguments[0]} in (select id from reference where DATE_PART(hour, {map.ReferenceFieldName}))",
			"minute" => $" {arguments[0]} in (select id from reference where DATE_PART(minute, {map.ReferenceFieldName}))",
			"second" => $" {arguments[0]} in (select id from reference where DATE_PART(second, {map.ReferenceFieldName}))",
			"totaloffsetminutes" => $" {arguments[0]} in (select id from reference where DATE_PART(timezone_minute, {map.ReferenceFieldName}))",
			"totalseconds" => $" {arguments[0]} in (select id from reference where EXTRACT(EPOCH FROM {map.ReferenceFieldName}::time)",
			"ceiling" => $" {arguments[0]} in (select id from reference where ceiling({map.ReferenceFieldName}))",
			"floor" => $" {arguments[0]} in (select id from reference where floor({map.ReferenceFieldName}))",
			"round" => $" {arguments[0]} in (select id from reference where round({map.ReferenceFieldName}))",
			"tolower" => $" {arguments[0]} in (select id from reference where lower({map.ReferenceFieldName}))",
			"toupper" => $" {arguments[0]} in (select id from reference where upper({map.ReferenceFieldName}))",
			"length" => $" {arguments[0]} in (select id from reference where length({map.ReferenceFieldName}))",
			"geo.distance" => $" {arguments[0]} in (select id from reference where ST_Distance({map.ReferenceFieldName}, {arguments[1].Replace("SRID=4326;", string.Empty)}){_suffix}",
			"geo.intersects" => $" {arguments[0]} in (select id from reference where ST_Intersects({map.ReferenceFieldName}, {arguments[1]})::boolean){_suffix}",
			_ => $"{functionName}({argumentNodes.Aggregate(string.Empty, (current, queryNode) => string.Concat(current, string.IsNullOrEmpty(current) ? null : ",", TranslateNode(queryNode)))}))"
		};
	}
	
	/// <summary>
	/// Build BinaryOperatorNode to uri
	/// </summary>
	/// <param name="operatorKind">the kind of the BinaryOperatorNode</param>
	/// <returns>String format of the operator</returns>
	private static string? BinaryOperatorNodeToString(BinaryOperatorKind operatorKind)
	{
		return operatorKind switch
		{
			BinaryOperatorKind.Has => "has",
			BinaryOperatorKind.Equal => "=",
			BinaryOperatorKind.NotEqual => "<>",
			BinaryOperatorKind.GreaterThan => ">",
			BinaryOperatorKind.GreaterThanOrEqual => ">=",
			BinaryOperatorKind.LessThan => "<",
			BinaryOperatorKind.LessThanOrEqual => "<=",
			BinaryOperatorKind.And => "and",
			BinaryOperatorKind.Or => "or",
			BinaryOperatorKind.Add => "add",
			BinaryOperatorKind.Subtract => "sub",
			BinaryOperatorKind.Multiply => "mul",
			BinaryOperatorKind.Divide => "div",
			BinaryOperatorKind.Modulo => "mod",
			_ => null
		};
	}

	/// <summary>
	/// Get the priority of BinaryOperatorNode
	/// This priority table is from http://docs.oasis-open.org/odata/odata/v4.0/odata-v4.0-part2-url-conventions.html (5.1.1.9 Operator Precedence )
	/// </summary>
	/// <param name="operatorKind">binary operator </param>
	/// <returns>the priority value of the binary operator</returns>
	private static int DetermineBinaryOperatorPriority(BinaryOperatorKind operatorKind)
	{
		return operatorKind switch
		{
			BinaryOperatorKind.Or => 1,
			BinaryOperatorKind.And => 2,
			BinaryOperatorKind.Equal => 3,
			BinaryOperatorKind.NotEqual => 3,
			BinaryOperatorKind.GreaterThan => 3,
			BinaryOperatorKind.GreaterThanOrEqual => 3,
			BinaryOperatorKind.LessThan => 3,
			BinaryOperatorKind.LessThanOrEqual => 3,
			BinaryOperatorKind.Add => 4,
			BinaryOperatorKind.Subtract => 4,
			BinaryOperatorKind.Divide => 5,
			BinaryOperatorKind.Multiply => 5,
			BinaryOperatorKind.Modulo => 5,
			BinaryOperatorKind.Has => 6,
			_ => -1
		};
	}

	/// <summary>
	/// Determine if the string is a valid search word.
	/// </summary>
	/// <param name="text">string text to be judged</param>
	/// <returns>if the string is a valid SearchWord, return true, or return false.</returns>
	private static bool IsValidSearchWord(string text)
	{
		var match = InvalidWordPattern.Match(text);
		return !match.Success && !string.Equals(text, "AND", StringComparison.Ordinal) && !string.Equals(text, "OR", StringComparison.Ordinal) && !string.Equals(text, "NOT", StringComparison.Ordinal);
	}

	private static bool ConvertToDateTimeUtc(string dateTimeString, out DateTime? dateTime)
	{
		if (DateTime.TryParse(dateTimeString, CultureInfo.InvariantCulture.DateTimeFormat, DateTimeStyles, out var dateTimeValue))
		{
			dateTime = dateTimeValue;
			return true;
		}

		dateTime = null;
		return false;
	}
}
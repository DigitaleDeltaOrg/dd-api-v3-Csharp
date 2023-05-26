namespace DatabaseLayer.Utility;

using Microsoft.AspNetCore.OData.Query;
using Microsoft.OData;
using Microsoft.OData.UriParser;

public class OrderByMapper
{
	private readonly Dictionary<string, string> _map;
	public OrderByMapper(Dictionary<string, string> map)
	{
		_map = map;
	}
	
	public string CreateOrderByClause(OrderByQueryOption? order)
	{
		var parts = new List<string>();
		if (order != null)
		{
			foreach (var orderByNode in order.OrderByNodes)
			{
				if (orderByNode is OrderByPropertyNode orderByPropertyNode)
				{
					parts.Add($"{MapOrderByParameterName(orderByPropertyNode.Property.Name)} {(orderByNode.Direction == OrderByDirection.Ascending ? " asc " : " desc")}");
				}
				else
				{
					throw new ODataException("Only ordering by properties is supported");
				}
			}
		}

		if (!parts.Any())
		{
			parts.Add("Id asc");
		}
		
		return string.Join(",", parts);
	}
	
	private string? MapOrderByParameterName(string propertyName)
	{
		return _map.TryGetValue(propertyName, out var mappedName) ? mappedName : propertyName;
	}
}
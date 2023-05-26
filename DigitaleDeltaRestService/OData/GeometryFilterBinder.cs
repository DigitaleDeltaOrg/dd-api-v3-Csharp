namespace DigitaleDeltaRestService.OData;

using System.Linq.Expressions;
using Microsoft.AspNetCore.OData.Query.Expressions;
using Microsoft.OData.UriParser;
using Microsoft.Spatial;
using NetTopologySuite;
using NetTopologySuite.Geometries;

//using Geometry = Microsoft.Spatial.Geometry;

public class GeometryFilterBinder : FilterBinder
{
	private const string GeoDistanceFunctionName = "geo.distance";
	public override Expression BindSingleValueFunctionCallNode(SingleValueFunctionCallNode node, QueryBinderContext context)
	{
		return node.Name switch
		{
			GeoDistanceFunctionName => BindGeoDistance(node, context),
			var _ => base.BindSingleValueFunctionCallNode(node, context)
		};
	}

	private Expression BindGeoDistance(SingleValueFunctionCallNode node, QueryBinderContext context)
	{
		Expression[] arguments    = BindArguments(node.Parameters, context);
		string?       propertyName = null;

		foreach (var queryNode in node.Parameters)
		{
			if (queryNode is SingleValuePropertyAccessNode svpan)
			{
				propertyName = svpan.Property.Name;
			}
		}

		if (propertyName == null)
		{
			return Expression.Constant(false);
		}
		
		var points   = GetPointExpressions(arguments);
		var distance = points.First().Distance(points.Last());
		var ex       = Expression.Constant(distance);

		return ex;
	}

	private static Point[] GetPointExpressions(Expression[] expressions)
	{
		var points = new List<Point>();

		foreach (var expression in expressions)
		{
			if (expression is not MemberExpression memberExpr)
			{
				return points.ToArray();
			}

			if (memberExpr.Expression is not ConstantExpression constantExpr)
			{
				continue;
			}

			var point = GetPointFromConstantExpression(constantExpr);
			if (point != null)
			{
				points.Add(point);
			}
		}

		return points.ToArray();
	}

	private static Point? GetPointFromConstantExpression(ConstantExpression? expression)
	{
		Point? point = null;
		if (expression == null)
		{
			return point;
		}

		var constantExpressionValuePropertyInfo = expression.Type.GetProperty("ObservableParameter");
		if (constantExpressionValuePropertyInfo == null)
		{
			return null;
		}
			
		var oDataPoint = constantExpressionValuePropertyInfo.GetValue(expression.Value) as GeometryPoint;
		if (oDataPoint?.X == null)
		{
			return null;
		}

		point = CreatePoint(oDataPoint.X, oDataPoint.Y);

		return point;
	}

	private static Point CreatePoint(double latitude, double longitude)
	{
		// 4326 is most common coordinate system used by GPS/Maps
		var geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(4326);
		var newLocation     = geometryFactory.CreatePoint(new Coordinate(longitude, latitude));

		return newLocation;
	}
}
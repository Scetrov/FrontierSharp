using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using FrontierSharp.Mud.Linq.Attributes;

namespace FrontierSharp.Mud.Linq;

	public class MudSqlExpressionVisitor : ExpressionVisitor {
		private readonly StringBuilder _sb = new();

		public string Translate(Expression expression, Type entityType) {
			_sb.Clear();

			if (entityType.IsGenericType && entityType.GetGenericTypeDefinition() == typeof(IGrouping<,>)) {
				VisitGroupBy(expression);
				return $"{_sb};";
			}
			
			VisitUngroupedSelect(entityType);

			Visit(expression);
			
			return $"{_sb};";
		}

		private void VisitUngroupedSelect(Type entityType) {
			_sb.AppendToken("SELECT");
			var columns = string.Join(", ", entityType
				.GetProperties(BindingFlags.Public | BindingFlags.Instance)
				.Select(PropertyToColumnName));
			_sb.AppendToken(columns);
			_sb.AppendToken("FROM");
			_sb.AppendToken(ClassToTableName(entityType));
		}

		private void VisitGroupBy(Expression expression) {
			// Handle GroupBy queries
			var expr = (MethodCallExpression)expression;
			var unary = (UnaryExpression)expr.Arguments[1];
			var lambda = (LambdaExpression)StripQuotes(unary);
			var body = (MemberExpression)lambda.Body;
				
			_sb.AppendToken("SELECT");
			_sb.AppendToken(PropertyToColumnName(body.Member as PropertyInfo));
			_sb.AppendToken("FROM");
			_sb.AppendToken(ClassToTableName(lambda.Parameters[0].Type));
			_sb.AppendToken("GROUP BY");
			_sb.AppendToken(PropertyToColumnName(body.Member as PropertyInfo));
		}

		private string ClassToTableName(Type entityType) {
			var attribute = entityType.GetCustomAttribute<MudTableAttribute>();
			
			if (attribute == null) {
				return $"\"{entityType.Name}\"";	
			}
			
			var tableName = string.IsNullOrWhiteSpace(entityType.Name) ? attribute.Name : entityType.Name;

			return $"\"{attribute.Namespace}__{tableName}\"";
		}
		
		private string PropertyToColumnName(PropertyInfo? p) {
			if (p is null) {
				ArgumentNullException.ThrowIfNull(p);
			}

			if (p.GetCustomAttribute<MudColumnAttribute>() is not { } columnAttribute) {
				return $"\"{p.Name}\"";
			}
		
			return $"\"{columnAttribute.ColumnName}\"";
		}

		protected override Expression VisitMethodCall(MethodCallExpression node) {
			Visit(node.Arguments[0]);
			switch (node.Method.Name) {
				case "Where": {
					_sb.AppendToken("WHERE");
					var lambda = (LambdaExpression)StripQuotes(node.Arguments[1]);
					Visit(lambda.Body);
					return node;
				}
				case "OrderBy": {
					_sb.AppendToken("ORDER BY");
					var lambda = (LambdaExpression)StripQuotes(node.Arguments[1]);
					Visit(lambda.Body);
					return node;
				}
				case "Take": {
					_sb.AppendToken("LIMIT");
					var arg = (ConstantExpression)StripQuotes(node.Arguments[1]);
					Visit(arg);
					return node;
				}
				case "Skip": {
					_sb.AppendToken("OFFSET");
					var arg = (ConstantExpression)StripQuotes(node.Arguments[1]);
					Visit(arg);
					return node;
				}
				default:
					return base.VisitMethodCall(node);
			}
		}

		protected override Expression VisitBinary(BinaryExpression node) {
			Visit(node.Left);
			_sb.AppendToken($"{GetSqlOperator(node.NodeType)}");
			Visit(node.Right);
			return node;
		}

		protected override Expression VisitMember(MemberExpression node) {
			_sb.AppendToken(PropertyToColumnName(node.Member as PropertyInfo));
			return node;
		}

		protected override Expression VisitConstant(ConstantExpression node) {
			if (node.Type.IsGenericType &&
					node.Type.GetGenericTypeDefinition() == typeof(MudQueryable<>)) {
				// Skip adding anything to SQL — just return the expression
				return node;
			}

			if (node.Type == typeof(string)) {
				_sb.AppendToken($"'{node.Value}'");
			} else {
				_sb.AppendToken(node.Value);
			}

			return node;
		}

		private static string GetSqlOperator(ExpressionType type) => type switch {
			ExpressionType.Equal => "=",
			ExpressionType.GreaterThan => ">",
			ExpressionType.LessThan => "<",
			_ => throw new NotSupportedException()
		};

		private static Expression StripQuotes(Expression e) {
			while (e.NodeType == ExpressionType.Quote)
				e = ((UnaryExpression)e).Operand;
			return e;
		}
	}
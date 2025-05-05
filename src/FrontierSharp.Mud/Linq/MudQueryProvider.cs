using System.Collections;
using System.Linq.Expressions;

namespace FrontierSharp.Mud.Linq;

public class MudQueryProvider : IQueryProvider {
    public IQueryable CreateQuery(Expression expression) {
        var elementType = expression.Type.GetGenericArguments()[0];
        var instance = Activator.CreateInstance(typeof(MudQueryable<>).MakeGenericType(elementType), this, expression);
        return (IQueryable)instance! ?? throw new InvalidOperationException("Failed to create instance of MudQueryable");
    }

    public IQueryable<TElement> CreateQuery<TElement>(Expression expression) {
        return new MudQueryable<TElement>(this, expression);
    }

    public object Execute(Expression expression) {
        return Execute<object>(expression);
    }

    public TResult Execute<TResult>(Expression expression) {
        var elementType = typeof(TResult).GetInterfaces().Contains(typeof(IEnumerable)) && typeof(TResult).IsGenericType
            ? typeof(TResult).GetGenericArguments()[0]
            : typeof(TResult);

        var translator = new MudSqlExpressionVisitor();
        var sql = translator.Translate(expression, elementType);

        //var result = _connection.Query<TResult>(sql);
        return (TResult)(object)null;
        // not implement
    }
}
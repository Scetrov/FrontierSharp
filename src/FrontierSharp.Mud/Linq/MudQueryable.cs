using System.Collections;
using System.Linq.Expressions;

namespace FrontierSharp.Mud.Linq;

public class MudQueryable<T> : IOrderedQueryable<T> {
    public MudQueryable(MudQueryProvider provider) {
        Provider = provider;
        Expression = Expression.Constant(this);
    }

    public MudQueryable(MudQueryProvider provider, Expression expression) {
        Provider = provider;
        Expression = expression;
    }

    public Expression Expression { get; }
    public Type ElementType => typeof(T);
    public IQueryProvider Provider { get; }

    public IEnumerator<T> GetEnumerator() {
        var result = Provider.Execute<IEnumerable<T>>(Expression);
        return result.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return GetEnumerator();
    }
}
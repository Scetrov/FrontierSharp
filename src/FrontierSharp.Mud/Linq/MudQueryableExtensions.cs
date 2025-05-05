using System.Text;

namespace FrontierSharp.Mud.Linq;

public static class MudQueryableExtensions {
    public static string ToSql<T>(this IQueryable<T> queryable) {
        if (queryable.Provider is not MudQueryProvider) {
            throw new InvalidOperationException("Queryable is not based on MudQueryProvider");
        }
        
        var translator = new MudSqlExpressionVisitor();
        var sql = translator.Translate(queryable.Expression, typeof(T));
        return sql;
    }

    public static StringBuilder AppendToken<T>(this StringBuilder builder, T token) {
        if (builder.Length == 0) {
            builder.Append(token);
            return builder;
        }
        
        if (builder[^1] != ' ') {
            builder.Append(' ');
        }
        
        builder.Append(token);
        return builder;
    }
}
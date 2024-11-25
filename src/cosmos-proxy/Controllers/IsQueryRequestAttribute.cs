using Microsoft.AspNetCore.Mvc.ActionConstraints;

public class IsQueryRequestAttribute : Attribute, IActionConstraint
{
    public int Order => 0;

    public bool Accept(ActionConstraintContext context)
    {
        var queryValue = context.RouteContext.HttpContext.Request.Headers.TryGetValue("x-ms-documentdb-query", out var query)
            ? query.ToString()
            : context.RouteContext.HttpContext.Request.Headers.TryGetValue("x-ms-documentdb-isquery", out var isQuery)
                ? isQuery.ToString()
                : null;

        return string.Equals(queryValue, "true", StringComparison.OrdinalIgnoreCase);
    }
}
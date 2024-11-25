using Microsoft.AspNetCore.Mvc.ActionConstraints;

public class IsQueryRequestAttribute : Attribute, IActionConstraint
{
    public int Order => 0;

    public bool Accept(ActionConstraintContext context)
    {
        if(!context.RouteContext.HttpContext.Request.Headers.TryGetValue("x-ms-documentdb-query", out var isQuery)) {
            Console.WriteLine("x-ms-documentdb-query header not found");
            return false;
        }

        Console.WriteLine("x-ms-documentdb-query header == " + isQuery);
        return string.Equals(isQuery.ToString(), "true", StringComparison.OrdinalIgnoreCase);
    }
}
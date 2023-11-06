using Dapr;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;

namespace DaprDemo.Api;

public static class Configure
{
    public static IEndpointConventionBuilder Map(this WebApplication app)
    {
        var root = app.MapGroup("");
        root.MapPost("orders", [Topic("my-pubsub", "Demo")](Data data, [FromServices] Do @do) => @do.SomeMagic(data.Value));

        return root;
    }
}
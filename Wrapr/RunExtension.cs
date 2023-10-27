using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Wrapr;

public static class RunExtension
{
    public static Run Args(this Run run, params string[] arguments) =>
        new(run.Arguments.Concat(arguments));

    [Obsolete(@"Flag --components-path has been deprecated, This flag is deprecated and will be removed in the future releases. Use ""resources-path"" flag instead"), ExcludeFromCodeCoverage]
    public static Run ComponentsPath(this Run run, string path) =>
        run.Args("--components-path", path);
        
    public static Run ResourcesPath(this Run run, string path) =>
        run.Args("--resources-path", path);
        
    public static Run AppPort(this Run run, int port) => 
        run.Args("--app-port", port.ToString());
        
    public static Run DaprGrpcPort(this Run run, int port) => 
        run.Args("--dapr-grpc-port", port.ToString());
        
    public static Run DaprHttpPort(this Run run, int port) => 
        run.Args("--dapr-http-port", port.ToString());
}
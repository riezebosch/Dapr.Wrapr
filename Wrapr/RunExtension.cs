using System.Linq;

namespace Wrapr
{
    public static class RunExtension
    {
        public static Run Args(this Run run, params string[] arguments) =>
            run with { Arguments = run.Arguments.Concat(arguments) };

        public static Run ComponentsPath(this Run run, string path) =>
            run.Args("--components-path", path);
        
        public static Run AppPort(this Run run, int port) => 
            run.Args("--app-port", port.ToString());
        
        public static Run DaprGrpcPort(this Run run, int port) => 
            run.Args("--dapr-grpc-port", port.ToString());
    }
}
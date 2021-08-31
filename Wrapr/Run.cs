using System.Collections.Generic;

namespace Wrapr
{
    public record Run(IEnumerable<string> Arguments)
    {
        public static Run Create(string appId) =>
            new(new[] { "run", "--app-id", appId });
    }
}
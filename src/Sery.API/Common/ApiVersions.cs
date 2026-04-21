using Asp.Versioning;

namespace Sery.API.Common;

public static class ApiVersions
{
    public const string V1 = "1.0";

    public static readonly ApiVersion Version1 = new(1, 0);
}

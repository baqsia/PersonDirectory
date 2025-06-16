using System.Reflection;

namespace Task.PersonDirectory.Application.Common;

public static class AssemblyMarker
{
    public static Assembly Assembly => typeof(AssemblyMarker).Assembly;
}
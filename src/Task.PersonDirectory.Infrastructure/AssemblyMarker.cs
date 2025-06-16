using System.Reflection;

namespace Task.PersonDirectory.Infrastructure;

public static class AssemblyMarker
{
    public static Assembly Assembly => typeof(AssemblyMarker).Assembly;
}
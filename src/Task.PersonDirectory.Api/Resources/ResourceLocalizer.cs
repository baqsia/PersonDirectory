using Microsoft.Extensions.Localization;
using Task.PersonDirectory.Application.Common;
using Task.PersonDirectory.Application.Services;

namespace Task.PersonDirectory.Api.Resources;

public class ResourceLocalizer: IResourceLocalizer
{
    private readonly IStringLocalizer _localizer;
    public ResourceLocalizer(IStringLocalizerFactory factory)
    {
        var type = typeof(SharedResource);
        var assemblyName = type.Assembly.GetName().Name!; 
        _localizer = factory.Create("SharedResource", assemblyName);
    }
    
    public string Localize(string key)
    {
        return _localizer[key];
    }
}
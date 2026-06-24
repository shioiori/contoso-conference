using Microsoft.Extensions.Localization;

namespace Eventbox.Notification.Api.Resources;

public interface IResourceLocalizer<T> where T : class
{
    string this[string key] { get; }
}

public class ResourceLocalizer<T>(IStringLocalizerFactory factory) : IResourceLocalizer<T> where T : class
{
    private readonly IStringLocalizer _localizer = factory.Create(typeof(T));

    public string this[string key] => _localizer[key];
}

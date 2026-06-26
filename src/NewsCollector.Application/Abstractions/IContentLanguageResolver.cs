using NewsCollector.Domain.Entities;
using NewsCollector.Domain.Enums;

namespace NewsCollector.Application.Abstractions;

public interface IContentLanguageResolver
{
    ContentLanguage Resolve(Source source, string title, string? summary = null, string? content = null);
}

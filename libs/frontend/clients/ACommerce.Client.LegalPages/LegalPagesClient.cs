using ACommerce.Client.Core.Http;

namespace ACommerce.Client.LegalPages;

public sealed class LegalPagesClient(IApiClient httpClient)
{
    private const string ServiceName = "Marketplace";

    public async Task<IEnumerable<LegalPageDto>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        var result = await httpClient.GetAsync<IEnumerable<LegalPageDto>>(ServiceName, "/api/legalpages", cancellationToken);
        return result ?? [];
    }

    public async Task<IEnumerable<LegalPageDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var result = await httpClient.GetAsync<IEnumerable<LegalPageDto>>(ServiceName, "/api/legalpages/all", cancellationToken);
        return result ?? [];
    }

    public async Task<LegalPageDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await httpClient.GetAsync<LegalPageDto>(ServiceName, $"/api/legalpages/{id}", cancellationToken);
    }

    public async Task<LegalPageDto?> GetByKeyAsync(string key, CancellationToken cancellationToken = default)
    {
        return await httpClient.GetAsync<LegalPageDto>(ServiceName, $"/api/legalpages/key/{key}", cancellationToken);
    }
}

public sealed class LegalPageDto
{
    public Guid Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string TitleAr { get; set; } = string.Empty;
    public string TitleEn { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
}

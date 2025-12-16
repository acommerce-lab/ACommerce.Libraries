using ACommerce.LegalPages.DTOs;

namespace ACommerce.LegalPages.Contracts;

public interface ILegalPagesService
{
    Task<IEnumerable<LegalPageDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<LegalPageDto>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<LegalPageDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<LegalPageDto?> GetByKeyAsync(string key, CancellationToken cancellationToken = default);
    Task<LegalPageDto> CreateAsync(CreateLegalPageRequest request, CancellationToken cancellationToken = default);
    Task<LegalPageDto?> UpdateAsync(Guid id, UpdateLegalPageRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

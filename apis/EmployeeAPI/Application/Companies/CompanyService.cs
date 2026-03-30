using System.Text.RegularExpressions;
using EmployeeAPI.Contracts.Companies;
using EmployeeAPI.Domain.Entities;
using EmployeeAPI.Mappings;
using Microsoft.Extensions.Logging;
using Shared.Security.Net.Auditing;

namespace EmployeeAPI.Application.Companies;

public class CompanyService
{
    private static readonly Regex ShortCodePattern = new("^[A-Z0-9]{10}$", RegexOptions.Compiled);
    private static readonly string[] ReservedShortCodeFragments = ["auth"];

    private readonly ILogger<CompanyService> _logger;
    private readonly ICompanyRepository _companyRepository;
    private readonly IAuditLogRepository _auditLogRepository;

    public CompanyService(
        ILogger<CompanyService> logger,
        ICompanyRepository companyRepository,
        IAuditLogRepository auditLogRepository)
    {
        _logger = logger;
        _companyRepository = companyRepository;
        _auditLogRepository = auditLogRepository;
    }

    public Task<List<Company>> GetAllAsync(CancellationToken ct) => _companyRepository.GetCompaniesAsync(ct);

    public Task<Company?> GetByIdAsync(Guid id, CancellationToken ct) => _companyRepository.GetCompanyByIdAsync(id, ct);

    public Task<Company?> GetByShortCodeAsync(string shortCode, CancellationToken ct) =>
        _companyRepository.GetCompanyByShortCodeAsync(shortCode.Trim().ToUpperInvariant(), ct);

    public async Task<CompanyResponse> CreateAsync(CreateCompanyRequest request, CancellationToken ct)
    {
        var normalizedShortCode = NormalizeShortCode(request.ShortCode);
        ValidateShortCode(normalizedShortCode);

        _logger.LogInformation("Creating company {ShortCode}", normalizedShortCode);
        var company = new Company
        {
            ShortCode = normalizedShortCode,
            Name = request.Name.Trim(),
            Industry = request.Industry?.Trim(),
            Email = request.Email?.Trim(),
            Phone = request.Phone?.Trim(),
            CreatedUtc = DateTime.UtcNow,
            UpdatedUtc = DateTime.UtcNow
        };

        await _companyRepository.AddAsync(company, ct);
        await AddAuditLogAsync(
            company.Id,
            "created",
            [nameof(company.ShortCode), nameof(company.Name), nameof(company.Industry), nameof(company.Email), nameof(company.Phone)],
            new Dictionary<string, object?>
            {
                [nameof(company.ShortCode)] = company.ShortCode,
                [nameof(company.Name)] = company.Name,
                [nameof(company.Industry)] = company.Industry,
                [nameof(company.Email)] = company.Email,
                [nameof(company.Phone)] = company.Phone
            },
            ct);
        await _companyRepository.SaveChangesAsync(ct);

        _logger.LogInformation("Created company {CompanyId}", company.Id);
        return company.ToResponse();
    }

    public async Task<CompanyResponse?> PatchAsync(Guid id, PatchCompanyRequest request, CancellationToken ct)
    {
        var company = await _companyRepository.GetForUpdateAsync(id, ct);
        if (company is null)
        {
            _logger.LogWarning("Patch skipped; company {CompanyId} not found", id);
            return null;
        }

        var changedFields = new List<string>();
        var changes = new Dictionary<string, object?>();

        if (request.Name is not null)
        {
            var newValue = request.Name.Trim();
            if (string.IsNullOrWhiteSpace(newValue))
            {
                throw new ArgumentException("Name cannot be empty.", nameof(request));
            }
            if (newValue != company.Name)
            {
                company.Name = newValue;
                changedFields.Add(nameof(company.Name));
                changes[nameof(company.Name)] = newValue;
            }
        }

        if (request.Industry is not null && request.Industry != company.Industry)
        {
            company.Industry = request.Industry?.Trim();
            changedFields.Add(nameof(company.Industry));
            changes[nameof(company.Industry)] = company.Industry;
        }

        if (request.Email is not null && request.Email != company.Email)
        {
            company.Email = request.Email?.Trim();
            changedFields.Add(nameof(company.Email));
            changes[nameof(company.Email)] = company.Email;
        }

        if (request.Phone is not null && request.Phone != company.Phone)
        {
            company.Phone = request.Phone?.Trim();
            changedFields.Add(nameof(company.Phone));
            changes[nameof(company.Phone)] = company.Phone;
        }

        if (changedFields.Count == 0)
        {
            _logger.LogInformation("No changes detected for company {CompanyId}", id);
            return company.ToResponse();
        }

        company.UpdatedUtc = DateTime.UtcNow;
        changes[nameof(company.UpdatedUtc)] = company.UpdatedUtc;

        await AddAuditLogAsync(company.Id, "updated", changedFields, changes, ct);
        await _companyRepository.SaveChangesAsync(ct);

        _logger.LogInformation("Updated company {CompanyId} ({Fields})", id, string.Join(",", changedFields));
        return company.ToResponse();
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct)
    {
        var company = await _companyRepository.GetForUpdateAsync(id, ct);
        if (company is null)
        {
            _logger.LogWarning("Delete skipped; company {CompanyId} not found", id);
            return false;
        }

        company.DeletedUtc = DateTime.UtcNow;
        company.UpdatedUtc = DateTime.UtcNow;

        await AddAuditLogAsync(
            company.Id,
            "deleted",
            null,
            new Dictionary<string, object?>
            {
                [nameof(company.DeletedUtc)] = company.DeletedUtc,
                [nameof(company.UpdatedUtc)] = company.UpdatedUtc
            },
            ct);
        await _companyRepository.SaveChangesAsync(ct);

        _logger.LogInformation("Deleted company {CompanyId}", id);
        return true;
    }

    private static string NormalizeShortCode(string shortCode) => shortCode.Trim().ToUpperInvariant();

    private static void ValidateShortCode(string shortCode)
    {
        if (!ShortCodePattern.IsMatch(shortCode))
        {
            throw new ArgumentException("ShortCode must be 10 uppercase letters or digits.", nameof(shortCode));
        }

        var loweredShortCode = shortCode.ToLowerInvariant();
        foreach (var fragment in ReservedShortCodeFragments)
        {
            if (loweredShortCode.Contains(fragment))
            {
                throw new InvalidOperationException("ShortCode contains a reserved fragment.");
            }
        }
    }

    private async Task AddAuditLogAsync(
        Guid entityId,
        string action,
        IEnumerable<string>? changedFields,
        Dictionary<string, object?>? changes,
        CancellationToken ct)
    {
        await _auditLogRepository.AddAsync(new AuditLog
        {
            EntityType = "EmployeeAPI.Company",
            EntityId = entityId,
            Action = action,
            OccurredUtc = DateTimeOffset.UtcNow,
            PerformedBy = "system",
            ChangedFields = changedFields?.ToList(),
            Changes = changes,
            Note = null
        }, ct);
    }
}

using Shared.InterServiceHelper.Grpc.Identity;

namespace EvaluationService.Application.Contracts;

public interface IIdentityLookupServiceClient
{
    Task<EmployeeResponse?> GetEmployeeByIdAsync(Guid employeeId, CancellationToken cancellationToken = default);
}

using ProgramService.Domain.Entities;
using Shared.Common.Interfaces;

namespace ProgramService.Application.Contracts.IRepository;

/// <summary>
/// Repository interface for TrainingPrograms entity operations.
/// </summary>
public interface IProgramRepository : IRepositoryBase<TrainingPrograms>
{
}


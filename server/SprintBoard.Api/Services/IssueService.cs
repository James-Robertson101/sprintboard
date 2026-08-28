using SprintBoard.Api.DTOs;
using SprintBoard.Api.Repositories;

namespace SprintBoard.Api.Services;

public class IssueService : IIssueService{
  private readonly IIssueRepository _repository;

  public IssueService(IIssueRepository repository)
  {
    _repository = repository;
  }
}
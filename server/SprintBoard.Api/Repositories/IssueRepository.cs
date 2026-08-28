namespace SprintBoard.Api.Repositories;
using SprintBoard.Api.DTOs;
using SprintBoard.Api.Models;
using SprintBoard.Api.Data;

public class IssueRepository : IIssueRepository
{
   private readonly AppDbContext _db;

   public IssueRepository(AppDbContext db)
  {
    _db = db;
  }
}
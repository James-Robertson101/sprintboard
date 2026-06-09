namespace SprintBoard.Api.Models;

public enum ProjectRole
{
    Owner,   // can invite, remove members, delete project
    Member   // can view, comment, move tasks
}
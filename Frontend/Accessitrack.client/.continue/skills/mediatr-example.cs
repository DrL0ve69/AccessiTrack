namespace PortfolioClean.Application.Features.Projects.Commands;

// Use Primary Constructors for DI
public sealed record CreateProjectCommand(string Title, string Slug) : IRequest<Result<Guid>>;

public sealed class CreateProjectValidator : AbstractValidator<CreateProjectCommand>
{
    public CreateProjectValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Slug).NotEmpty().Matches(@"^[a-z0-9-]+$");
    }
}

internal sealed class CreateProjectHandler(IProjectRepository repository, IUnitOfWork unitOfWork) 
    : IRequestHandler<CreateProjectCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateProjectCommand request, CancellationToken ct)
    {
        // Logic stays in Domain Entity (hypothetical Factory method)
        var project = Project.Create(request.Title, request.Slug);
        
        repository.Add(project);
        await unitOfWork.SaveChangesAsync(ct);
        
        return Result.Success(project.Id);
    }
}
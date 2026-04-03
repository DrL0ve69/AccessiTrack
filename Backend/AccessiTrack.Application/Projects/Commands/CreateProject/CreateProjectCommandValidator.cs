using System;
using System.Collections.Generic;
using System.Text;

using FluentValidation;

namespace AccessiTrack.Application.Projects.Commands.CreateProject;

public class CreateProjectCommandValidator
    : AbstractValidator<CreateProjectCommand>
{
    public CreateProjectCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Le nom est obligatoire.")
            .MaximumLength(200);

        RuleFor(x => x.TargetUrl)
            .NotEmpty()
            .Must(url => Uri.TryCreate(url, UriKind.Absolute, out _))
            .WithMessage("L'URL doit être une URL valide (ex: https://exemple.com).");

        RuleFor(x => x.ClientName)
            .NotEmpty().WithMessage("Le nom du client est obligatoire.");
    }
}

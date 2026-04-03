using System;
using System.Collections.Generic;
using System.Text;

namespace AccessiTrack.Application.Common.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException(string entityName, Guid id)
        : base($"{entityName} avec l'identifiant '{id}' est introuvable.") { }
}

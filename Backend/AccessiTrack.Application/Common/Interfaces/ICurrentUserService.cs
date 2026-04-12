using System;
using System.Collections.Generic;
using System.Text;

namespace AccessiTrack.Application.Common.Interfaces;

public interface ICurrentUserService
{
    string UserId { get; }
    bool IsAdmin { get; }
}

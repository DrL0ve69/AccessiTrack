using System;
using System.Collections.Generic;
using System.Text;

namespace AccessiTrack.Domain.Enums;

public enum ViolationSeverity
{
    Critical = 1,   // Bloque complètement l'accès (ex: image sans alt sur un bouton)
    Major = 2,      // Rend l'usage très difficile
    Minor = 3       // Gêne mais contournement possible
}

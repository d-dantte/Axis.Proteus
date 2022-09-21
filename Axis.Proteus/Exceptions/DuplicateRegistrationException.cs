using System;

namespace Axis.Proteus.Exceptions
{
    [Obsolete]
    public class DuplicateRegistrationException: Exception
    {
        public Type DuplicateType { get; }

        public DuplicateRegistrationException(Type duplicateType)
        : base($"Duplicate type registration detected for: {duplicateType}")
        {
            DuplicateType = duplicateType ?? throw new ArgumentNullException(nameof(duplicateType));
        }
    }
}

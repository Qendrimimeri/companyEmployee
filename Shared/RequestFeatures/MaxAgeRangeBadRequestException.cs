using Entities.Exceptions;

namespace Shared.RequestFeatures;

public sealed class MaxAgeRangeBadRequestException : BadRequestException
{
    public MaxAgeRangeBadRequestException()
    : base("Max age can't be less than min age.")
    {
    }
}

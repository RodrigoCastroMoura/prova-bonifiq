﻿namespace ProvaPub.Domain.Services
{
    public interface IDateTimeProvider
    {
        DateTime UtcNow { get; }
    }

}

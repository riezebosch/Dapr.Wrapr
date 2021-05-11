using System;

namespace DaprDemo.Events
{
    public record CloudEvent<TData>(Guid Id, TData Data);
}
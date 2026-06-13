using System;
using System.Collections.Generic;
using System.Text;

namespace Eventbox.EventBus.Core.Abstractions
{
    public interface ICommandBus
    {
        Task SendAsync<TCommand>(TCommand command, CancellationToken cancellationToken = default)
            where TCommand : ICommand;
    }

    public interface ICommand
    {
        Guid CommandId { get; }
    }

}

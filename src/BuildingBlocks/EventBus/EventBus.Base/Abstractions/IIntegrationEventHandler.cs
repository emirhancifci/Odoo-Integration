using EventBus.Base.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventBus.Base.Abstractions
{
    public interface IIntegrationEventHandler<TEvent> : IEventHandler where TEvent : IntegrationEvent
    {
        Task Handle(TEvent @event);
    }
}

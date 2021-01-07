using System;
using System.Linq;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using System.Threading.Tasks;
using System.Threading;

namespace src
{
    class Program
    {
        static void Main(string[] args)
        {

            var serviceProvider = new ServiceCollection()
            .AddMediatR(Assembly.GetExecutingAssembly())
            .BuildServiceProvider();

            var mediator = serviceProvider.GetService<IMediator>();

            var evt = new EventMessage{
                  Domain = Domain.Guest,
                  Event = Event.Changed
            };

            var factory = new CustomEventFactory();

            var result = mediator.Send(factory.Cast(evt));

            evt = new EventMessage{
                  Domain = Domain.Reservation,
                  Event = Event.Changed
            };

            result = mediator.Send(factory.Cast(evt));
        }
    }

    public enum Domain {
        Reservation,
        Guest
    }

    public enum Event {
        Changed
    }

    public class EventMessage : IRequest {
        public Domain Domain { get; set; }  
        public Event Event { get; set; }

        public EventMessage()
        {
            
        }

        public EventMessage(EventMessage m)
        {
            Domain = m.Domain;
            Event = m.Event;
        }
    }

    public class GuestChangedEvent : EventMessage, IRequest {

        public string MyProperty { get; set; } = "Guest";

        public GuestChangedEvent(EventMessage m) : base(m){ }

    }

    public class GuestChangedEventHandler : AsyncRequestHandler<GuestChangedEvent>
    {
        protected override Task Handle(GuestChangedEvent request, CancellationToken cancellationToken)
        {
            Console.WriteLine("Handled guest");
            return Task.CompletedTask;
        }
    }      

    public class GenericEventHandler : AsyncRequestHandler<EventMessage>
    {
        protected override Task Handle(EventMessage request, CancellationToken cancellationToken)
        {
            Console.WriteLine("Handled generic");
            return Task.CompletedTask;
        }
    }     

    public class CustomEventFactory {
        public EventMessage Cast(EventMessage value) {
            return
            this.GetType().Assembly.GetTypes()
            .Where(t=>t.IsSubclassOf(typeof(EventMessage)))
            .Where(t=>t.Name == $"{value.Domain.ToString()}{value.Event.ToString()}Event")
            .Select(t=>(EventMessage)Activator.CreateInstance(t, value))
            .DefaultIfEmpty(value)
            .FirstOrDefault();
        }
    }
}

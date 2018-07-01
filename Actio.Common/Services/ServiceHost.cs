using Actio.Common.Commands;
using Actio.Common.Events;
using Actio.Common.RabbitMq;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using RawRabbit;
using System;

namespace Actio.Common.Services
{
    public class ServiceHost : IServiceHost
    {
        private readonly IWebHost Host;

        public ServiceHost(IWebHost webHost)
        {
            Host = webHost;
        }

        public void Run() => Host.Run();

        public static HostBuilder Create<TStartup>(string[] args) where TStartup : class
        {
            Console.Title = typeof(TStartup).Namespace;
            var config = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .AddCommandLine(args).Build();
            var webHostBuilder = WebHost.CreateDefaultBuilder()
                .UseConfiguration(config)
                .UseStartup<TStartup>();

            return new HostBuilder(webHostBuilder.Build());
        }

        public abstract class BuilderBase
        {
            public abstract ServiceHost Build();
        }

        public class HostBuilder : BuilderBase
        {
            private readonly IWebHost Host;
            private IBusClient Bus { get; set; }

            public HostBuilder(IWebHost webHost)
            {
                Host = webHost;
            }

            public BusBuilder UseRabbitMq()
            {
                Bus = (IBusClient)Host.Services.GetService(typeof(IBusClient));
                return new BusBuilder(Host, Bus);
            }

            public override ServiceHost Build()
            {
                return new ServiceHost(Host);
            }
        }

        public class BusBuilder : BuilderBase
        {
            private readonly IWebHost WebHost;
            private IBusClient Bus { get; set; }

            public BusBuilder(IWebHost webHost, IBusClient bus)
            {
                WebHost = webHost;
                Bus = bus;
            }

            public BusBuilder SubscribeToCommand<TCommand>() where TCommand : ICommand
            {
                var handler = (ICommandHandler<TCommand>)WebHost.Services.GetService(typeof(ICommandHandler<TCommand>));
                Bus.WithCommandHandlerAsync(handler);

                return this;
            }

            public BusBuilder SubscribeToEvent<TEvent>() where TEvent : IEvent
            {
                var handler = (IEventHandler<TEvent>)WebHost.Services.GetService(typeof(IEventHandler<TEvent>));
                Bus.WithEventHandlerAsync(handler);

                return this;
            }

            public override ServiceHost Build()
            {
                return new ServiceHost(WebHost);
            }
        }
    }
}
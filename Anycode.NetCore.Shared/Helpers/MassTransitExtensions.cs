using MassTransit;
using RabbitMQ.Client;

namespace Anycode.NetCore.Shared.Helpers;

public static class MassTransitExtensions
{
	public static void AddMassTransit(this IServiceCollection services, string rabbitMqUrl,
		Action<IBusRegistrationConfigurator> configureMt, Action<IRabbitMqBusFactoryConfigurator> configureRabbit)
	{
		services.AddMassTransit(config =>
		{
			config.SetKebabCaseEndpointNameFormatter();

			configureMt(config);

			config.UsingRabbitMq((busContext, rabbitConfig) =>
			{
				rabbitConfig.UseInMemoryOutbox(busContext);
				configureRabbit(rabbitConfig);

				rabbitConfig.Host(new Uri(rabbitMqUrl));

				rabbitConfig.UseMessageRetry(r => { r.Incremental(3, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(5)); });

				rabbitConfig.ConfigureEndpoints(busContext);
			});
		});
	}

	/// <summary>
	/// Setup publisher to create queue and start sending messages
	/// </summary>
	public static void SetupPublisher<T>(this IRabbitMqBusFactoryConfigurator configurator, string exchangeName)
		where T : class
	{
		configurator.Message<T>(x => x.SetEntityName(exchangeName));
		configurator.Publish<T>(x =>
		{
			x.ExchangeType = ExchangeType.Fanout;
			x.Durable = true;
		});
	}
}
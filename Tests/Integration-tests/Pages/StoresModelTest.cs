using Application.Models.Certificates;
using Application.Pages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace IntegrationTests.Pages
{
	public class StoresModelTest
	{
		#region Methods

		[Fact]
		public async Task Stores_Test()
		{
			using(var serviceProvider = Global.CreateServices().BuildServiceProvider())
			{
				var storesModel = new StoresModel(serviceProvider.GetRequiredService<ILoggerFactory>(), serviceProvider.GetRequiredService<ICertificateStoreLoader>());

				await storesModel.OnGet(SearchFormAction.Search, null, "*");

				Assert.True(storesModel.Stores.Any());
			}
		}

		#endregion
	}
}
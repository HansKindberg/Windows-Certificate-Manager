using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using Application.Models.Certificates;
using Application.Models.Certificates.Configuration;
using Application.Models.Extensions;
using Application.Models.Security.Principal.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Win32;

namespace IntegrationTests.Models.Certificates
{
	public class CertificateStoreRepositoryTest
	{
		#region Methods

		private static CertificateStoreRepository CreateCertificateStoreRepository()
		{
			using(var serviceProvider = Global.CreateServices().BuildServiceProvider())
			{
				return new CertificateStoreRepository(serviceProvider.GetRequiredService<ICertificateStoreFactory>(), serviceProvider.GetRequiredService<IOptionsMonitor<WindowsCertificateManagerOptions>>());
			}
		}

		[Fact]
		public async Task Delete_IfTheLocationIsCurrentUser_And_IfTheStoreContainsCertificates_ShouldDeleteTheCertificatesAndTheStore()
		{
			await Delete_IfTheStoreContainsCertificates_ShouldDeleteTheCertificatesAndTheStore(StoreLocation.CurrentUser, "15f2c814-3e8b-4223-b7b0-f57c7854c72d", Registry.CurrentUser);
		}

		[Fact]
		public async Task Delete_IfTheLocationIsCurrentUser_ShouldDeleteTheStore()
		{
			await Delete_ShouldDeleteTheStore(StoreLocation.CurrentUser, "169617ad-98b9-4070-908f-fd0c220523c9", Registry.CurrentUser);
		}

		[Fact]
		public async Task Delete_IfTheLocationIsLocalMachine_And_IfTheStoreContainsCertificates_ShouldDeleteTheCertificatesAndTheStore()
		{
			if(!WindowsIdentity.GetCurrent().HasElevatedPrivileges())
				Assert.Fail("You need to run Visual Studio with elevated privileges, \"Run as Administrator\".");

			await Delete_IfTheStoreContainsCertificates_ShouldDeleteTheCertificatesAndTheStore(StoreLocation.LocalMachine, "706caf09-080d-4f30-bb68-2ddd891faa54", Registry.LocalMachine);
		}

		[Fact]
		public async Task Delete_IfTheLocationIsLocalMachine_ShouldDeleteTheStore()
		{
			if(!WindowsIdentity.GetCurrent().HasElevatedPrivileges())
				Assert.Fail("You need to run Visual Studio with elevated privileges, \"Run as Administrator\".");

			await Delete_ShouldDeleteTheStore(StoreLocation.LocalMachine, "76f8c91a-0a99-4456-b72c-8185facba51d", Registry.LocalMachine);
		}

		private static async Task Delete_IfTheStoreContainsCertificates_ShouldDeleteTheCertificatesAndTheStore(StoreLocation location, string name, RegistryKey registryKey)
		{
			await Task.CompletedTask;

			var certificatesDirectoryPath = Path.Combine(Global.ProjectDirectory.Parent!.Parent!.FullName, ".certificates");
			var certificateFilePath = Path.Combine(certificatesDirectoryPath, "root-certificate.crt");
			const string subKeyName = @"SOFTWARE\Microsoft\SystemCertificates";

			using(var key = registryKey.OpenSubKey(subKeyName, false))
			{
				Assert.DoesNotContain(name, key!.GetSubKeyNames(), StringComparer.OrdinalIgnoreCase);
			}

			using(var store = new X509Store(name, location))
			{
				store.Open(OpenFlags.ReadWrite);
				store.Add(new X509Certificate2(certificateFilePath));
				Assert.Single(store.Certificates);
			}

			using(var key = registryKey.OpenSubKey(subKeyName, false))
			{
				Assert.Contains(name, key!.GetSubKeyNames(), StringComparer.OrdinalIgnoreCase);
			}

			var certificateStoreRepository = CreateCertificateStoreRepository();

			certificateStoreRepository.Delete(location, name);

			using(var key = registryKey.OpenSubKey(subKeyName, false))
			{
				Assert.DoesNotContain(name, key!.GetSubKeyNames(), StringComparer.OrdinalIgnoreCase);
			}
		}

		private static async Task Delete_ShouldDeleteTheStore(StoreLocation location, string name, RegistryKey registryKey)
		{
			await Task.CompletedTask;

			const string subKeyName = @"SOFTWARE\Microsoft\SystemCertificates";

			using(var key = registryKey.OpenSubKey(subKeyName, false))
			{
				Assert.DoesNotContain(name, key!.GetSubKeyNames(), StringComparer.OrdinalIgnoreCase);
			}

			using(var store = new X509Store(name, location))
			{
				store.Open(OpenFlags.ReadWrite);
			}

			using(var key = registryKey.OpenSubKey(subKeyName, false))
			{
				Assert.Contains(name, key!.GetSubKeyNames(), StringComparer.OrdinalIgnoreCase);
			}

			var certificateStoreRepository = CreateCertificateStoreRepository();

			certificateStoreRepository.Delete(location, name);

			using(var key = registryKey.OpenSubKey(subKeyName, false))
			{
				Assert.DoesNotContain(name, key!.GetSubKeyNames(), StringComparer.OrdinalIgnoreCase);
			}
		}

		[Fact]
		public async Task Find_Test()
		{
			await Task.CompletedTask;

			const string subKeyName = @"SOFTWARE\Microsoft\SystemCertificates";

			var currentUserStores = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
			using(var key = Registry.CurrentUser.OpenSubKey(subKeyName, false))
			{
				foreach(var name in key!.GetSubKeyNames())
				{
					currentUserStores.Add(name);
				}
			}

			var localMachineStores = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
			using(var key = Registry.LocalMachine.OpenSubKey(subKeyName, false))
			{
				foreach(var name in key!.GetSubKeyNames())
				{
					localMachineStores.Add(name);
				}
			}

			var certificateStoreRepository = CreateCertificateStoreRepository();

			const string pattern = "*a*";

			// CurrentUser
			var stores = certificateStoreRepository.Find(StoreLocation.CurrentUser, pattern).ToArray();
			var registryStores = currentUserStores.Where(store => store.Like(pattern)).ToArray();
			foreach(var store in stores)
			{
				Assert.True(store.Name.Contains('a', StringComparison.OrdinalIgnoreCase));
				Assert.True(store.RegistryName!.Contains('a', StringComparison.OrdinalIgnoreCase));
			}

			foreach(var registryStore in registryStores)
			{
				Assert.True(registryStore.Contains('a', StringComparison.OrdinalIgnoreCase));
			}

			Assert.Equal(registryStores.Length, stores.Length);

			// LocalMachine
			stores = certificateStoreRepository.Find(StoreLocation.LocalMachine, pattern).ToArray();
			registryStores = localMachineStores.Where(store => store.Like(pattern)).ToArray();
			foreach(var store in stores)
			{
				Assert.True(store.Name.Contains('a', StringComparison.OrdinalIgnoreCase));
				Assert.True(store.RegistryName!.Contains('a', StringComparison.OrdinalIgnoreCase));
			}

			foreach(var registryStore in registryStores)
			{
				Assert.True(registryStore.Contains('a', StringComparison.OrdinalIgnoreCase));
			}

			// Sometimes the following test fails. E.g. Expected 12, Actual 11. Don't know why.
			Assert.Equal(registryStores.Length, stores.Length);
		}

		[Fact]
		public async Task Get_IfTheLocationIsCurrentUser_And_IfTheNameDoesNotExist_ShouldReturnNull()
		{
			await Get_IfTheNameDoesNotExist_ShouldReturnNull(StoreLocation.CurrentUser, "3107316b-8f3c-47ab-9b1d-808a8bebe10c", Registry.CurrentUser);
		}

		[Fact]
		public async Task Get_IfTheLocationIsCurrentUser_RootUpperCase_Test()
		{
			await Get_RootUpperCase_Test(StoreLocation.CurrentUser, Registry.CurrentUser);
		}

		[Fact]
		public async Task Get_IfTheLocationIsLocalMachine_And_IfTheNameDoesNotExist_ShouldReturnNull()
		{
			await Get_IfTheNameDoesNotExist_ShouldReturnNull(StoreLocation.LocalMachine, "298c3e20-7b0c-40a4-86d9-914c29d762d9", Registry.LocalMachine);
		}

		[Fact]
		public async Task Get_IfTheLocationIsLocalMachine_RootUpperCase_Test()
		{
			await Get_RootUpperCase_Test(StoreLocation.LocalMachine, Registry.LocalMachine);
		}

		private static async Task Get_IfTheNameDoesNotExist_ShouldReturnNull(StoreLocation location, string name, RegistryKey registryKey)
		{
			await Task.CompletedTask;

			const string subKeyName = @"SOFTWARE\Microsoft\SystemCertificates";

			using(var key = registryKey.OpenSubKey(subKeyName, false))
			{
				Assert.DoesNotContain(name, key!.GetSubKeyNames(), StringComparer.OrdinalIgnoreCase);
			}

			Assert.Null(CreateCertificateStoreRepository().Get(location, name));
		}

		private static async Task Get_RootUpperCase_Test(StoreLocation location, RegistryKey registryKey)
		{
			await Task.CompletedTask;

			const string name = "ROOT";

			using(var key = registryKey.OpenSubKey(@"SOFTWARE\Microsoft\SystemCertificates", false))
			{
				var registryName = key!.GetSubKeyNames().FirstOrDefault(subKey => string.Equals(subKey, name, StringComparison.OrdinalIgnoreCase));

				var certificateStoreRepository = CreateCertificateStoreRepository();
				var store = (CertificateStore)certificateStoreRepository.Get(location, name)!;

				Assert.NotNull(store);
				Assert.Equal(location, store.Location);
				Assert.Equal("Root", store.Name);
				Assert.Equal(registryName, store.RegistryName);
				Assert.True(store.Standard);
				Assert.Equal(StoreName.Root, store.StoreName);
			}
		}

		[Fact]
		public async Task List_Test()
		{
			await Task.CompletedTask;

			const string subKeyName = @"SOFTWARE\Microsoft\SystemCertificates";

			var currentUserStores = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
			using(var key = Registry.CurrentUser.OpenSubKey(subKeyName, false))
			{
				foreach(var name in key!.GetSubKeyNames())
				{
					currentUserStores.Add(name);
				}
			}

			var localMachineStores = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
			using(var key = Registry.LocalMachine.OpenSubKey(subKeyName, false))
			{
				foreach(var name in key!.GetSubKeyNames())
				{
					localMachineStores.Add(name);
				}
			}

			var certificateStoreRepository = CreateCertificateStoreRepository();
			var stores = certificateStoreRepository.List().Cast<CertificateStore>().ToArray();

			Assert.Equal(currentUserStores.Count + localMachineStores.Count, stores.Length);

			Assert.Empty(stores.Where(store => store.Standard && store.StoreName == null));
			Assert.Empty(stores.Where(store => !store.Standard && store.StoreName != null));
			Assert.Empty(stores.Where(store => store.RegistryName == null));
			Assert.Empty(stores.Where(store => !string.Equals(store.RegistryName, store.Name, StringComparison.OrdinalIgnoreCase)));
		}

		[Fact]
		public async Task Save_IfTheLocationIsCurrentUser_ShouldAddTheStore()
		{
			await Save_ShouldAddTheStore(StoreLocation.CurrentUser, "082be34c-a04b-4b45-ae9d-98cf4b21b891", Registry.CurrentUser);
		}

		[Fact]
		public async Task Save_IfTheLocationIsLocalMachine_ShouldAddTheStore()
		{
			if(!WindowsIdentity.GetCurrent().HasElevatedPrivileges())
				Assert.Fail("You need to run Visual Studio with elevated privileges, \"Run as Administrator\".");

			await Save_ShouldAddTheStore(StoreLocation.LocalMachine, "07cce21a-1897-4d78-bf2b-4778052c7efb", Registry.LocalMachine);
		}

		private static async Task Save_ShouldAddTheStore(StoreLocation location, string name, RegistryKey registryKey)
		{
			await Task.CompletedTask;

			const string subKeyName = @"SOFTWARE\Microsoft\SystemCertificates";

			using(var key = registryKey.OpenSubKey(subKeyName, false))
			{
				Assert.DoesNotContain(name, key!.GetSubKeyNames(), StringComparer.OrdinalIgnoreCase);
			}

			using(var store = new X509Store(name, location))
			{
				Assert.Throws<CryptographicException>(() => store.Open(OpenFlags.OpenExistingOnly));
			}

			var certificateStoreRepository = CreateCertificateStoreRepository();

			certificateStoreRepository.Save(location, name);

			using(var key = registryKey.OpenSubKey(subKeyName, false))
			{
				Assert.Contains(name, key!.GetSubKeyNames(), StringComparer.OrdinalIgnoreCase);
			}

			using(var store = new X509Store(name, location))
			{
				store.Open(OpenFlags.OpenExistingOnly);
			}

			certificateStoreRepository.Delete(location, name);

			using(var key = registryKey.OpenSubKey(subKeyName, false))
			{
				Assert.DoesNotContain(name, key!.GetSubKeyNames(), StringComparer.OrdinalIgnoreCase);
			}
		}

		#endregion
	}
}
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using Application.Models.Certificates;
using Application.Models.Certificates.Configuration;
using Application.Models.Certificates.Internal;
using Application.Models.Security.Principal.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Win32;

namespace IntegrationTests.Models.Certificates
{
	public class CertificateRepositoryTest
	{
		#region Methods

		[Fact]
		public async Task Delete_IfTheLocationIsCurrentUser_ShouldRemoveTheCertificate()
		{
			await SaveAndDelete_ShouldAddAndThenRemoveTheCertificate(Registry.CurrentUser, StoreLocation.CurrentUser, "11da4234-5b44-4853-9571-458a9497828e");
		}

		[Fact]
		public async Task Delete_IfTheLocationIsLocalMachine_ShouldRemoveTheCertificate()
		{
			if(!WindowsIdentity.GetCurrent().HasElevatedPrivileges())
				Assert.Fail("You need to run Visual Studio with elevated privileges, \"Run as Administrator\".");

			await SaveAndDelete_ShouldAddAndThenRemoveTheCertificate(Registry.LocalMachine, StoreLocation.LocalMachine, "eb444853-2992-43fe-8687-1cb0df4c7ae6");
		}

		[Fact]
		public async Task Find_IfNoParameters_ShouldReturnEmptyResult()
		{
			await Task.CompletedTask;

			using(var serviceProvider = Global.CreateServices().BuildServiceProvider())
			{
				var certificateRepository = new CertificateRepository(serviceProvider.GetRequiredService<ICertificateFactory>(), serviceProvider.GetRequiredService<IOptionsMonitor<WindowsCertificateManagerOptions>>(), serviceProvider.GetRequiredService<ICertificateStoreLoader>());

				Assert.Empty(certificateRepository.Find());
			}
		}

		[Fact]
		public async Task Find_IfThumbprintIsWildcard_ShouldReturnSome()
		{
			await Task.CompletedTask;

			using(var serviceProvider = Global.CreateServices().BuildServiceProvider())
			{
				var certificateRepository = new CertificateRepository(serviceProvider.GetRequiredService<ICertificateFactory>(), serviceProvider.GetRequiredService<IOptionsMonitor<WindowsCertificateManagerOptions>>(), serviceProvider.GetRequiredService<ICertificateStoreLoader>());

				Assert.True(certificateRepository.Find(thumbprint: "*").Any());
			}
		}

		[Fact]
		public async Task Save_IfTheLocationIsCurrentUser_ShouldAddTheCertificate()
		{
			await SaveAndDelete_ShouldAddAndThenRemoveTheCertificate(Registry.CurrentUser, StoreLocation.CurrentUser, "c8862e68-c059-4134-a154-1c11a14ec474");
		}

		[Fact]
		public async Task Save_IfTheLocationIsLocalMachine_ShouldAddTheCertificate()
		{
			if(!WindowsIdentity.GetCurrent().HasElevatedPrivileges())
				Assert.Fail("You need to run Visual Studio with elevated privileges, \"Run as Administrator\".");

			await SaveAndDelete_ShouldAddAndThenRemoveTheCertificate(Registry.LocalMachine, StoreLocation.LocalMachine, "6acadc16-b9fe-44d8-b8cf-e102c885ce03");
		}

		private static async Task SaveAndDelete_ShouldAddAndThenRemoveTheCertificate(RegistryKey registryKey, StoreLocation storeLocation, string storeName)
		{
			var certificatesDirectoryPath = Path.Combine(Global.ProjectDirectory.Parent!.Parent!.FullName, ".certificates");
			var crtFilePath = Path.Combine(certificatesDirectoryPath, "intermediate-certificate-1.crt");
			var pfxFilePath = Path.Combine(certificatesDirectoryPath, "client-certificate-1.pfx");

			Assert.True(File.Exists(crtFilePath));
			Assert.True(File.Exists(pfxFilePath));

			const string registryPath = @"SOFTWARE\Microsoft\SystemCertificates";

			using(var key = registryKey.OpenSubKey(registryPath, false))
			{
				Assert.DoesNotContain(storeName, key!.GetSubKeyNames(), StringComparer.OrdinalIgnoreCase);
			}

			using(var serviceProvider = Global.CreateServices().BuildServiceProvider())
			{
				var certificateRepository = new CertificateRepository(serviceProvider.GetRequiredService<ICertificateFactory>(), serviceProvider.GetRequiredService<IOptionsMonitor<WindowsCertificateManagerOptions>>(), serviceProvider.GetRequiredService<ICertificateStoreLoader>());

				var crtFileBytes = await File.ReadAllBytesAsync(crtFilePath);
				var crtCertificate = certificateRepository.Save(crtFileBytes, storeLocation, storeName);

				var pfxFileBytes = await File.ReadAllBytesAsync(pfxFilePath);
				var pfxCertificate = certificateRepository.Save(pfxFileBytes, storeLocation, storeName, "password");

				using(var store = new X509Store(storeName, storeLocation))
				{
					store.Open(OpenFlags.OpenExistingOnly);

					Assert.Equal(2, store.Certificates.Count);
					Assert.NotNull(store.Certificates.FirstOrDefault(certificate => string.Equals(crtCertificate.Thumbprint, certificate.Thumbprint, StringComparison.OrdinalIgnoreCase)));
					Assert.NotNull(store.Certificates.FirstOrDefault(certificate => string.Equals(pfxCertificate.Thumbprint, certificate.Thumbprint, StringComparison.OrdinalIgnoreCase)));
				}

				certificateRepository.Delete(storeLocation, storeName, crtCertificate.Thumbprint!);
				certificateRepository.Delete(storeLocation, storeName, pfxCertificate.Thumbprint!);

				using(var store = new X509Store(storeName, storeLocation))
				{
					store.Open(OpenFlags.OpenExistingOnly);

					Assert.Empty(store.Certificates);
				}
			}

			var flags = WindowsCryptographic.CertificateStoreDelete;

			if(storeLocation == StoreLocation.CurrentUser)
				flags |= WindowsCryptographic.CertificateStoreCurrentUser;
			else if(storeLocation == StoreLocation.LocalMachine)
				flags |= WindowsCryptographic.CertificateStoreLocalMachine;

			WindowsCryptographic.CertUnregisterSystemStore(storeName, flags);

			using(var key = registryKey.OpenSubKey(registryPath, false))
			{
				Assert.DoesNotContain(storeName, key!.GetSubKeyNames(), StringComparer.OrdinalIgnoreCase);
			}
		}

		#endregion
	}
}
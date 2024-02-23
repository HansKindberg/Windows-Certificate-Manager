using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using Application.Models.Certificates.Internal;
using Application.Models.Security.Principal.Extensions;
using Microsoft.Win32;

namespace IntegrationTests.Models.Certificates.Internal
{
	public class WindowsCryptographicTest
	{
		#region Fields

		private const string _registrySubKeyName = @"SOFTWARE\Microsoft\SystemCertificates";

		#endregion

		#region Methods

		[Fact]
		public async Task CertUnregisterSystemStore_CurrentUser_Test()
		{
			await Task.CompletedTask;

			const StoreLocation storeLocation = StoreLocation.CurrentUser;
			const string storeName = "00739558-dff8-4c92-94b4-95b85181db70";
			var registryKey = Registry.CurrentUser;

			using(var key = registryKey.OpenSubKey(_registrySubKeyName, false))
			{
				Assert.DoesNotContain(storeName, key!.GetSubKeyNames(), StringComparer.OrdinalIgnoreCase);
			}

			using(var store = new X509Store(storeName, storeLocation))
			{
				Assert.Throws<CryptographicException>(() => store.Open(OpenFlags.OpenExistingOnly));
			}

			// Create the store.
			using(var store = new X509Store(storeName, storeLocation))
			{
				store.Open(OpenFlags.ReadWrite);

				Assert.Empty(store.Certificates);
			}

			using(var key = registryKey.OpenSubKey(_registrySubKeyName, false))
			{
				Assert.Contains(storeName, key!.GetSubKeyNames(), StringComparer.OrdinalIgnoreCase);
			}

			using(var store = new X509Store(storeName, storeLocation))
			{
				store.Open(OpenFlags.OpenExistingOnly);

				Assert.Empty(store.Certificates);
			}

			// Delete the store.
			WindowsCryptographic.CertUnregisterSystemStore(storeName, WindowsCryptographic.CertificateStoreDelete | WindowsCryptographic.CertificateStoreCurrentUser);

			using(var key = registryKey.OpenSubKey(_registrySubKeyName, false))
			{
				Assert.DoesNotContain(storeName, key!.GetSubKeyNames(), StringComparer.OrdinalIgnoreCase);
			}

			using(var store = new X509Store(storeName, storeLocation))
			{
				Assert.Throws<CryptographicException>(() => store.Open(OpenFlags.OpenExistingOnly));
			}
		}

		[Fact]
		public async Task CertUnregisterSystemStore_LocalMachine_Test()
		{
			await Task.CompletedTask;

			if(!WindowsIdentity.GetCurrent().HasElevatedPrivileges())
				Assert.Fail("You need to run Visual Studio with elevated privileges, \"Run as Administrator\".");

			const StoreLocation storeLocation = StoreLocation.LocalMachine;
			const string storeName = "2b7ae05e-fa1c-4fe8-b026-fce1a1ea5f1c";
			var registryKey = Registry.LocalMachine;

			using(var key = registryKey.OpenSubKey(_registrySubKeyName, false))
			{
				Assert.DoesNotContain(storeName, key!.GetSubKeyNames(), StringComparer.OrdinalIgnoreCase);
			}

			using(var store = new X509Store(storeName, storeLocation))
			{
				Assert.Throws<CryptographicException>(() => store.Open(OpenFlags.OpenExistingOnly));
			}

			// Create the store.
			using(var store = new X509Store(storeName, storeLocation))
			{
				store.Open(OpenFlags.ReadWrite);

				Assert.Empty(store.Certificates);
			}

			using(var key = registryKey.OpenSubKey(_registrySubKeyName, false))
			{
				Assert.Contains(storeName, key!.GetSubKeyNames(), StringComparer.OrdinalIgnoreCase);
			}

			using(var store = new X509Store(storeName, storeLocation))
			{
				store.Open(OpenFlags.OpenExistingOnly);

				Assert.Empty(store.Certificates);
			}

			// Delete the store.
			WindowsCryptographic.CertUnregisterSystemStore(storeName, WindowsCryptographic.CertificateStoreDelete | WindowsCryptographic.CertificateStoreLocalMachine);

			using(var key = registryKey.OpenSubKey(_registrySubKeyName, false))
			{
				Assert.DoesNotContain(storeName, key!.GetSubKeyNames(), StringComparer.OrdinalIgnoreCase);
			}

			using(var store = new X509Store(storeName, storeLocation))
			{
				Assert.Throws<CryptographicException>(() => store.Open(OpenFlags.OpenExistingOnly));
			}
		}

		#endregion
	}
}
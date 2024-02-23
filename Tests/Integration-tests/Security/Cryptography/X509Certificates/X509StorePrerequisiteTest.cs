using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using Application.Models.Certificates.Internal;
using Application.Models.Security.Principal.Extensions;
using Microsoft.Win32;

namespace IntegrationTests.Security.Cryptography.X509Certificates
{
	public class X509StorePrerequisiteTest
	{
		#region Fields

		private const string _registrySubKeyName = @"SOFTWARE\Microsoft\SystemCertificates";

		#endregion

		#region Methods

		[Fact]
		public async Task Open_IfTheStoreLocationIsCurrentUser_And_IfTheStoreNameDoesNotExists_And_IfTheFlagIsOpenExistingOnly_ShouldThrowACryptographicException()
		{
			await Task.CompletedTask;

			const StoreLocation location = StoreLocation.CurrentUser;
			const string name = "a18a7af8-365f-429a-ae16-6d69503929d3";
			var registryKey = Registry.CurrentUser;

			using(var key = registryKey.OpenSubKey(_registrySubKeyName, false))
			{
				Assert.DoesNotContain(name, key!.GetSubKeyNames(), StringComparer.OrdinalIgnoreCase);
			}

			using(var store = new X509Store(name, location))
			{
				Assert.Throws<CryptographicException>(() => store.Open(OpenFlags.OpenExistingOnly));
			}

			using(var key = registryKey.OpenSubKey(_registrySubKeyName, false))
			{
				Assert.DoesNotContain(name, key!.GetSubKeyNames(), StringComparer.OrdinalIgnoreCase);
			}
		}

		[Fact]
		public async Task Open_IfTheStoreLocationIsCurrentUser_And_IfTheStoreNameDoesNotExists_And_IfTheFlagIsOpenExistingOnlyAndReadOnly_ShouldThrowACryptographicException()
		{
			await Task.CompletedTask;

			const StoreLocation location = StoreLocation.CurrentUser;
			const string name = "bdb3f0aa-d175-43f4-ba3c-be163f5e37d3";
			var registryKey = Registry.CurrentUser;

			using(var key = registryKey.OpenSubKey(_registrySubKeyName, false))
			{
				Assert.DoesNotContain(name, key!.GetSubKeyNames(), StringComparer.OrdinalIgnoreCase);
			}

			using(var store = new X509Store(name, location))
			{
				Assert.Throws<CryptographicException>(() => store.Open(OpenFlags.OpenExistingOnly | OpenFlags.ReadOnly));
			}

			using(var key = registryKey.OpenSubKey(_registrySubKeyName, false))
			{
				Assert.DoesNotContain(name, key!.GetSubKeyNames(), StringComparer.OrdinalIgnoreCase);
			}
		}

		[Fact]
		public async Task Open_IfTheStoreLocationIsCurrentUser_And_IfTheStoreNameDoesNotExists_And_IfTheFlagIsReadOnly_ShouldNotCreateTheStore()
		{
			await Task.CompletedTask;

			const StoreLocation location = StoreLocation.CurrentUser;
			const string name = "688409e8-e172-4d4a-801a-7aa0bb5c2748";
			var registryKey = Registry.CurrentUser;

			using(var key = registryKey.OpenSubKey(_registrySubKeyName, false))
			{
				Assert.DoesNotContain(name, key!.GetSubKeyNames(), StringComparer.OrdinalIgnoreCase);
			}

			using(var store = new X509Store(name, location))
			{
				store.Open(OpenFlags.ReadOnly);

				Assert.Empty(store.Certificates);
			}

			using(var key = registryKey.OpenSubKey(_registrySubKeyName, false))
			{
				Assert.DoesNotContain(name, key!.GetSubKeyNames(), StringComparer.OrdinalIgnoreCase);
			}
		}

		[Fact]
		public async Task Open_IfTheStoreLocationIsCurrentUser_And_IfTheStoreNameDoesNotExists_And_IfTheFlagIsReadWrite_ShouldCreateTheStore()
		{
			await Task.CompletedTask;

			const StoreLocation location = StoreLocation.CurrentUser;
			const string name = "8c579ab5-f210-477a-b31c-9502087eec35";
			var registryKey = Registry.CurrentUser;

			using(var key = registryKey.OpenSubKey(_registrySubKeyName, false))
			{
				Assert.DoesNotContain(name, key!.GetSubKeyNames(), StringComparer.OrdinalIgnoreCase);
			}

			using(var store = new X509Store(name, location))
			{
				Assert.Throws<CryptographicException>(() => store.Open(OpenFlags.OpenExistingOnly));
			}

			using(var store = new X509Store(name, location))
			{
				store.Open(OpenFlags.ReadWrite);
			}

			using(var key = registryKey.OpenSubKey(_registrySubKeyName, false))
			{
				Assert.Contains(name, key!.GetSubKeyNames(), StringComparer.OrdinalIgnoreCase);
			}

			using(var store = new X509Store(name, location))
			{
				store.Open(OpenFlags.OpenExistingOnly);
			}

			WindowsCryptographic.CertUnregisterSystemStore(name, WindowsCryptographic.CertificateStoreDelete | WindowsCryptographic.CertificateStoreCurrentUser);

			using(var key = registryKey.OpenSubKey(_registrySubKeyName, false))
			{
				Assert.DoesNotContain(name, key!.GetSubKeyNames(), StringComparer.OrdinalIgnoreCase);
			}

			using(var store = new X509Store(name, location))
			{
				Assert.Throws<CryptographicException>(() => store.Open(OpenFlags.OpenExistingOnly));
			}
		}

		[Fact]
		public async Task Open_IfTheStoreLocationIsLocalMachine_And_IfTheStoreNameDoesNotExists_And_IfTheFlagIsOpenExistingOnly_ShouldThrowACryptographicException()
		{
			await Task.CompletedTask;

			const StoreLocation location = StoreLocation.LocalMachine;
			const string name = "3a327fda-ba5f-4b5b-9d4f-ab6d94d9111c";
			var registryKey = Registry.LocalMachine;

			using(var key = registryKey.OpenSubKey(_registrySubKeyName, false))
			{
				Assert.DoesNotContain(name, key!.GetSubKeyNames(), StringComparer.OrdinalIgnoreCase);
			}

			using(var store = new X509Store(name, location))
			{
				Assert.Throws<CryptographicException>(() => store.Open(OpenFlags.OpenExistingOnly));
			}

			using(var key = registryKey.OpenSubKey(_registrySubKeyName, false))
			{
				Assert.DoesNotContain(name, key!.GetSubKeyNames(), StringComparer.OrdinalIgnoreCase);
			}
		}

		[Fact]
		public async Task Open_IfTheStoreLocationIsLocalMachine_And_IfTheStoreNameDoesNotExists_And_IfTheFlagIsOpenExistingOnlyAndReadOnly_ShouldThrowACryptographicException()
		{
			await Task.CompletedTask;

			const StoreLocation location = StoreLocation.LocalMachine;
			const string name = "5eb3af23-b594-451e-9332-c6e0001bbd03";
			var registryKey = Registry.LocalMachine;

			using(var key = registryKey.OpenSubKey(_registrySubKeyName, false))
			{
				Assert.DoesNotContain(name, key!.GetSubKeyNames(), StringComparer.OrdinalIgnoreCase);
			}

			using(var store = new X509Store(name, location))
			{
				Assert.Throws<CryptographicException>(() => store.Open(OpenFlags.OpenExistingOnly | OpenFlags.ReadOnly));
			}

			using(var key = registryKey.OpenSubKey(_registrySubKeyName, false))
			{
				Assert.DoesNotContain(name, key!.GetSubKeyNames(), StringComparer.OrdinalIgnoreCase);
			}
		}

		[Fact]
		public async Task Open_IfTheStoreLocationIsLocalMachine_And_IfTheStoreNameDoesNotExists_And_IfTheFlagIsReadOnly_ShouldNotCreateTheStore()
		{
			await Task.CompletedTask;

			const StoreLocation location = StoreLocation.LocalMachine;
			const string name = "216045ec-5604-4e19-b846-9c36398626fd";
			var registryKey = Registry.LocalMachine;

			using(var key = registryKey.OpenSubKey(_registrySubKeyName, false))
			{
				Assert.DoesNotContain(name, key!.GetSubKeyNames(), StringComparer.OrdinalIgnoreCase);
			}

			using(var store = new X509Store(name, location))
			{
				store.Open(OpenFlags.ReadOnly);

				Assert.Empty(store.Certificates);
			}

			using(var key = registryKey.OpenSubKey(_registrySubKeyName, false))
			{
				Assert.DoesNotContain(name, key!.GetSubKeyNames(), StringComparer.OrdinalIgnoreCase);
			}
		}

		[Fact]
		public async Task Open_IfTheStoreLocationIsLocalMachine_And_IfTheStoreNameDoesNotExists_And_IfTheFlagIsReadWrite_ShouldCreateTheStore()
		{
			await Task.CompletedTask;

			if(!WindowsIdentity.GetCurrent().HasElevatedPrivileges())
				Assert.Fail("You need to run Visual Studio with elevated privileges, \"Run as Administrator\".");

			const StoreLocation location = StoreLocation.LocalMachine;
			const string name = "2a4b309a-ffc4-4fe1-80e6-3692dc939eb0";
			var registryKey = Registry.LocalMachine;

			using(var key = registryKey.OpenSubKey(_registrySubKeyName, false))
			{
				Assert.DoesNotContain(name, key!.GetSubKeyNames(), StringComparer.OrdinalIgnoreCase);
			}

			using(var store = new X509Store(name, location))
			{
				Assert.Throws<CryptographicException>(() => store.Open(OpenFlags.OpenExistingOnly));
			}

			using(var store = new X509Store(name, location))
			{
				store.Open(OpenFlags.ReadWrite);
			}

			using(var key = registryKey.OpenSubKey(_registrySubKeyName, false))
			{
				Assert.Contains(name, key!.GetSubKeyNames(), StringComparer.OrdinalIgnoreCase);
			}

			using(var store = new X509Store(name, location))
			{
				store.Open(OpenFlags.OpenExistingOnly);
			}

			WindowsCryptographic.CertUnregisterSystemStore(name, WindowsCryptographic.CertificateStoreDelete | WindowsCryptographic.CertificateStoreLocalMachine);

			using(var key = registryKey.OpenSubKey(_registrySubKeyName, false))
			{
				Assert.DoesNotContain(name, key!.GetSubKeyNames(), StringComparer.OrdinalIgnoreCase);
			}

			using(var store = new X509Store(name, location))
			{
				Assert.Throws<CryptographicException>(() => store.Open(OpenFlags.OpenExistingOnly));
			}
		}

		#endregion
	}
}
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using Application.Models.Certificates.Configuration;
using Application.Models.Certificates.Internal;
using Application.Models.Extensions;
using Application.Models.Security.Principal.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.Win32;

namespace Application.Models.Certificates
{
	public class CertificateStoreRepository(ICertificateStoreFactory factory, IOptionsMonitor<WindowsCertificateManagerOptions> optionsMonitor) : ICertificateStoreRepository
	{
		#region Fields

		private ISet<ICertificateStore>? _standardStores;

		#endregion

		#region Properties

		protected internal virtual ICertificateStoreFactory Factory { get; } = factory ?? throw new ArgumentNullException(nameof(factory));
		protected internal virtual IOptionsMonitor<WindowsCertificateManagerOptions> OptionsMonitor { get; } = optionsMonitor ?? throw new ArgumentNullException(nameof(optionsMonitor));
		protected internal virtual ISet<ICertificateStore> StandardStores => this._standardStores ??= this.GetStandardStores();

		#endregion

		#region Methods

		public virtual void Delete(StoreLocation location, string name)
		{
			try
			{
				var storeNameRestrictionPattern = this.OptionsMonitor.CurrentValue.StoreNameRestrictionPattern;

				if(!name.Like(storeNameRestrictionPattern))
					throw new InvalidOperationException($"You are not allowed to delete this store, \"{location}/{name}\". You are only allowed to delete stores with a name matching the pattern \"{storeNameRestrictionPattern}\".");

				if(location == StoreLocation.LocalMachine && !WindowsIdentity.GetCurrent().HasElevatedPrivileges())
					throw new SecurityException("You need to run this application with elevated privileges, \"Run as Administrator\".");

				using(var store = new X509Store(name, location))
				{
					store.Open(OpenFlags.ReadWrite);

					if(store.Certificates.Any())
					{
						store.RemoveRange(store.Certificates);
					}
				}

				var flags = WindowsCryptographic.CertificateStoreDelete;

				if(location == StoreLocation.CurrentUser)
					flags |= WindowsCryptographic.CertificateStoreCurrentUser;
				else if(location == StoreLocation.LocalMachine)
					flags |= WindowsCryptographic.CertificateStoreLocalMachine;
				else
					throw new InvalidOperationException("Not supported.");

				WindowsCryptographic.CertUnregisterSystemStore(name, flags);
			}
			catch(Exception exception)
			{
				throw new InvalidOperationException($"Could not delete certificate-store {location}/{name}.", exception);
			}
		}

		public virtual IEnumerable<ICertificateStore> Find(StoreLocation? location = null, string? name = null)
		{
			var list = new List<ICertificateStore>();

			if(location != null || name != null)
			{
				var locations = new List<StoreLocation>();

				if(location == null)
					locations.AddRange([StoreLocation.CurrentUser, StoreLocation.LocalMachine]);
				else
					locations.Add(location.Value);

				foreach(var storeLocation in locations)
				{
					list.AddRange(this.GetNames(storeLocation).Where(item => name == null || item.Key.Like(name)).Select(item => this.Factory.Create(storeLocation, item.Value, item.Key)));
				}
			}

			return list.ToArray();
		}

		public virtual ICertificateStore? Get(StoreLocation location, string name)
		{
			var names = this.GetNames(location).Where(item => string.Equals(item.Key, name, StringComparison.OrdinalIgnoreCase)).ToArray();

			if(names.Any())
			{
				var first = names.First();

				return this.Factory.Create(location, first.Value, first.Key);
			}

			return null;
		}

		protected internal virtual IDictionary<string, StoreName?> GetNames(StoreLocation location)
		{
			return this.GetNames(this.GetRegistryKey(location), location);
		}

		protected internal virtual IDictionary<string, StoreName?> GetNames(RegistryKey registryKey, StoreLocation location)
		{
			var names = new SortedDictionary<string, StoreName?>(StringComparer.Ordinal);

			using(var key = registryKey.OpenSubKey(@"SOFTWARE\Microsoft\SystemCertificates", false))
			{
				foreach(var subKey in key!.GetSubKeyNames().OrderBy(name => name))
				{
					var name = this.StandardStores.FirstOrDefault(item => item.Location == location && item.StoreName != null && item.Name.Equals(subKey, StringComparison.OrdinalIgnoreCase))?.StoreName;

					names.Add(subKey, name);
				}
			}

			return names;
		}

		protected internal virtual RegistryKey GetRegistryKey(StoreLocation location)
		{
			if(location == StoreLocation.CurrentUser)
				return Registry.CurrentUser;

			if(location == StoreLocation.LocalMachine)
				return Registry.LocalMachine;

			throw new InvalidOperationException($"Could not get registry-key for store-location {location}.");
		}

		protected internal virtual ISet<ICertificateStore> GetStandardStores()
		{
			var stores = new HashSet<ICertificateStore>();

			foreach(var location in new[] { StoreLocation.CurrentUser, StoreLocation.LocalMachine })
			{
				foreach(var name in Enum.GetValues<StoreName>())
				{
					using(var store = new X509Store(name, location))
					{
						try
						{
							store.Open(OpenFlags.OpenExistingOnly);
							stores.Add(this.Factory.Create(location, name, null));
						}
						// ReSharper disable EmptyGeneralCatchClause
						catch { }
						// ReSharper restore EmptyGeneralCatchClause
					}
				}
			}

			return stores;
		}

		public virtual IEnumerable<ICertificateStore> List()
		{
			return this.Find(null, "*");
		}

		public virtual ICertificateStore Save(StoreLocation location, string name)
		{
			ArgumentNullException.ThrowIfNull(name);

			try
			{
				var storeNameRestrictionPattern = this.OptionsMonitor.CurrentValue.StoreNameRestrictionPattern;

				if(!name.Like(storeNameRestrictionPattern))
					throw new InvalidOperationException($"You are not allowed to save this store \"{location}/{name}\". You are only allowed to save stores with a name matching the pattern \"{storeNameRestrictionPattern}\".");

				var existingStore = this.Get(location, name);

				if(existingStore != null)
					throw new InvalidOperationException($"The certificate-store {location}/{name} already exists.");

				if(location == StoreLocation.LocalMachine && !WindowsIdentity.GetCurrent().HasElevatedPrivileges())
					throw new SecurityException("You need to run this application with elevated privileges, \"Run as Administrator\".");

				using(var store = new X509Store(name, location))
				{
					store.Open(OpenFlags.ReadWrite);
				}

				return this.Get(location, name)!;
			}
			catch(Exception exception)
			{
				throw new InvalidOperationException($"Could not save certificate-store {location}/{name}.", exception);
			}
		}

		#endregion
	}
}
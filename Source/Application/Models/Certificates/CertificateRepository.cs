using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using Application.Models.Certificates.Configuration;
using Application.Models.Extensions;
using Application.Models.Security.Principal.Extensions;
using Microsoft.Extensions.Options;

namespace Application.Models.Certificates
{
	public class CertificateRepository(ICertificateFactory factory, IOptionsMonitor<WindowsCertificateManagerOptions> optionsMonitor, ICertificateStoreLoader storeLoader) : ICertificateRepository
	{
		#region Properties

		protected internal virtual ICertificateFactory Factory { get; } = factory ?? throw new ArgumentNullException(nameof(factory));
		protected internal virtual IOptionsMonitor<WindowsCertificateManagerOptions> OptionsMonitor { get; } = optionsMonitor ?? throw new ArgumentNullException(nameof(optionsMonitor));
		protected internal virtual ICertificateStoreLoader StoreLoader { get; } = storeLoader ?? throw new ArgumentNullException(nameof(storeLoader));

		#endregion

		#region Methods

		public virtual void Delete(StoreLocation storeLocation, string storeName, string thumbprint)
		{
			try
			{
				var storeNameRestrictionPattern = this.OptionsMonitor.CurrentValue.StoreNameRestrictionPattern;

				if(!storeName.Like(storeNameRestrictionPattern))
					throw new InvalidOperationException($"You are not allowed to delete this certificate, \"{storeLocation}/{storeName} - {thumbprint}\". You are only allowed to delete certificates from stores with a name matching the pattern \"{storeNameRestrictionPattern}\".");

				if(storeLocation == StoreLocation.LocalMachine && !WindowsIdentity.GetCurrent().HasElevatedPrivileges())
					throw new SecurityException("You need to run this application with elevated privileges, \"Run as Administrator\".");

				using(var store = new X509Store(storeName, storeLocation))
				{
					store.Open(OpenFlags.ReadWrite);

					var certificate = store.Certificates.FirstOrDefault(certificate => certificate.Thumbprint == thumbprint);

					if(certificate == null)
						throw new InvalidOperationException($"Could not find certificate \"{thumbprint}\" in {storeLocation}/{storeName}.");

					store.Remove(certificate);
				}
			}
			catch(Exception exception)
			{
				throw new InvalidOperationException($"Could not delete certificate \"{thumbprint}\" from {storeLocation}/{storeName}.", exception);
			}
		}

		public virtual IEnumerable<ICertificate> Find(string? friendlyName = null, string? issuer = null, string? serialNumber = null, StoreLocation? storeLocation = null, string? storeName = null, string? subject = null, string? thumbprint = null)
		{
			if(friendlyName == null && issuer == null && serialNumber == null && storeLocation == null && storeName == null && subject == null && thumbprint == null)
				return [];

			var result = new List<ICertificate>();

			var locations = new List<StoreLocation>();

			if(storeLocation == null)
				locations.AddRange([StoreLocation.CurrentUser, StoreLocation.LocalMachine]);
			else
				locations.Add(storeLocation.Value);

			var stores = this.StoreLoader.List().ToArray();

			foreach(var store in stores)
			{
				if(!locations.Contains(store.Location))
					continue;

				if(storeName != null && !store.Name.Like(storeName))
					continue;

				using(var x509Store = new X509Store(store.Name, store.Location))
				{
					x509Store.Open(OpenFlags.OpenExistingOnly);

					foreach(var certificate in x509Store.Certificates)
					{
						if(!this.Include(certificate, friendlyName, issuer, serialNumber, subject, thumbprint))
							continue;

						result.Add(this.Factory.Create(certificate, store));
					}
				}
			}

			return result;
		}

		public virtual ICertificate? Get(StoreLocation storeLocation, string storeName, string thumbprint)
		{
			var store = this.StoreLoader.Get(storeLocation, storeName);

			if(store == null)
				return null;

			using(var x509Store = new X509Store(storeName, storeLocation))
			{
				x509Store.Open(OpenFlags.OpenExistingOnly);

				var certificate = x509Store.Certificates.FirstOrDefault(certificate => string.Equals(certificate.Thumbprint, thumbprint, StringComparison.OrdinalIgnoreCase));

				return certificate != null ? this.Factory.Create(certificate, store) : null;
			}
		}

		protected internal virtual bool Include(X509Certificate2? certificate, string? friendlyName = null, string? issuer = null, string? serialNumber = null, string? subject = null, string? thumbprint = null)
		{
			if(certificate == null)
				return false;

			if(friendlyName != null && !certificate.FriendlyName.Like(friendlyName))
				return false;

			if(issuer != null && !certificate.Issuer.Like(issuer))
				return false;

			if(serialNumber != null && !certificate.SerialNumber.Like(serialNumber))
				return false;

			if(subject != null && !certificate.Subject.Like(subject))
				return false;

			if(thumbprint != null && !certificate.Thumbprint.Like(thumbprint))
				return false;

			return true;
		}

		public virtual ICertificate Save(IEnumerable<byte> bytes, StoreLocation storeLocation, string storeName, string? password = null)
		{
			ArgumentNullException.ThrowIfNull(bytes);
			ArgumentNullException.ThrowIfNull(storeName);

			try
			{
				var storeNameRestrictionPattern = this.OptionsMonitor.CurrentValue.StoreNameRestrictionPattern;

				if(!storeName.Like(storeNameRestrictionPattern))
					throw new InvalidOperationException($"You are not allowed to save this certificate to \"{storeLocation}/{storeName}\". You are only allowed to save certificates to stores with a name matching the pattern \"{storeNameRestrictionPattern}\".");

				var certificate = new X509Certificate2(bytes.ToArray(), password);

				using(var store = new X509Store(storeName, storeLocation))
				{
					store.Open(OpenFlags.ReadWrite);

					store.Add(certificate);
				}

				return this.Factory.Create(certificate, this.StoreLoader.Get(storeLocation, storeName)!);
			}
			catch(Exception exception)
			{
				throw new InvalidOperationException($"Could not save certificate to {storeLocation}/{storeName}.", exception);
			}
		}

		#endregion
	}
}
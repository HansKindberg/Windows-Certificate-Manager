using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography.X509Certificates;
using Application.Models.Certificates;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Application.Pages
{
	public class IndexModel : SitePageModel
	{
		#region Fields

		private IList<SelectListItem>? _storeLocations;

		#endregion

		#region Constructors

		public IndexModel(ICertificateLoader certificateLoader, ILoggerFactory loggerFactory)
		{
			this.CertificateLoader = certificateLoader ?? throw new ArgumentNullException(nameof(certificateLoader));
			this.Logger = (loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory))).CreateLogger(this.GetType());
		}

		#endregion

		#region Properties

		public virtual SearchFormAction? Action { get; set; }
		protected internal virtual ICertificateLoader CertificateLoader { get; }
		public virtual IList<ICertificate> Certificates { get; } = [];
		public virtual string? Error { get; set; }

		[Display(Name = "Friendly name")]
		public virtual string? FriendlyName { get; set; }

		[Display(Name = "Issuer")]
		public virtual string? Issuer { get; set; }

		protected internal virtual ILogger Logger { get; }

		[Display(Name = "Serial number")]
		public virtual string? SerialNumber { get; set; }

		[Display(Name = "Store-location")]
		public virtual StoreLocation? StoreLocation { get; set; }

		public virtual IList<SelectListItem> StoreLocations => this._storeLocations ??= this.CreateStoreLocationSelection(this.StoreLocation);

		[Display(Name = "Store-name")]
		public virtual string? StoreName { get; set; }

		[Display(Name = "Subject")]
		public virtual string? Subject { get; set; }

		[Display(Name = "Thumbprint")]
		public virtual string? Thumbprint { get; set; }

		#endregion

		#region Methods

		public virtual async Task OnGet(SearchFormAction? action, string? friendlyName = null, string? issuer = null, string? serialNumber = null, StoreLocation? storeLocation = null, string? storeName = null, string? subject = null, string? thumbprint = null)
		{
			await Task.CompletedTask;

			if(action == null)
				return;

			if(action == SearchFormAction.Reset)
			{
				this.Response.Redirect(this.Request.Path);

				return;
			}

			if(string.IsNullOrEmpty(friendlyName) && string.IsNullOrEmpty(issuer) && string.IsNullOrEmpty(serialNumber) && storeLocation == null && string.IsNullOrEmpty(storeName) && string.IsNullOrEmpty(subject) && string.IsNullOrEmpty(thumbprint))
				return;

			this.Action = action;
			this.FriendlyName = friendlyName;
			this.Issuer = issuer;
			this.SerialNumber = serialNumber;
			this.StoreLocation = storeLocation;
			this.StoreName = storeName;
			this.Subject = subject;
			this.Thumbprint = thumbprint;

			try
			{
				foreach(var certificate in this.CertificateLoader.Find(friendlyName, issuer, serialNumber, storeLocation, storeName, subject, thumbprint))
				{
					this.Certificates.Add(certificate);
				}
			}
			catch(Exception exception)
			{
				this.Logger.LogError(exception, exception.ToString());
				this.Error = exception.ToString();
			}
		}

		#endregion
	}
}
using System.Security.Cryptography.X509Certificates;
using Application.Models.Certificates;
using Application.Models.Certificates.Configuration;
using Application.Models.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Application.Pages
{
	public class CertificateModel : SitePageModel
	{
		#region Fields

		private bool? _protectedStoreName;
		private string? _storeNameRestrictionPattern;

		#endregion

		#region Constructors

		public CertificateModel(ICertificateRepository certificateRepository, ILoggerFactory loggerFactory, IOptionsMonitor<WindowsCertificateManagerOptions> optionsMonitor)
		{
			this.CertificateRepository = certificateRepository ?? throw new ArgumentNullException(nameof(certificateRepository));
			this.Logger = (loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory))).CreateLogger(this.GetType());
			this.OptionsMonitor = optionsMonitor ?? throw new ArgumentNullException(nameof(optionsMonitor));
		}

		#endregion

		#region Properties

		public virtual ICertificate? Certificate { get; set; }
		protected internal virtual ICertificateRepository CertificateRepository { get; }
		public virtual string? Confirmation { get; set; }
		public virtual string? Error { get; set; }
		public virtual IList<string> ErrorDetails { get; } = [];
		protected internal virtual ILogger Logger { get; }
		protected internal virtual IOptionsMonitor<WindowsCertificateManagerOptions> OptionsMonitor { get; }
		public virtual bool ProtectedStoreName => this._protectedStoreName ??= this.Certificate != null && this.StoreName != null && !this.StoreName.Like(this.StoreNameRestrictionPattern);

		[BindProperty]
		public virtual string? ReturnUrl { get; set; }

		[BindProperty]
		public virtual StoreLocation? StoreLocation { get; set; }

		[BindProperty]
		public virtual string? StoreName { get; set; }

		public virtual string StoreNameRestrictionPattern => this._storeNameRestrictionPattern ??= this.OptionsMonitor.CurrentValue.StoreNameRestrictionPattern;

		[BindProperty]
		public virtual string? Thumbprint { get; set; }

		public virtual string? ValidatedReturnUrl => this.Url.IsLocalUrl(this.ReturnUrl) ? this.ReturnUrl : null;

		#endregion

		#region Methods

		public virtual async Task OnGet(StoreLocation storeLocation, string storeName, string thumbprint, string? returnUrl = null)
		{
			await Task.CompletedTask;

			this.ReturnUrl = returnUrl;
			this.StoreLocation = storeLocation;
			this.StoreName = storeName;
			this.Thumbprint = thumbprint;

			try
			{
				this.Certificate = this.CertificateRepository.Get(storeLocation, storeName, thumbprint);
			}
			catch(Exception exception)
			{
				this.Logger.LogError(exception, exception.ToString());
				this.Error = exception.ToString();
			}
		}

		public virtual async Task OnPostDelete()
		{
			await Task.CompletedTask;

			if(this.StoreLocation == null)
				this.ErrorDetails.Add("Store-location required.");

			if(string.IsNullOrWhiteSpace(this.StoreName))
				this.ErrorDetails.Add("Store-name required.");
			else if(!this.StoreName!.Like(this.StoreNameRestrictionPattern))
				this.ErrorDetails.Add($"Store-name \"{this.StoreName}\" is not allowed. The store-name must match the pattern \"{this.StoreNameRestrictionPattern}\".");

			if(string.IsNullOrWhiteSpace(this.Thumbprint))
				this.ErrorDetails.Add("Thumbprint required.");

			if(this.StoreLocation != null && this.StoreName != null && this.Thumbprint != null)
			{
				try
				{
					this.Certificate = this.CertificateRepository.Get(this.StoreLocation!.Value, this.StoreName, this.Thumbprint);
				}
				// ReSharper disable EmptyGeneralCatchClause
				catch { }
				// ReSharper restore EmptyGeneralCatchClause
			}

			if(this.ErrorDetails.Any())
			{
				this.Error = "Could not delete. Invalid input.";

				return;
			}

			try
			{
				this.CertificateRepository.Delete(this.StoreLocation!.Value, this.StoreName!, this.Thumbprint!);

				this.Confirmation = $"The certificate \"{this.StoreLocation}/{this.StoreName} - {this.Thumbprint}\" was deleted.";
				this.ModelState.Clear();
				this.Certificate = null;
				this.StoreLocation = null;
				this.StoreName = null;
				this.Thumbprint = null;
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
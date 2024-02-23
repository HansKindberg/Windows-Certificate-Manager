using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography.X509Certificates;
using Application.Models.Certificates;
using Application.Models.Certificates.Configuration;
using Application.Models.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;

namespace Application.Pages
{
	public class AddCertificateModel : SitePageModel
	{
		#region Fields

		private IList<SelectListItem>? _storeLocations;
		private static readonly IEnumerable<string> _validFileContentTypes = ["application/x-pkcs12", "application/x-x509-ca-cert"];

		#endregion

		#region Constructors

		public AddCertificateModel(ICertificateRepository certificateRepository, ILoggerFactory loggerFactory, IOptionsMonitor<WindowsCertificateManagerOptions> optionsMonitor)
		{
			this.CertificateRepository = certificateRepository ?? throw new ArgumentNullException(nameof(certificateRepository));
			this.Logger = (loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory))).CreateLogger(this.GetType());
			this.OptionsMonitor = optionsMonitor ?? throw new ArgumentNullException(nameof(optionsMonitor));
		}

		#endregion

		#region Properties

		[BindProperty]
		[Display(Name = "File")]
		public virtual IFormFile? CertificateFile { get; set; }

		protected internal virtual ICertificateRepository CertificateRepository { get; }
		public virtual string? Confirmation { get; set; }
		public virtual string? Error { get; set; }
		public virtual IList<string> ErrorDetails { get; } = [];
		protected internal virtual ILogger Logger { get; }
		protected internal virtual IOptionsMonitor<WindowsCertificateManagerOptions> OptionsMonitor { get; }

		[BindProperty]
		public virtual string? Password { get; set; }

		[BindProperty]
		[Display(Name = "Store-location")]
		public virtual StoreLocation? StoreLocation { get; set; }

		public virtual IList<SelectListItem> StoreLocations => this._storeLocations ??= this.CreateStoreLocationSelection(this.StoreLocation);

		[BindProperty]
		[Display(Name = "Store-name")]
		public virtual string? StoreName { get; set; }

		protected internal virtual ISet<string> ValidFileContentTypes { get; } = new HashSet<string>(_validFileContentTypes, StringComparer.OrdinalIgnoreCase);

		#endregion

		#region Methods

		public virtual async Task OnPost()
		{
			await Task.CompletedTask;

			if(this.StoreLocation == null)
				this.ErrorDetails.Add("Store-location required.");

			if(string.IsNullOrWhiteSpace(this.StoreName))
			{
				this.ErrorDetails.Add("Store-name required.");
			}
			else
			{
				var storeNameRestrictionPattern = this.OptionsMonitor.CurrentValue.StoreNameRestrictionPattern;

				if(!this.StoreName!.Like(storeNameRestrictionPattern))
					this.ErrorDetails.Add($"Store-name \"{this.StoreName}\" is not allowed. The store-name must match the pattern \"{storeNameRestrictionPattern}\".");
			}

			if(this.CertificateFile == null)
				this.ErrorDetails.Add("File required.");
			else if(this.CertificateFile.Length == 0)
				this.ErrorDetails.Add("File can not be empty.");
			else if(!this.ValidFileContentTypes.Contains(this.CertificateFile.ContentType))
				this.ErrorDetails.Add($"File content-type \"{this.CertificateFile.ContentType}\" is not allowed. Only the following content-types are allowed: {string.Join(", ", this.ValidFileContentTypes)}.");

			if(this.ErrorDetails.Any())
			{
				this.Error = "Could not save. Invalid input.";

				return;
			}

			try
			{
				using(var stream = new MemoryStream())
				{
					await this.CertificateFile!.CopyToAsync(stream);
					var bytes = stream.ToArray();
					this.CertificateRepository.Save(bytes, this.StoreLocation!.Value, this.StoreName!, this.Password);
				}

				this.Confirmation = $"The certificate was saved to \"{this.StoreLocation}/{this.StoreName}\".";
				this.ModelState.Clear();
				this.CertificateFile = null;
				this.StoreLocation = null;
				this.StoreName = null;
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
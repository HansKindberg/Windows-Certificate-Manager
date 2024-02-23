using System.Security.Cryptography.X509Certificates;
using Application.Models.Certificates;
using Application.Models.Certificates.Configuration;
using Application.Models.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Application.Pages
{
	public class StoreModel : SitePageModel
	{
		#region Fields

		private string? _nameRestrictionPattern;
		private bool? _protectedName;

		#endregion

		#region Constructors

		public StoreModel(ILoggerFactory loggerFactory, IOptionsMonitor<WindowsCertificateManagerOptions> optionsMonitor, ICertificateStoreRepository storeRepository)
		{
			this.Logger = (loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory))).CreateLogger(this.GetType());
			this.OptionsMonitor = optionsMonitor ?? throw new ArgumentNullException(nameof(optionsMonitor));
			this.StoreRepository = storeRepository ?? throw new ArgumentNullException(nameof(storeRepository));
		}

		#endregion

		#region Properties

		public virtual IList<ICertificate> Certificates { get; } = [];
		public virtual string? Confirmation { get; set; }
		public virtual string? Error { get; set; }
		public virtual IList<string> ErrorDetails { get; } = [];

		[BindProperty]
		public virtual StoreLocation? Location { get; set; }

		protected internal virtual ILogger Logger { get; }

		[BindProperty]
		public virtual string? Name { get; set; }

		public virtual string NameRestrictionPattern => this._nameRestrictionPattern ??= this.OptionsMonitor.CurrentValue.StoreNameRestrictionPattern;
		protected internal virtual IOptionsMonitor<WindowsCertificateManagerOptions> OptionsMonitor { get; }
		public virtual bool ProtectedName => this._protectedName ??= this.Store != null && this.Name != null && !this.Name.Like(this.NameRestrictionPattern);

		[BindProperty]
		public virtual string? ReturnUrl { get; set; }

		public virtual ICertificateStore? Store { get; set; }
		protected internal virtual ICertificateStoreRepository StoreRepository { get; }
		public virtual string? ValidatedReturnUrl => this.Url.IsLocalUrl(this.ReturnUrl) ? this.ReturnUrl : null;

		#endregion

		#region Methods

		public virtual async Task OnGet(StoreLocation location, string name, string? returnUrl = null)
		{
			await Task.CompletedTask;

			this.Location = location;
			this.Name = name;
			this.ReturnUrl = returnUrl;

			try
			{
				this.Store = this.StoreRepository.Get(location, name);

				if(this.Store != null)
				{
					foreach(var certificate in this.Store!.Certificates())
					{
						this.Certificates.Add(certificate);
					}
				}
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

			if(this.Location == null)
				this.ErrorDetails.Add("Location required.");

			if(string.IsNullOrWhiteSpace(this.Name))
				this.ErrorDetails.Add("Name required.");
			else if(!this.Name.Like(this.NameRestrictionPattern))
				this.ErrorDetails.Add($"Name \"{this.Name}\" is not allowed. The name must match the pattern \"{this.NameRestrictionPattern}\".");

			if(this.Location != null && this.Name != null)
			{
				try
				{
					this.Store = this.StoreRepository.Get(this.Location!.Value, this.Name!);

					if(this.Store != null)
					{
						foreach(var certificate in this.Store!.Certificates())
						{
							this.Certificates.Add(certificate);
						}
					}
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
				this.StoreRepository.Delete(this.Location!.Value, this.Name!);

				this.Confirmation = $"The store \"{this.Location}/{this.Name}\" was deleted.";
				this.ModelState.Clear();
				this.Certificates.Clear();
				this.Location = null;
				this.Name = null;
				this.Store = null;
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
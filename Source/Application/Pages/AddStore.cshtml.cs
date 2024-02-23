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
	public class AddStoreModel : SitePageModel
	{
		#region Fields

		private IList<SelectListItem>? _locations;

		#endregion

		#region Constructors

		public AddStoreModel(ICertificateStoreRepository storeRepository, ILoggerFactory loggerFactory, IOptionsMonitor<WindowsCertificateManagerOptions> optionsMonitor)
		{
			this.Logger = (loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory))).CreateLogger(this.GetType());
			this.StoreRepository = storeRepository ?? throw new ArgumentNullException(nameof(storeRepository));
			this.OptionsMonitor = optionsMonitor ?? throw new ArgumentNullException(nameof(optionsMonitor));
		}

		#endregion

		#region Properties

		public virtual string? Confirmation { get; set; }
		public virtual string? Error { get; set; }
		public virtual IList<string> ErrorDetails { get; } = [];

		[BindProperty]
		[Display(Name = "Location")]
		public virtual StoreLocation? Location { get; set; }

		public virtual IList<SelectListItem> Locations => this._locations ??= this.CreateStoreLocationSelection(this.Location);
		protected internal virtual ILogger Logger { get; }

		[BindProperty]
		[Display(Name = "Name")]
		public virtual string? Name { get; set; }

		protected internal virtual IOptionsMonitor<WindowsCertificateManagerOptions> OptionsMonitor { get; }
		protected internal virtual ICertificateStoreRepository StoreRepository { get; }

		#endregion

		#region Methods

		public virtual async Task OnPost()
		{
			await Task.CompletedTask;

			if(this.Location == null)
				this.ErrorDetails.Add("Location required.");

			if(string.IsNullOrWhiteSpace(this.Name))
			{
				this.ErrorDetails.Add("Name required.");
			}
			else
			{
				var storeNameRestrictionPattern = this.OptionsMonitor.CurrentValue.StoreNameRestrictionPattern;

				if(!this.Name!.Like(storeNameRestrictionPattern))
					this.ErrorDetails.Add($"Name \"{this.Name}\" is not allowed. The name must match the pattern \"{storeNameRestrictionPattern}\".");
			}

			if(!this.ErrorDetails.Any())
			{
				try
				{
					if(this.StoreRepository.Get(this.Location!.Value, this.Name!) != null)
						this.ErrorDetails.Add($"The store \"{this.Location}/{this.Name}\" already exists.");
				}
				// ReSharper disable EmptyGeneralCatchClause
				catch { }
				// ReSharper restore EmptyGeneralCatchClause
			}

			if(this.ErrorDetails.Any())
			{
				this.Error = "Could not save. Invalid input.";

				return;
			}

			try
			{
				this.StoreRepository.Save(this.Location!.Value, this.Name!);

				this.Confirmation = $"The certificate-store \"{this.Location}/{this.Name}\" was saved.";
				this.ModelState.Clear();
				this.Location = null;
				this.Name = null;
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
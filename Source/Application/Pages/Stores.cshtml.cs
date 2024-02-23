using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography.X509Certificates;
using Application.Models.Certificates;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Application.Pages
{
	public class StoresModel : SitePageModel
	{
		#region Fields

		private IList<SelectListItem>? _locations;

		#endregion

		#region Constructors

		public StoresModel(ILoggerFactory loggerFactory, ICertificateStoreLoader storeLoader)
		{
			this.Logger = (loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory))).CreateLogger(this.GetType());
			this.StoreLoader = storeLoader ?? throw new ArgumentNullException(nameof(storeLoader));
		}

		#endregion

		#region Properties

		public virtual SearchFormAction? Action { get; set; }
		public virtual string? Error { get; set; }

		[Display(Name = "Location")]
		public virtual StoreLocation? Location { get; set; }

		public virtual IList<SelectListItem> Locations => this._locations ??= this.CreateStoreLocationSelection(this.Location);
		protected internal virtual ILogger Logger { get; }

		[Display(Name = "Name")]
		public virtual string? Name { get; set; }

		protected internal virtual ICertificateStoreLoader StoreLoader { get; }
		public virtual IDictionary<StoreLocation, IList<ICertificateStore>> Stores { get; } = new SortedDictionary<StoreLocation, IList<ICertificateStore>>();

		#endregion

		#region Methods

		public virtual async Task OnGet(SearchFormAction? action, StoreLocation? location = null, string? name = null)
		{
			await Task.CompletedTask;

			if(action == null)
				return;

			if(action == SearchFormAction.Reset)
			{
				this.Response.Redirect(this.Request.Path);
				return;
			}

			if(location == null && string.IsNullOrEmpty(name))
				return;

			this.Action = action;
			this.Location = location;
			this.Name = name;

			try
			{
				foreach(var store in this.StoreLoader.Find(location, name))
				{
					if(!this.Stores.TryGetValue(store.Location, out var list))
					{
						list = [];
						this.Stores.Add(store.Location, list);
					}

					list.Add(store);
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
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Application.Pages
{
	public abstract class SitePageModel : PageModel
	{
		#region Methods

		protected internal virtual IList<SelectListItem> CreateStoreLocationSelection(StoreLocation? storeLocation)
		{
			var selection = new List<SelectListItem> { new() };

			foreach(var location in Enum.GetValues<StoreLocation>())
			{
				selection.Add(new SelectListItem(location.ToString(), location.ToString(), location == storeLocation));
			}

			selection.Sort((first, second) => string.Compare(first.Text, second.Text, StringComparison.OrdinalIgnoreCase));

			return selection;
		}

		#endregion
	}
}
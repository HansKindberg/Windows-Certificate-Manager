namespace Application.Models.Certificates.Configuration
{
	public class WindowsCertificateManagerOptions
	{
		#region Properties

		/// <summary>
		/// It is only allowed to add and delete stores with a name matching this pattern. It is also only allowed to add and delete certificates from stores with a name matching this pattern. Default is "__*", that is a name starting with two underscores, "__".
		/// </summary>
		public virtual string StoreNameRestrictionPattern { get; set; } = "__*";

		#endregion
	}
}
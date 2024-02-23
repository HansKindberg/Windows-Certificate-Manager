using System.Security.Cryptography.X509Certificates;

namespace Application.Models.Certificates
{
	public interface ICertificateStoreRepository : ICertificateStoreLoader
	{
		#region Methods

		void Delete(StoreLocation location, string name);
		ICertificateStore Save(StoreLocation location, string name);

		#endregion
	}
}
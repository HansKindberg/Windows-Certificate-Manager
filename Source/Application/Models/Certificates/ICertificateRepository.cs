using System.Security.Cryptography.X509Certificates;

namespace Application.Models.Certificates
{
	public interface ICertificateRepository : ICertificateLoader
	{
		#region Methods

		void Delete(StoreLocation storeLocation, string storeName, string thumbprint);
		ICertificate Save(IEnumerable<byte> bytes, StoreLocation storeLocation, string storeName, string? password = null);

		#endregion
	}
}
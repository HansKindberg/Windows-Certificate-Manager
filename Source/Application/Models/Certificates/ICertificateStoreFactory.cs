using System.Security.Cryptography.X509Certificates;

namespace Application.Models.Certificates
{
	public interface ICertificateStoreFactory
	{
		#region Methods

		ICertificateStore Create(StoreLocation location, StoreName? name, string? registryName);

		#endregion
	}
}
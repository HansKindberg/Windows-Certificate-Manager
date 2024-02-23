using System.Security.Cryptography.X509Certificates;

namespace Application.Models.Certificates
{
	public interface ICertificateFactory
	{
		#region Methods

		ICertificate Create(X509Certificate2 certificate, ICertificateStore store);

		#endregion
	}
}
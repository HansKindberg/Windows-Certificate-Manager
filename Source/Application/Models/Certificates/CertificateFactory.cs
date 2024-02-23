using System.Security.Cryptography.X509Certificates;

namespace Application.Models.Certificates
{
	public class CertificateFactory : ICertificateFactory
	{
		#region Methods

		public virtual ICertificate Create(X509Certificate2 certificate, ICertificateStore store)
		{
			return new Certificate
			{
				Archived = certificate.Archived,
				FriendlyName = certificate.FriendlyName,
				HasPrivateKey = certificate.HasPrivateKey,
				Issuer = certificate.Issuer,
				NotAfter = certificate.NotAfter,
				NotBefore = certificate.NotBefore,
				RawData = certificate.RawData.ToArray(),
				SerialNumber = certificate.SerialNumber,
				SignatureAlgorithm = certificate.SignatureAlgorithm,
				Store = store,
				StringRepresentation = certificate.ToString(),
				Subject = certificate.Subject,
				Version = certificate.Version,
				Thumbprint = certificate.Thumbprint
			};
		}

		#endregion
	}
}
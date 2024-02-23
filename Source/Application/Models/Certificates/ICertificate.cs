using System.Security.Cryptography;

namespace Application.Models.Certificates
{
	public interface ICertificate
	{
		#region Properties

		bool Archived { get; }
		string? FriendlyName { get; }
		bool HasPrivateKey { get; }
		string? Issuer { get; }
		DateTime? NotAfter { get; }
		DateTime? NotBefore { get; }
		IEnumerable<byte>? RawData { get; }
		string? SerialNumber { get; }
		Oid? SignatureAlgorithm { get; }
		ICertificateStore? Store { get; }
		string? StringRepresentation { get; }
		string? Subject { get; }
		string? Thumbprint { get; }
		int Version { get; }

		#endregion
	}
}
using System.Security.Cryptography;

namespace Application.Models.Certificates
{
	public class Certificate : ICertificate
	{
		#region Properties

		public virtual bool Archived { get; set; }
		public virtual string? FriendlyName { get; set; }
		public virtual bool HasPrivateKey { get; set; }
		public virtual string? Issuer { get; set; }
		public virtual DateTime? NotAfter { get; set; }
		public virtual DateTime? NotBefore { get; set; }
		public virtual IEnumerable<byte>? RawData { get; set; }
		public virtual string? SerialNumber { get; set; }
		public virtual Oid? SignatureAlgorithm { get; set; }
		public virtual ICertificateStore? Store { get; set; }
		public virtual string? StringRepresentation { get; set; }
		public virtual string? Subject { get; set; }
		public virtual string? Thumbprint { get; set; }
		public virtual int Version { get; set; }

		#endregion
	}
}
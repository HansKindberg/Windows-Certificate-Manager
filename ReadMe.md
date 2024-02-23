# Windows-Certificate-Manager

Web-application for managing certificates on Windows.

## 1 Development

- Windows only: [&lt;TargetFramework&gt;net8.0-windows&lt;/TargetFramework&gt;](/Directory.Build.props#L11)

### 1.1 Certificates

The [.certificates](/.certificates) folder contains certificates to use for testing/laborating. The certificates are **only** for testing/laborating.

- **client-certificate-1.pfx** - password = **password**
- **client-certificate-2.pfx** - password = **password**
- **client-certificate-3.pfx** - password = **password**
- **client-certificate-4.pfx** - password = **password**
- **intermediate-certificate-1.crt**
- **intermediate-certificate-2.crt**
- **intermediate-certificate-3.crt**
- **intermediate-certificate-4.crt**
- **root-certificate.crt**

The certificates are created by using this web-application, [Certificate-Factory](https://github.com/HansKindberg-Lab/Certificate-Factory). It is a web-application you can run in Visual Studio and then upload a json-file like this:

	{
		"Defaults": {
			"HashAlgorithm": "Sha256",
			"NotAfter": "2050-01-01"
		},
		"Roots": {
			"root-certificate": {
				"Certificate": {
					"Subject": "CN=Windows-Certificate-Manager Root CA"
				},
				"IssuedCertificates": {
					"intermediate-certificate-1": {
						"Certificate": {
							"CertificateAuthority": {
								"PathLengthConstraint": 0
							},
							"KeyUsage": "KeyCertSign",
							"Subject": "CN=Windows-Certificate-Manager Intermediate CA 1"
						},
						"IssuedCertificates": {
							"client-certificate-1": {
								"Certificate": {
									"EnhancedKeyUsage": "ClientAuthentication",
									"KeyUsage": "DigitalSignature",
									"Subject": "CN=Windows-Certificate-Manager client-certificate 1"
								}
							}
						}
					},
					"intermediate-certificate-2": {
						"Certificate": {
							"CertificateAuthority": {
								"PathLengthConstraint": 0
							},
							"KeyUsage": "KeyCertSign",
							"Subject": "CN=Windows-Certificate-Manager Intermediate CA 2"
						},
						"IssuedCertificates": {
							"client-certificate-2": {
								"Certificate": {
									"EnhancedKeyUsage": "ClientAuthentication",
									"KeyUsage": "DigitalSignature",
									"Subject": "CN=Windows-Certificate-Manager client-certificate 2"
								}
							}
						}
					},
					"intermediate-certificate-3": {
						"Certificate": {
							"CertificateAuthority": {
								"PathLengthConstraint": 0
							},
							"KeyUsage": "KeyCertSign",
							"Subject": "CN=Windows-Certificate-Manager Intermediate CA 3"
						},
						"IssuedCertificates": {
							"client-certificate-3": {
								"Certificate": {
									"EnhancedKeyUsage": "ClientAuthentication",
									"KeyUsage": "DigitalSignature",
									"Subject": "CN=Windows-Certificate-Manager client-certificate 3"
								}
							}
						}
					},
					"intermediate-certificate-4": {
						"Certificate": {
							"CertificateAuthority": {
								"PathLengthConstraint": 0
							},
							"KeyUsage": "KeyCertSign",
							"Subject": "CN=Windows-Certificate-Manager Intermediate CA 4"
						},
						"IssuedCertificates": {
							"client-certificate-4": {
								"Certificate": {
									"EnhancedKeyUsage": "ClientAuthentication",
									"KeyUsage": "DigitalSignature",
									"Subject": "CN=Windows-Certificate-Manager client-certificate 4"
								}
							}
						}
					}
				}
			}
		},
		"RootsDefaults": {
			"CertificateAuthority": {
				"PathLengthConstraint": null
			},
			"KeyUsage": "KeyCertSign"
		}
	}

## 2 Configuration

With this application you can delete certificates and stores from a windows system. For security reasons there is a default configuration:

- [StoreNameRestrictionPattern](/Source/Application/Models/Certificates/Configuration/WindowsCertificateManagerOptions.cs#L10)

It is only allowed to add and delete stores with a name matching this pattern. It is also only allowed to add and delete certificates from stores with a name matching this pattern. Default is "\_\_\*", that is a name starting with two underscores, "\_\_".

You can change this configuration in appsettings.json.

If you want to allow anything:

	{
		...,
		"WindowsCertificateManager": {
			"StoreNameRestrictionPattern": "*"
		},
		...
	}

If you only want to allow stores with a guid as name:

	{
		...,
		"WindowsCertificateManager": {
			"StoreNameRestrictionPattern": "********-****-****-****-************"
		},
		...
	}

Used for the integration-tests: [appsettings.json](/Tests/Integration-tests/appsettings.json#L3)

## 3 Tests

We need to run Visual Studio with elevated privileges, "Run as Administrator", if we want all the tests to succeed. If not, some of the tests involving certificate-store "LocalMachine" will fail.

## 4 Links

- [Get List of Certificate Store Names in C#](https://stackoverflow.com/questions/4036939/get-list-of-certificate-store-names-in-c-sharp)
- [Removing a Windows System Certificate Store](http://www.digitallycreated.net/Blog/58/removing-a-windows-system-certificate-store)
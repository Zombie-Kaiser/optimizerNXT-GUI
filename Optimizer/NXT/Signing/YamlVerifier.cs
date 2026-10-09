using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace optimizerNXT {
    public static class YamlVerifier {
        public static bool Verify(string yamlPath)
        {
            string sigPath = yamlPath + ".sig";
            if (!File.Exists(sigPath))
            {
                Logger.Error($"Signature file not found: {sigPath}");
                return false;
            }

            using (FileStream fs = File.OpenRead(sigPath))
            using (BinaryReader br = new BinaryReader(fs, Encoding.UTF8))
            {
                // Validate header
                byte[] header = br.ReadBytes(4);
                if (header[0] != 'D' || header[1] != 'E' || header[2] != 'A' || header[3] != 'D')
                {
                    Logger.Error($"Invalid signature file header: {sigPath}");
                    return false;
                }

                ushort version = br.ReadUInt16();
                if (version != 1)
                {
                    Logger.Error($"Invalid signature version {version}: {sigPath}");
                    return false;
                }

                // Read blobs
                string fileName = ReadBlobString(br);
                string metadataText = ReadBlobString(br);
                byte[] signature = ReadBlobBytes(br);

                // File name match check
                if (!string.Equals(fileName, Path.GetFileName(yamlPath), StringComparison.OrdinalIgnoreCase))
                {
                    Logger.Error($"File name does not match the signature: {sigPath}");
                    return false;
                }

                // Compute hash from fileName + yaml + metadata
                byte[] yamlBytes = File.ReadAllBytes(yamlPath);
                byte[] hash = ComputeHash(fileName, yamlBytes, Encoding.UTF8.GetBytes(metadataText));

                // Verify using embedded public key
                using (RSA rsa = GetEmbeddedPublicKey())
                {
                    if (!rsa.VerifyData(hash, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1))
                    {
                        Logger.Error($"Signature verification failed: {sigPath}");
                        return false;
                    }
                }

                Logger.Info($"Signature valid. YAML is authentic: {sigPath}");
                return true;
            }
        }

        private static byte[] ComputeHash(string fileName, byte[] yaml, byte[] metadata)
        {
            using (SHA256 sha = SHA256.Create())
            using (MemoryStream ms = new MemoryStream())
            {
                WriteUtf8(ms, fileName);
                ms.Write(yaml, 0, yaml.Length);
                ms.Write(metadata, 0, metadata.Length);

                ms.Position = 0;
                return sha.ComputeHash(ms);
            }
        }

        private static void WriteUtf8(Stream s, string value)
        {
            byte[] b = Encoding.UTF8.GetBytes(value);
            s.Write(b, 0, b.Length);
        }

        private static string ReadBlobString(BinaryReader br)
        {
            int len = br.ReadInt32();
            byte[] b = br.ReadBytes(len);
            return Encoding.UTF8.GetString(b);
        }

        private static byte[] ReadBlobBytes(BinaryReader br)
        {
            int len = br.ReadInt32();
            return br.ReadBytes(len);
        }

        private static RSA GetEmbeddedPublicKey()
        {
            string resourceName = "optimizerNXT.Resources.pubkey.xml";
            var assembly = Assembly.GetExecutingAssembly();
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                using (var reader = new StreamReader(stream))
                {
                    string xml = reader.ReadToEnd();
                    var rsa = new RSACryptoServiceProvider();
                    rsa.FromXmlString(xml);
                    return rsa;
                }
            }
        }
    }
}

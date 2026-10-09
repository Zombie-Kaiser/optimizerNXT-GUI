using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace YamlSigner
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                PrintUsage();
                return;
            }

            switch (args[0].ToLowerInvariant())
            {
                case "genkeys":
                    GenerateKeys();
                    break;
                case "sign":
                    if (args.Length < 2)
                    {
                        Console.WriteLine("Usage: YamlSigner.exe sign <file.yaml>");
                        return;
                    }
                    SignFile(args[1]);
                    break;
                case "sign-all":
                    if (args.Length < 2)
                    {
                        Console.WriteLine("Usage: YamlSigner.exe sign-all <directory>");
                        return;
                    }
                    SignAll(args[1]);
                    break;
                default:
                    PrintUsage();
                    break;
            }
        }

        static void PrintUsage()
        {
            Console.WriteLine("YamlSigner - YAML Package Signing Tool for OptimizerNXT");
            Console.WriteLine();
            Console.WriteLine("Usage:");
            Console.WriteLine("  YamlSigner.exe genkeys              Generate new RSA key pair (pubkey.xml + privkey.xml)");
            Console.WriteLine("  YamlSigner.exe sign <file.yaml>      Sign a single YAML file");
            Console.WriteLine("  YamlSigner.exe sign-all <directory>  Sign all .yaml files in a directory");
        }

        static void GenerateKeys()
        {
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(2048))
            {
                string pubKey = rsa.ToXmlString(false);
                string privKey = rsa.ToXmlString(true);

                File.WriteAllText("pubkey.xml", pubKey);
                File.WriteAllText("privkey.xml", privKey);

                Console.WriteLine("Generated key pair:");
                Console.WriteLine("  pubkey.xml  (public key - embed in app)");
                Console.WriteLine("  privkey.xml (private key - KEEP SECRET!)");
            }
        }

        static RSA LoadPrivateKey()
        {
            if (!File.Exists("privkey.xml"))
            {
                Console.WriteLine("Error: {0} not found. Run 'genkeys' first.", "privkey.xml");
                return null;
            }
            string xml = File.ReadAllText("privkey.xml");
            var rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(xml);
            return rsa;
        }

        static string BuildMetadata()
        {
            return "Author=Zombie-Kaiser\r\nTool=OptimizerNXT-GUI\r\nUpdatedAtUtc=" + DateTime.UtcNow.ToString("o") + "\r\n";
        }

        static byte[] ComputeHash(string fileName, byte[] yaml, byte[] metadata)
        {
            using (SHA256 sha = SHA256.Create())
            using (MemoryStream ms = new MemoryStream())
            {
                byte[] fileNameBytes = Encoding.UTF8.GetBytes(fileName);
                ms.Write(fileNameBytes, 0, fileNameBytes.Length);
                ms.Write(yaml, 0, yaml.Length);
                ms.Write(metadata, 0, metadata.Length);
                ms.Position = 0;
                return sha.ComputeHash(ms);
            }
        }

        static void SignFile(string yamlPath)
        {
            RSA rsa = LoadPrivateKey();
            if (rsa == null) return;

            if (!File.Exists(yamlPath))
            {
                Console.WriteLine("Error: {0} not found.", yamlPath);
                return;
            }

            string fileName = Path.GetFileName(yamlPath);
            byte[] yamlBytes = File.ReadAllBytes(yamlPath);
            string metadata = BuildMetadata();
            byte[] metadataBytes = Encoding.UTF8.GetBytes(metadata);

            byte[] hash = ComputeHash(fileName, yamlBytes, metadataBytes);
            byte[] signature = rsa.SignData(hash, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

            string sigPath = yamlPath + ".sig";
            using (FileStream fs = File.Create(sigPath))
            using (BinaryWriter bw = new BinaryWriter(fs, Encoding.UTF8))
            {
                bw.Write((byte)'D');
                bw.Write((byte)'E');
                bw.Write((byte)'A');
                bw.Write((byte)'D');
                bw.Write((ushort)1);
                WriteBlobString(bw, fileName);
                WriteBlobString(bw, metadata);
                WriteBlobBytes(bw, signature);
            }

            Console.WriteLine("Signed: {0} -> {1}", fileName, Path.GetFileName(sigPath));
        }

        static void SignAll(string directory)
        {
            RSA rsa = LoadPrivateKey();
            if (rsa == null) return;

            if (!Directory.Exists(directory))
            {
                Console.WriteLine("Error: {0} not found.", directory);
                return;
            }

            string[] yamlFiles = Directory.GetFiles(directory, "*.yaml");
            Console.WriteLine("Found {0} YAML files to sign.", yamlFiles.Length);

            foreach (string yamlPath in yamlFiles)
            {
                SignFile(yamlPath);
            }

            Console.WriteLine("Done. Signed {0} files.", yamlFiles.Length);
        }

        static void WriteBlobString(BinaryWriter bw, string value)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(value);
            bw.Write(bytes.Length);
            bw.Write(bytes);
        }

        static void WriteBlobBytes(BinaryWriter bw, byte[] bytes)
        {
            bw.Write(bytes.Length);
            bw.Write(bytes);
        }
    }
}

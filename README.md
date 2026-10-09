<p align="center">
  <h1>OptimizerNXT GUI</h1>
  <p>The Finest Windows Optimizer — NXT GUI Edition</p>
  <p>Author: <a href="https://github.com/Zombie-Kaiser">Zombie-Kaiser</a></p>
</p>

---

## Overview

OptimizerNXT GUI combines the classic [Optimizer](https://github.com/hellzerg/optimizer) WinForms interface with the powerful [OptimizerNXT](https://github.com/hellzerg/optimizerNXT) YAML engine. It provides a tabbed GUI for applying signed YAML optimization packages to Windows, alongside all the original Optimizer features.

## Key Features

### Original Optimizer Features
- Full multilingual support (24 languages, including Chinese 简体中文)
- Enhance system and network performance
- Disable unnecessary Windows services
- Turn off Windows telemetry, Cortana, and more
- Disable Office telemetry (Office 2016+)
- Stop automatic Windows 10/11 updates
- Disable CoPilot AI in Windows 11 & Edge
- Enable UTC time globally
- Advanced tweaks (HPET, OneDrive, etc.)
- Uninstall UWP apps
- Clean system drive and browser profiles
- Fix common registry issues
- Ping IPs and assess latency
- Quickly change DNS server (from a pre-made list)
- Flush DNS cache
- Remove unwanted startup programs
- Edit your HOSTS file
- Edit your System Variables paths
- Hardware inspection tool
- Add items to the desktop right-click menu
- Define custom commands for the run dialog

### New NXT YAML Engine Features
- **YAML Tab** — browse and apply 28 pre-built signed optimization packages
- **Signed YAML enforcement** — RSA SHA256 signature verification for all packages
- **Conditional execution** — packages auto-skip steps based on Windows version and bitness
- **Process Control** — deny or allow specific processes
- **Shell execution** — run custom commands via cmd/powershell
- **Granular service management** — stop/start/disable/enable services
- **Typed registry operations** — add/delete registry keys and values with ownership support
- **DNS presets** — Cloudflare, Quad9, Google, OpenDNS, AdGuard, and more
- **Startup management** — add/remove startup items (registry + folder)
- **Hosts file editing** — add/remove hosts entries
- **UWP app removal** — uninstall modern Windows apps
- **Browse & Apply** — load custom YAML files from anywhere on disk
- **Live log panel** — real-time execution output in the YAML tab
- **Chinese language support** — YAML tab UI fully localized for 简体中文

## YAML Tab Usage

1. Switch to the **YAML** tab
2. Check the packages you want to apply from the list (28 available)
3. Click **Apply Selected** (or **应用所选** in Chinese)
4. Watch the log panel for real-time execution output
5. Alternatively, click **Browse File...** to apply a custom YAML file

All YAML packages are cryptographically signed. Files that fail signature verification are automatically skipped.

## YAML Signing & Custom Packages

### How Signing Works

Every YAML package is cryptographically signed using **RSA-2048 + SHA256 + PKCS1**. The app embeds a public key and verifies each `.sig` file before execution — tampered or unsigned YAML files are rejected.

**Signature format (`.sig` file):**
```
Header:     "DEAD" (4 bytes)
Version:    1 (ushort)
FileName:   length-prefixed UTF8 string
Metadata:   length-prefixed UTF8 string (Author, Tool, UpdatedAtUtc)
Signature:  length-prefixed RSA-SHA256-PKCS1 bytes
```

The signature covers `SHA256(fileName + yamlContent + metadata)`, verified against the embedded public key.

### Running Custom YAML Configurations

You **can** run your own custom YAML files — but they must be signed with your private key first. A signing tool is included in `tools/YamlSigner/`.

**Step 1: Build the signing tool**
```bash
cd tools/YamlSigner
csc.exe -out:YamlSigner.exe -target:exe YamlSigner.cs
```

**Step 2: Generate your key pair** (already done in this repo)
```bash
YamlSigner.exe genkeys
```
This creates:
- `pubkey.xml` — public key (embedded in the app, safe to share)
- `privkey.xml` — **private key (KEEP SECRET! Never commit to GitHub!)**

**Step 3: Sign your custom YAML**
```bash
YamlSigner.exe sign C:\path\to\your-custom.yaml
```
This creates `your-custom.yaml.sig` next to your YAML file.

**Step 4: Apply in the app**
- Open OptimizerNXT, go to the **YAML** tab
- Click **Browse File...** and select your signed YAML
- The app verifies the signature and executes it

### Signing Tool Commands

| Command | Description |
|---|---|
| `YamlSigner.exe genkeys` | Generate a new RSA-2048 key pair |
| `YamlSigner.exe sign <file.yaml>` | Sign a single YAML file |
| `YamlSigner.exe sign-all <directory>` | Sign all `.yaml` files in a directory |

### Security Notes

- The private key (`privkey.xml`) is listed in `.gitignore` and will **never** be committed to GitHub
- Only the public key (`pubkey.xml`) is embedded in the executable
- If you lose `privkey.xml`, you cannot sign new packages — back it up securely
- All 28 pre-built packages are pre-signed with the repo's key pair
- Re-signing is required after modifying any YAML file content

## Downloads

Find the latest release on the [Releases](https://github.com/Zombie-Kaiser/optimizerNXT-GUI/releases) page.

## Compatibility

- Requires .NET Framework 4.8.1
- Compatible with Windows 7, 8, 8.1, 10, 11
- Can run on Windows Server 2008, 2012, 2016, 2019, 2022 using `/unsafe` switch

## Details

- Latest version: 17.0
- Author: Zombie-Kaiser
- GitHub: https://github.com/Zombie-Kaiser/optimizerNXT-GUI

## Credits

- Original Optimizer by [deadmoon](https://github.com/hellzerg/optimizer)
- OptimizerNXT CLI by [deadmoon](https://github.com/hellzerg/optimizerNXT)
- [ByteSize](https://github.com/omar/ByteSize) by Omar Rahman
- ColorPicker theme engine by [vadiscode](https://github.com/vadiscode)

## License

[GNU GPL 3.0](https://www.gnu.org/licenses/gpl-3.0.en.html)

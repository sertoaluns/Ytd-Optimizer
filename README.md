# 🎮 GTA5 YTD Batch Texture Optimizer (Alpha-Preserving DXT5)

[![.NET Framework](https://img.shields.io/badge/.NET%20Framework-4.8%20(x64)-blue.svg)](https://dotnet.microsoft.com/)
[![Language](https://img.shields.io/badge/Language-C%23%20WinForms-brightgreen.svg)]()
[![Platform](https://img.shields.io/badge/Platform-Windows%20x64-lightgrey.svg)]()
[![License](https://img.shields.io/badge/License-MIT-orange.svg)](LICENSE)
[![Languages](https://img.shields.io/badge/Languages-EN%20%7C%20DE%20%7C%20TR-purple.svg)]()

> High-performance Windows desktop application designed to batch optimize and downscale oversized GTA 5 addon and clothing textures (e.g. from `grzyClothTool` stream exports) strictly to **DDS DXT5 (BC3_UNORM)** while **guaranteeing 100% alpha transparency preservation** with mathematical verification.

---

## 🌍 Language / Sprache / Dil
- [English](#-english)
- [Deutsch](#-deutsch)
- [Türkçe](#-türkçe)

---

## 🇬🇧 English

### Overview
GTA 5 clothing and prop mods frequently contain massive 2K or 4K textures saved in uncompressed or inefficient formats (e.g. BC7, uncompressed RGBA). These cause severe FiveM texture budget warnings, streaming crashes, and stuttering.

Standard tools frequently corrupt the alpha channel of badges, decals, semi-transparent t-shirts, and logos—turning transparent areas into black/white halos or stripping alpha entirely. **GTA5 YTD Optimizer** solves this with a dedicated, non-destructive **Alpha-Preserving Pipeline**.

### Key Features
* 🛡 **Guaranteed Alpha Transparency Pipeline**:
  1. Multiplies RGB color channels by alpha (*Premultiply*).
  2. Downscales using high-quality **Lanczos3 / Bicubic** filtering.
  3. Divides RGB by alpha (*Un-premultiply*).
  4. Applies **Edge Dilation** on transparent borders ($\alpha = 0$) to eliminate white/black halo artifacts.
  5. Compresses strictly to DDS DXT5 (BC3_UNORM) with mipmaps via Microsoft `texconv.exe`.
  6. **Dual Verification**: Compares source alpha channel with the result. If average difference exceeds 2% or transparent area delta exceeds 3%, original texture is preserved untouched.
* ⚡ **Multi-Folder & Multi-Threaded**: Queue multiple folders or drag-and-drop entire mod directories simultaneously. Processes YTD files concurrently across multi-core CPUs.
* 🔗 **Automatic YDD & YTD Matching**: Pairs 3D drawables (`.ydd`) with corresponding texture dictionaries (`.ytd`) based on naming conventions (e.g. `accs_001_u` $\leftrightarrow$ `accs_diff_001_a_uni`).
* 🔍 **Side-by-Side Checkerboard Preview**: Right-click any texture to inspect original vs optimized texture side-by-side on an alpha checkerboard grid with instant mathematical verification stats.
* 🌐 **Multi-Language Support**: Switch dynamically between English (EN), German (DE), and Turkish (TR).
* 🔒 **Automatic Backup & Integrity Checking**: Creates `.bak` backups before writing. Re-opens each saved YTD file to verify headers; automatically restores from `.bak` if corrupted.
* ✏ **In-Place Texture Management**: Right-click to replace any individual texture with PNG/DDS (automatically resized and converted) or delete textures from YTD dictionaries.

### Dimension Rules
* Standard target sizes: `1024x1024`, `1024x512`, `512x1024`.
* Existing textures at or below target dimensions are kept unchanged.
* Squares larger than 1024 become `1024x1024`.
* 2:1 Horizontal textures larger than 1024x512 become `1024x512`.
* 1:2 Vertical textures larger than 512x1024 become `512x1024`.
* All dimensions remain powers of two with strictly maintained aspect ratios.

---

## 🇩🇪 Deutsch

### Übersicht
GTA 5 Kleidungs- und Addon-Mods enthalten häufig überdimensionierte 2K- oder 4K-Texturen in ineffizienten Formaten (z. B. BC7 oder unkomprimiertes RGBA), was zu FiveM-Texturbudget-Warnungen, Abstürzen und Rucklern führt.

Herkömmliche Konverter zerstören häufig den Alphakanal von Abzeichen, Logos oder transparenten Kleidungsstücken. Der **GTA5 YTD Optimizer** verhindert dies durch eine mathematisch geprüfte **Alpha-Erhaltungs-Pipeline**.

### Hauptmerkmale
* 🛡 **Garantierte Alphakanal-Erhaltung**: Premultiply $\rightarrow$ Lanczos3/Bicubic Skalierung $\rightarrow$ Un-premultiply $\rightarrow$ Farbkanten-Erweiterung (Dilation) gegen Kantenränder $\rightarrow$ DXT5-Komprimierung mit Mipmaps $\rightarrow$ Mathematische Validierung.
* ⚡ **Stapelverarbeitung & Mehrere Ordner**: Fügen Sie mehrere Ordner per Klick oder Drag & Drop hinzu. Schnelle parallele Verarbeitung auf Multi-Core-Prozessoren.
* 🔗 **Automatische YDD- & YTD-Zuordnung**: Erkennt Modelldateien (`.ydd`) und zugehörige Texturarchive (`.ytd`) anhand des Namenschemas.
* 🔍 **Schachbrett-Vorschau**: Texturen nebeneinander mit Transparenzgitter und Prüfbericht vergleichen.
* 🌐 **Mehrsprachig**: Umschaltbar zwischen Englisch, Deutsch und Türkisch.
* 🔒 **Sicherheitskopie (.bak)**: Automatische Backup-Erstellung und Wiederherstellung bei fehlerhaften Dateien.

---

## 🇹🇷 Türkçe

### Genel Bakış
GTA 5 kıyafet ve addon modlarında (özellikle `grzyClothTool` çıktıları) gelen yüksek çözünürlüklü (2K, 4K) dokuları toplu olarak küçültüp **DDS DXT5 (BC3_UNORM)** formatına dönüştüren, YDD ve YTD dosyalarını otomatik eşleştiren masaüstü aracıdır.

En kritik kural: **Şeffaflık (alfa kanalı) hiçbir koşulda bozulmaz.** Rozetler, armalar, logolar ve şeffaf tül dokular için kenar taşması (dilation) ve matematiksel tolerans doğrulaması uygulanır.

### Başlıca Özellikler
* 🛡 **Zorunlu Alfa Koruma Akışı**: Premultiply $\rightarrow$ Lanczos3 / Bicubic küçültme $\rightarrow$ Un-premultiply $\rightarrow$ Dilation (şeffaf piksel kenar doldurma) $\rightarrow$ texconv DXT5 sıkıştırma $\rightarrow$ Çift doğrulama (ortalama fark <%2, alan farkı <%3).
* ⚡ **Çoklu Klasör ve Paralel İşlem**: Birden fazla klasörü aynı anda ekleyebilir veya sürükleyip bırakabilirsiniz. Çok çekirdekli işlemcilerde paralel çalışır.
* 🔗 **YDD ve YTD Otomatik Eşleştirme**: `accs_001_u` ile `accs_diff_001_a_uni` gibi dosyaları isim kuralına göre otomatik bağlar.
* 🔍 **Damalı (Checkerboard) Yan Yana Önizleme**: Her dokunun eski ve yeni halini damalı şeffaflık zemininde doğrulama raporuyla birlikte inceler.
* 🌐 **3 Dil Desteği**: Türkçe (TR), İngilizce (EN) ve Almanca (DE) arasında anında geçiş.
* 🔒 **Güvenli .bak Yedeği**: Orijinal dosyaları yedekler, kayıt sonrası YTD'nin açılabilirliğini teyit eder.

---

## 🚀 Building from Source

### Requirements
* Windows 10 / 11 (x64)
* Visual Studio 2022 (Community or higher) with `.NET desktop development` workload
* .NET Framework 4.8 Developer Pack
* MSBuild 17+

### Steps
1. Clone the repository:
   ```bash
   git clone https://github.com/sertoaluns/Ytd-Optimizer.git
   cd Ytd-Optimizer
   ```
2. Build with MSBuild or open `YtdOptimizer.sln` in Visual Studio:
   ```powershell
   & "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe" YtdOptimizer.sln /p:Configuration=Release /p:Platform=x64
   ```
3. The executable will be compiled to `bin\Release\YtdOptimizer.exe`.

---

## 🛠 Dependencies & Credits
* [dexyfex / CodeWalker](https://github.com/dexyfex/CodeWalker) - GTA V RPF, YTD, and DDSIO resource handling.
* [Microsoft DirectXTex (texconv)](https://github.com/microsoft/DirectXTex) - High performance DirectX texture processing & DXT5/BC3 compressor.
* [SixLabors.ImageSharp](https://github.com/SixLabors/ImageSharp) - Bicubic and Lanczos image scaling and pixel operations.

---

## 📄 License
This project is licensed under the [MIT License](LICENSE).

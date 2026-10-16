using System;
using System.Collections.Generic;

namespace YtdOptimizer.Core
{
    public static class Loc
    {
        public static string CurrentLanguage { get; private set; } = "tr";

        public static event Action LanguageChanged;

        private static readonly Dictionary<string, Dictionary<string, string>> Strings = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["tr"] = new Dictionary<string, string>
            {
                ["AppTitle"] = "GTA5 YTD Toplu Texture Optimizer (Şeffaflık Korumalı DXT5)",
                ["FoldersLabel"] = "Klasörler:",
                ["AddFolder"] = "➕ Klasör Ekle...",
                ["RemoveFolder"] = "➖ Seçileni Çıkar",
                ["ClearFolders"] = "🗑 Temizle",
                ["Analyze"] = "🔍 Analiz Et",
                ["OptimizeSelected"] = "⚡ Seçilileri Optimize Et",
                ["OptimizeAll"] = "🚀 Hepsini Optimize Et",
                ["Settings"] = "⚙ Ayarlar",
                ["Language"] = "Dil / Language:",

                ["SummaryTotalTextures"] = "Toplam Doku: {0}",
                ["SummaryTexturesToChange"] = "Değişecek Doku: {0}",
                ["SummaryTotalSize"] = "Toplam Boyut: {0:F2} MB",
                ["SummaryEstimatedSavings"] = "Tahmini Kazanç: ~{0:F2} MB",

                ["FilterLabel"] = "Filtrele:",
                ["ShowingCounter"] = "Gösterilen: {0} / {1}",
                ["Unmatched"] = "(Eşleşmedi)",

                ["ColDrawableFile"] = "Drawable / Dosya",
                ["ColYtdFile"] = "YTD Dosyası",
                ["ColFolder"] = "Klasör",
                ["ColSize"] = "Boyut",

                ["ColTexName"] = "Doku Adı",
                ["ColTexFormat"] = "Format",
                ["ColTexOldSize"] = "Eski Boyut",
                ["ColTexNewSize"] = "Yeni Boyut",
                ["ColTexFileSize"] = "Dosya Boyutu",
                ["ColTexAlpha"] = "Alfa/Opak",
                ["ColTexPreview"] = "Önizleme",
                ["ColTexStatus"] = "Durum",

                ["TagAlpha"] = "ALFA",
                ["TagOpaque"] = "OPAK",

                ["StatusPending"] = "Bekliyor",
                ["StatusOptimized"] = "Optimize Edildi",
                ["StatusSkipped"] = "Atlandı",
                ["StatusError"] = "Hata",

                ["MenuPreview"] = "🔍 Önizle (Yan Yana Checkerboard)",
                ["MenuReplace"] = "📁 Dosyayla Değiştir (PNG/DDS)...",
                ["MenuDelete"] = "✖ Dokuyu Sil",

                ["Ready"] = "Hazır",
                ["ScanningFolders"] = "Klasörler taranıyor ve analiz ediliyor...",
                ["LogScanning"] = "[ANALİZ] {0} klasör taranıyor (alt klasörler dahil)...",
                ["LogCompleted"] = "[ANALİZ] Tamamlandı: {0} YTD dosyası analiz edildi.",
                ["AnalysisCompleted"] = "Analiz tamamlandı. ({0} YTD, {1} YDD bulundu)",
                ["NoFolderWarning"] = "Lütfen en az bir klasör ekleyin veya klasörleri sürükleyip bırakın.",
                ["WarningTitle"] = "Uyarı",
                ["ConfirmTitle"] = "Onay",
                ["OptimizeConfirm"] = "Toplam {0} YTD ve {1} doku incelenecek.\n{2} doku hedef boyut ve DXT5 kuralına göre optimize edilecek.\nTahmini alan kazancı: ~{3:F2} MB.\n\nİşlemi başlatmak istiyor musunuz?",
                ["NoYtdToOptimize"] = "Optimize edilecek doku veya YTD bulunamadı.",
                ["ReportTitle"] = "Toplu Optimizasyon Raporu",
                ["ReportSummary"] = "Optimizasyon Tamamlandı!\n\nİşlenen YTD: {0}/{1}\nHatalı YTD: {2}\nDeğişen Doku: {3}\nAtlanan Doku: {4}\nHatalı Doku: {5}\n\nDoku Türleri:\n- Alfa Kanallı (Şeffaf): {6}\n- Opak: {7}\n\nAlfa Doğrulama Testi:\n- Başarıyla Geçen: {8}\n- Bozulma Riskiyle Korunan: {9}\n\nBoyut Özeti:\n- Eski Boyut: {10:F2} MB\n- Yeni Boyut: {11:F2} MB\n- Kazanılan Alan: {12:F2} MB",
                ["YtdVerifiedSuccess"] = "[BAŞARILI] {0} optimize edildi ve doğrulandı.",
                ["YtdVerificationFailed"] = "[HATA] {0} doğrulanamadı ve .bak yedeğinden geri yüklendi.",

                // Preview Form
                ["PreviewTitle"] = "Doku Önizleme: {0}",
                ["PreviewOriginal"] = "Orijinal Hali: {0}",
                ["PreviewStatusAlpha"] = "Durum: Alfa Kanallı (Şeffaf piksel içeriyor)",
                ["PreviewStatusOpaque"] = "Durum: Opak (Alfa Kanalı Tamamen 255)",
                ["PreviewOptimized"] = "Optimize Hali (Hedef): {0}",
                ["PreviewValPassed"] = "✓ Şeffaflık Doğrulaması BAŞARILI (Ort. Alfa Farkı: %{0:F2}, Alan Farkı: %{1:F2})",
                ["PreviewValWarning"] = "⚠ UYARI: Şeffaflık bozulmuş olabilir! (Ort. Alfa Farkı: %{0:F2}, Alan Farkı: %{1:F2})",
                ["Close"] = "Kapat",

                // Settings Form
                ["SettingsTitle"] = "Ayarlar - GTA5 YTD Optimizer",
                ["TabGeneral"] = "Genel ve Yollar",
                ["TabSizes"] = "Boyut Kuralları",
                ["TabAlpha"] = "Şeffaflık & Doğrulama",
                ["Gta5Folder"] = "GTA 5 Kurulu Klasörü:",
                ["CodeWalkerFolder"] = "CodeWalker Klasörü (CodeWalker.Core.dll):",
                ["TexconvPath"] = "texconv.exe Dosya Yolu:",
                ["Browse"] = "Gözat...",
                ["ParallelThreads"] = "Paralel İşlem Sayısı (Thread):",
                ["BackupOriginal"] = "Orijinal YTD'yi değiştirmeden önce .bak olarak yedekle",
                ["SkipNormal"] = "Normal map (_n) dokularını koru/atla",
                ["SkipSpecular"] = "Specular (_s) dokularını koru/atla",
                ["ConvertIfNotDxt5"] = "Boyutu uygun olsa bile format DXT5 değilse dönüştür",
                ["SquareRule"] = "Kare dokular için hedef üst sınır (varsayılan 1024):",
                ["WideRule"] = "2:1 Yatay dokular için hedef (Genişlik x Yükseklik):",
                ["TallRule"] = "1:2 Dikey dokular için hedef (Genişlik x Yükseklik):",
                ["GenericRule"] = "Diğer oranlar için en uzun kenar üst sınırı:",
                ["AlphaDiffThreshold"] = "Maksimum İzin Verilen Ortalama Alfa Farkı (%):",
                ["TransparentRatioThreshold"] = "Maksimum İzin Verilen Şeffaf Alan Değişimi (%):",
                ["AlphaInfoText"] = "Şeffaflık Koruma Kuralları (Zorunlu Akış):\n1. RGB renkleri alfa ile çarpılır (premultiply).\n2. Lanczos / bicubic yüksek kaliteli filtre ile küçültülür.\n3. Sonuç tekrar alfaya bölünür (un-premultiply).\n4. Tam şeffaf kenarlara renk taşması (dilation) uygulanır.\n5. texconv ile yalnızca DXT5'e sıkıştırılır.\n6. Kaynak alfa ile sonuç alfa karşılaştırılır; toleransı aşarsa orijinal korunur.",
                ["Save"] = "Kaydet",
                ["Cancel"] = "İptal",
                ["ResetDefaults"] = "Varsayılanlara Sıfırla"
            },
            ["en"] = new Dictionary<string, string>
            {
                ["AppTitle"] = "GTA5 YTD Batch Texture Optimizer (Alpha-Preserving DXT5)",
                ["FoldersLabel"] = "Folders:",
                ["AddFolder"] = "➕ Add Folder...",
                ["RemoveFolder"] = "➖ Remove Selected",
                ["ClearFolders"] = "🗑 Clear",
                ["Analyze"] = "🔍 Analyze",
                ["OptimizeSelected"] = "⚡ Optimize Selected",
                ["OptimizeAll"] = "🚀 Optimize All",
                ["Settings"] = "⚙ Settings",
                ["Language"] = "Language:",

                ["SummaryTotalTextures"] = "Total Textures: {0}",
                ["SummaryTexturesToChange"] = "Textures to Change: {0}",
                ["SummaryTotalSize"] = "Total Size: {0:F2} MB",
                ["SummaryEstimatedSavings"] = "Estimated Savings: ~{0:F2} MB",

                ["FilterLabel"] = "Filter:",
                ["ShowingCounter"] = "Showing: {0} / {1}",
                ["Unmatched"] = "(Unmatched)",

                ["ColDrawableFile"] = "Drawable / File",
                ["ColYtdFile"] = "YTD File",
                ["ColFolder"] = "Folder",
                ["ColSize"] = "Size",

                ["ColTexName"] = "Texture Name",
                ["ColTexFormat"] = "Format",
                ["ColTexOldSize"] = "Original Size",
                ["ColTexNewSize"] = "Target Size",
                ["ColTexFileSize"] = "File Size",
                ["ColTexAlpha"] = "Alpha/Opaque",
                ["ColTexPreview"] = "Preview",
                ["ColTexStatus"] = "Status",

                ["TagAlpha"] = "ALPHA",
                ["TagOpaque"] = "OPAQUE",

                ["StatusPending"] = "Pending",
                ["StatusOptimized"] = "Optimized",
                ["StatusSkipped"] = "Skipped",
                ["StatusError"] = "Error",

                ["MenuPreview"] = "🔍 Preview (Side-by-Side Checkerboard)",
                ["MenuReplace"] = "📁 Replace with File (PNG/DDS)...",
                ["MenuDelete"] = "✖ Delete Texture",

                ["Ready"] = "Ready",
                ["ScanningFolders"] = "Scanning and analyzing folders...",
                ["LogScanning"] = "[ANALYSIS] Scanning {0} folder(s) (including subdirectories)...",
                ["LogCompleted"] = "[ANALYSIS] Completed: {0} YTD files analyzed.",
                ["AnalysisCompleted"] = "Analysis complete. ({0} YTDs, {1} YDDs found)",
                ["NoFolderWarning"] = "Please add at least one folder or drag and drop folders.",
                ["WarningTitle"] = "Warning",
                ["ConfirmTitle"] = "Confirm",
                ["OptimizeConfirm"] = "{0} YTD files and {1} textures will be processed.\n{2} textures will be optimized to target size & DXT5.\nEstimated space savings: ~{3:F2} MB.\n\nDo you want to proceed?",
                ["NoYtdToOptimize"] = "No textures or YTD files found requiring optimization.",
                ["ReportTitle"] = "Batch Optimization Report",
                ["ReportSummary"] = "Optimization Finished!\n\nProcessed YTD: {0}/{1}\nFailed YTD: {2}\nModified Textures: {3}\nSkipped Textures: {4}\nFailed Textures: {5}\n\nTexture Types:\n- With Alpha Channel (Transparent): {6}\n- Opaque: {7}\n\nAlpha Validation Test:\n- Passed: {8}\n- Protected Due to Quality Risk: {9}\n\nSize Summary:\n- Original Size: {10:F2} MB\n- New Size: {11:F2} MB\n- Space Saved: {12:F2} MB",
                ["YtdVerifiedSuccess"] = "[SUCCESS] {0} optimized and verified.",
                ["YtdVerificationFailed"] = "[ERROR] {0} failed verification and was restored from .bak backup.",

                // Preview Form
                ["PreviewTitle"] = "Texture Preview: {0}",
                ["PreviewOriginal"] = "Original: {0}",
                ["PreviewStatusAlpha"] = "Status: Alpha Channel Present (Contains transparency)",
                ["PreviewStatusOpaque"] = "Status: Opaque (Alpha Channel Completely 255)",
                ["PreviewOptimized"] = "Optimized (Target): {0}",
                ["PreviewValPassed"] = "✓ Transparency Validation PASSED (Avg. Alpha Diff: %{0:F2}, Area Diff: %{1:F2})",
                ["PreviewValWarning"] = "⚠ WARNING: Transparency might be degraded! (Avg. Alpha Diff: %{0:F2}, Area Diff: %{1:F2})",
                ["Close"] = "Close",

                // Settings Form
                ["SettingsTitle"] = "Settings - GTA5 YTD Optimizer",
                ["TabGeneral"] = "General & Paths",
                ["TabSizes"] = "Size Rules",
                ["TabAlpha"] = "Transparency & Validation",
                ["Gta5Folder"] = "GTA 5 Installation Folder:",
                ["CodeWalkerFolder"] = "CodeWalker Folder (CodeWalker.Core.dll):",
                ["TexconvPath"] = "texconv.exe File Path:",
                ["Browse"] = "Browse...",
                ["ParallelThreads"] = "Parallel Threads:",
                ["BackupOriginal"] = "Backup original YTD as .bak before modifying",
                ["SkipNormal"] = "Protect/skip normal maps (_n)",
                ["SkipSpecular"] = "Protect/skip specular maps (_s)",
                ["ConvertIfNotDxt5"] = "Convert to DXT5 even if size already fits",
                ["SquareRule"] = "Target max limit for square textures (default 1024):",
                ["WideRule"] = "Target for 2:1 Horizontal textures (Width x Height):",
                ["TallRule"] = "Target for 1:2 Vertical textures (Width x Height):",
                ["GenericRule"] = "Target max long-edge for other aspect ratios:",
                ["AlphaDiffThreshold"] = "Max Allowed Average Alpha Difference (%):",
                ["TransparentRatioThreshold"] = "Max Allowed Transparent Area Change (%):",
                ["AlphaInfoText"] = "Alpha Preservation Pipeline (Mandatory Workflow):\n1. RGB colors multiplied by alpha (premultiply).\n2. High quality Lanczos / bicubic downscale.\n3. Result un-premultiplied back from alpha.\n4. Edge dilation applied to fully transparent pixels.\n5. Compressed strictly to DXT5 using texconv.\n6. Source vs target alpha compared; original preserved if threshold exceeded.",
                ["Save"] = "Save",
                ["Cancel"] = "Cancel",
                ["ResetDefaults"] = "Reset to Defaults"
            },
            ["de"] = new Dictionary<string, string>
            {
                ["AppTitle"] = "GTA5 YTD Stapel-Texturoptimierer (Alpha-erhaltendes DXT5)",
                ["FoldersLabel"] = "Ordner:",
                ["AddFolder"] = "➕ Ordner hinzufügen...",
                ["RemoveFolder"] = "➖ Ausgewählte entfernen",
                ["ClearFolders"] = "🗑 Leeren",
                ["Analyze"] = "🔍 Analysieren",
                ["OptimizeSelected"] = "⚡ Ausgewählte optimieren",
                ["OptimizeAll"] = "🚀 Alle optimieren",
                ["Settings"] = "⚙ Einstellungen",
                ["Language"] = "Sprache / Language:",

                ["SummaryTotalTextures"] = "Gesamttexturen: {0}",
                ["SummaryTexturesToChange"] = "Zu ändernde Texturen: {0}",
                ["SummaryTotalSize"] = "Gesamtgröße: {0:F2} MB",
                ["SummaryEstimatedSavings"] = "Geschätzte Ersparnis: ~{0:F2} MB",

                ["FilterLabel"] = "Filtern:",
                ["ShowingCounter"] = "Angezeigt: {0} / {1}",
                ["Unmatched"] = "(Nicht zugeordnet)",

                ["ColDrawableFile"] = "Drawable / Datei",
                ["ColYtdFile"] = "YTD-Datei",
                ["ColFolder"] = "Ordner",
                ["ColSize"] = "Größe",

                ["ColTexName"] = "Texturname",
                ["ColTexFormat"] = "Format",
                ["ColTexOldSize"] = "Ursprungsgröße",
                ["ColTexNewSize"] = "Zielgröße",
                ["ColTexFileSize"] = "Dateigröße",
                ["ColTexAlpha"] = "Alpha/Deckend",
                ["ColTexPreview"] = "Vorschau",
                ["ColTexStatus"] = "Status",

                ["TagAlpha"] = "ALPHA",
                ["TagOpaque"] = "DECKEND",

                ["StatusPending"] = "Wartend",
                ["StatusOptimized"] = "Optimiert",
                ["StatusSkipped"] = "Übersprungen",
                ["StatusError"] = "Fehler",

                ["MenuPreview"] = "🔍 Vorschau (Nebeneinander Schachbrett)",
                ["MenuReplace"] = "📁 Durch Datei ersetzen (PNG/DDS)...",
                ["MenuDelete"] = "✖ Textur löschen",

                ["Ready"] = "Bereit",
                ["ScanningFolders"] = "Ordner werden gescannt und analysiert...",
                ["LogScanning"] = "[ANALYSE] {0} Ordner wird gescannt (inklusive Unterordner)...",
                ["LogCompleted"] = "[ANALYSE] Abgeschlossen: {0} YTD-Dateien analysiert.",
                ["AnalysisCompleted"] = "Analyse abgeschlossen. ({0} YTDs, {1} YDDs gefunden)",
                ["NoFolderWarning"] = "Bitte fügen Sie mindestens einen Ordner hinzu oder ziehen Sie Ordner per Drag & Drop hinein.",
                ["WarningTitle"] = "Warnung",
                ["ConfirmTitle"] = "Bestätigung",
                ["OptimizeConfirm"] = "{0} YTD-Dateien und {1} Texturen werden verarbeitet.\n{2} Texturen werden gemäß Zielgröße und DXT5 optimiert.\nGeschätzte Speicherplatzersparnis: ~{3:F2} MB.\n\nMöchten Sie fortfahren?",
                ["NoYtdToOptimize"] = "Keine Texturen oder YTD-Dateien zur Optimierung gefunden.",
                ["ReportTitle"] = "Stapel-Optimierungsbericht",
                ["ReportSummary"] = "Optimierung abgeschlossen!\n\nVerarbeitete YTD: {0}/{1}\nFehlerhafte YTD: {2}\nGeänderte Texturen: {3}\nÜbersprungene Texturen: {4}\nFehlerhafte Texturen: {5}\n\nTexturarten:\n- Mit Alpha-Kanal (Transparent): {6}\n- Deckend: {7}\n\nAlpha-Validierungstest:\n- Erfolgreich: {8}\n- Aus Sicherheitsgründen beibehalten: {9}\n\nGrößenübersicht:\n- Ursprüngliche Größe: {10:F2} MB\n- Neue Größe: {11:F2} MB\n- Gesparter Speicherplatz: {12:F2} MB",
                ["YtdVerifiedSuccess"] = "[ERFOLG] {0} optimiert und verifiziert.",
                ["YtdVerificationFailed"] = "[FEHLER] {0} konnte nicht verifiziert werden und wurde aus dem .bak-Backup wiederhergestellt.",

                // Preview Form
                ["PreviewTitle"] = "Texturvorschau: {0}",
                ["PreviewOriginal"] = "Original: {0}",
                ["PreviewStatusAlpha"] = "Status: Alpha-Kanal vorhanden (Enthält Transparenz)",
                ["PreviewStatusOpaque"] = "Status: Deckend (Alpha-Kanal vollständig 255)",
                ["PreviewOptimized"] = "Optimiert (Ziel): {0}",
                ["PreviewValPassed"] = "✓ Transparenz-Validierung ERFOLGREICH (Durchschn. Alpha-Diff: %{0:F2}, Flächen-Diff: %{1:F2})",
                ["PreviewValWarning"] = "⚠ WARNUNG: Transparenz möglicherweise beeinträchtigt! (Durchschn. Alpha-Diff: %{0:F2}, Flächen-Diff: %{1:F2})",
                ["Close"] = "Schließen",

                // Settings Form
                ["SettingsTitle"] = "Einstellungen - GTA5 YTD Optimizer",
                ["TabGeneral"] = "Allgemein & Pfade",
                ["TabSizes"] = "Größenregeln",
                ["TabAlpha"] = "Transparenz & Validierung",
                ["Gta5Folder"] = "GTA 5 Installationsordner:",
                ["CodeWalkerFolder"] = "CodeWalker-Ordner (CodeWalker.Core.dll):",
                ["TexconvPath"] = "texconv.exe Dateipfad:",
                ["Browse"] = "Durchsuchen...",
                ["ParallelThreads"] = "Parallele Threads:",
                ["BackupOriginal"] = "Originale YTD vor Änderung als .bak sichern",
                ["SkipNormal"] = "Normal-Maps (_n) schützen/überspringen",
                ["SkipSpecular"] = "Specular-Maps (_s) schützen/überspringen",
                ["ConvertIfNotDxt5"] = "In DXT5 konvertieren falls noch nicht im Format",
                ["SquareRule"] = "Zielobergrenze für quadratische Texturen (Standard 1024):",
                ["WideRule"] = "Ziel für 2:1 horizontale Texturen (Breite x Höhe):",
                ["TallRule"] = "Ziel für 1:2 vertikale Texturen (Breite x Höhe):",
                ["GenericRule"] = "Maximale Kantenlänge für andere Seitenverhältnisse:",
                ["AlphaDiffThreshold"] = "Maximal erlaubte durchschnittliche Alpha-Differenz (%):",
                ["TransparentRatioThreshold"] = "Maximal erlaubte Änderung des transparenten Bereichs (%):",
                ["AlphaInfoText"] = "Alpha-Erhaltungs-Pipeline (Obligatorischer Ablauf):\n1. RGB-Farben werden mit Alpha multipliziert (premultiply).\n2. Hochwertige Lanczos-/Bicubic-Skalierung.\n3. Ergebnis wird wieder durch Alpha dividiert (un-premultiply).\n4. Farbkanten-Erweiterung (Dilation) für volltransparente Pixel.\n5. Strikt als DXT5 mit texconv komprimiert.\n6. Quell- und Ziel-Alpha werden verglichen; bei Überschreitung wird das Original beibehalten.",
                ["Save"] = "Speichern",
                ["Cancel"] = "Abbrechen",
                ["ResetDefaults"] = "Auf Standard zurücksetzen"
            }
        };

        public static void SetLanguage(string lang)
        {
            if (string.IsNullOrWhiteSpace(lang)) lang = "tr";
            lang = lang.ToLowerInvariant();
            if (!Strings.ContainsKey(lang)) lang = "tr";
            if (CurrentLanguage != lang)
            {
                CurrentLanguage = lang;
                LanguageChanged?.Invoke();
            }
        }

        public static string T(string key)
        {
            if (Strings.TryGetValue(CurrentLanguage, out var dict) && dict.TryGetValue(key, out var val))
            {
                return val;
            }
            if (Strings["en"].TryGetValue(key, out var enVal))
            {
                return enVal;
            }
            return key;
        }

        public static string T(string key, params object[] args)
        {
            string format = T(key);
            try
            {
                return string.Format(format, args);
            }
            catch
            {
                return format;
            }
        }
    }
}

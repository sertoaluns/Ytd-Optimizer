using System;
using System.IO;
using CodeWalker.GameFiles;
using CodeWalker.Utils;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace YtdOptimizer.Core
{
    public static class CodeWalkerInterop
    {
        /// <summary>
        /// Reads a YtdFile from file on disk.
        /// GTA V resource files start with 'RSC7' header and are compressed/uncompressed.
        /// CodeWalker's RpfFile.GetResourceFile<YtdFile>(bytes) handles header, decompression, and structure parsing.
        /// </summary>
        public static YtdFile LoadYtd(string ytdFilePath)
        {
            if (!File.Exists(ytdFilePath))
                throw new FileNotFoundException("YTD dosyası bulunamadı: " + ytdFilePath);

            byte[] data = File.ReadAllBytes(ytdFilePath);
            return RpfFile.GetResourceFile<YtdFile>(data);
        }

        /// <summary>
        /// Saves a YtdFile to byte array using YtdFile.Save().
        /// CodeWalker's YtdFile.Save() calls ResourceBuilder.Build() to output the complete compressed RSC7 file bytes.
        /// </summary>
        public static byte[] SaveYtdToBytes(YtdFile ytd)
        {
            if (ytd == null) throw new ArgumentNullException(nameof(ytd));
            return ytd.Save();
        }

        /// <summary>
        /// Saves YtdFile to disk, optionally creating a .bak backup first.
        /// </summary>
        public static void SaveYtd(YtdFile ytd, string ytdFilePath, bool createBackup = true)
        {
            if (createBackup && File.Exists(ytdFilePath))
            {
                string bakPath = ytdFilePath + ".bak";
                if (!File.Exists(bakPath))
                {
                    File.Copy(ytdFilePath, bakPath, true);
                }
            }

            byte[] data = SaveYtdToBytes(ytd);
            string tempTarget = ytdFilePath + ".tmp_" + Guid.NewGuid().ToString("N");
            File.WriteAllBytes(tempTarget, data);

            if (File.Exists(ytdFilePath))
            {
                File.Delete(ytdFilePath);
            }
            File.Move(tempTarget, ytdFilePath);
        }

        /// <summary>
        /// Extracts texture from CodeWalker Texture to DDS file bytes.
        /// </summary>
        public static byte[] ExtractDdsBytes(Texture texture)
        {
            if (texture == null) throw new ArgumentNullException(nameof(texture));
            return DDSIO.GetDDSFile(texture);
        }

        /// <summary>
        /// Creates a CodeWalker Texture from DDS file bytes.
        /// </summary>
        public static Texture CreateTextureFromDds(byte[] ddsBytes, string textureName)
        {
            if (ddsBytes == null || ddsBytes.Length == 0)
                throw new ArgumentException("DDS verisi boş olamaz.", nameof(ddsBytes));

            Texture tex = DDSIO.GetTexture(ddsBytes);
            if (tex != null && !string.IsNullOrEmpty(textureName))
            {
                tex.Name = textureName;
            }
            return tex;
        }

        /// <summary>
        /// Extracts Texture as an ImageSharp Image<Rgba32>.
        /// First attempts CodeWalker's native DDSIO.GetPixels(texture, 0) which decodes all GTA5 formats (DXT1, DXT5, BC7, A8R8G8B8, etc.).
        /// If that fails, falls back to DDS export and texconv decompression.
        /// </summary>
        public static Image<Rgba32> ExtractTextureToImage(Texture texture, string texconvPath, string tempDir)
        {
            if (texture == null) throw new ArgumentNullException(nameof(texture));

            byte[] pixels = null;
            try
            {
                pixels = DDSIO.GetPixels(texture, 0);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"DDSIO.GetPixels({texture.Name}, {texture.Width}x{texture.Height}, {texture.Format}) başarısız: {ex.Message}", ex);
            }

            if (pixels != null && pixels.Length >= texture.Width * texture.Height * 4)
            {
                return Image.LoadPixelData<Rgba32>(pixels, texture.Width, texture.Height);
            }

            throw new InvalidOperationException($"DDSIO.GetPixels beklenmeyen piksel boyutu döndürdü: {(pixels == null ? "null" : pixels.Length.ToString())} (Beklenen: {texture.Width * texture.Height * 4})");
        }
    }
}

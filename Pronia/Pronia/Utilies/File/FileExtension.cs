using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Pronia.Utilies.File
{
    public static class FileExtension
    {
        public static bool CheckFileFormat(this IFormFile file, string format)
        {
            if (file.ContentType.Contains(format)) return true;
            return false;
        }
        public static bool CheckFileSize(this IFormFile file, int mb)
        {
            if (file.Length / 1024 / 1024 <= mb) return true;
            return false;
        }
        public static async Task<string> SaveFileAsync(this IFormFile file, string loc)
        {
            string fileName = Guid.NewGuid().ToString() + file.FileName;
            string path = Path.Combine(loc, fileName);
            FileStream fileStream = new FileStream(path, FileMode.Create);
            await file.CopyToAsync(fileStream);
            return fileName;
        }
        public static void DeleteFile(string ImgPath)
        {
            if (System.IO.File.Exists(ImgPath))
            {
                System.IO.File.Delete(ImgPath);
            }
        }
        public static string PhotoIsOk(this List<IFormFile> files)
        {
            foreach (var file in files)
            {
                if (!file.CheckFileFormat("image/"))
                {
                    return "Image formatinda deyil";
                }
                if (!file.CheckFileSize(1))
                {
                    return "Secilen File 1mb cox olmamalidir";
                }
            }
            return "";
        }
    }
}

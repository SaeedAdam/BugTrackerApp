using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace TheBlogProject.Services
{
    public interface IBTImageService
    {
        Task<byte[]> EncodeImageAsync(IFormFile file);
        Task<byte[]> EncodeImageAsync(string fileName);
        string DecodeImage(byte[] data, string type);
        string GetContentType(IFormFile file);
        int GetImageSize(IFormFile file);
    }
}

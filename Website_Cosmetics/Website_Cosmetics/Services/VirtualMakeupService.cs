using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Website_Cosmetics.Services
{
    /// <summary>
    /// Service để gọi PSGAN Docker API cho virtual makeup
    /// </summary>
    public class VirtualMakeupService
    {
        private readonly HttpClient _httpClient;
        private readonly string _psganApiUrl;

        public VirtualMakeupService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(30); // API có thể mất 5-10s
            
            // Lấy URL từ appsettings.json
            _psganApiUrl = configuration["PsganApi:Url"] ?? "http://localhost:5000";
        }

        /// <summary>
        /// Kiểm tra PSGAN API có hoạt động không
        /// </summary>
        public async Task<bool> IsApiHealthyAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_psganApiUrl}/api/health");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Transfer makeup - Nhận file upload và binary image data
        /// </summary>
        public async Task<byte[]> ApplyVirtualMakeupAsync(
            IFormFile customerPhoto,
            byte[] makeupStyleImageData)
        {
            using var form = new MultipartFormDataContent();

            // 1. Thêm ảnh khách hàng
            using var customerStream = customerPhoto.OpenReadStream();
            var customerContent = new StreamContent(customerStream);
            customerContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(
                customerPhoto.ContentType ?? "image/jpeg");
            form.Add(customerContent, "customer_photo", customerPhoto.FileName);

            // 2. Thêm ảnh mẫu makeup (makeup_reference theo Flask API mới)
            var makeupContent = new ByteArrayContent(makeupStyleImageData);
            makeupContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
            form.Add(makeupContent, "makeup_reference", "makeup_reference.jpg");

            // 3. Gọi PSGAN Flask API endpoint mới
            var response = await _httpClient.PostAsync($"{_psganApiUrl}/api/makeup/transfer", form);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsByteArrayAsync();
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"PSGAN API error: {error}");
            }
        }
    }
}


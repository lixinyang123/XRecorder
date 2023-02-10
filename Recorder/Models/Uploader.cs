using Microsoft.Net.Http.Headers;
using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Transactions;

namespace Recorder.Models
{
    public class Uploader
    {
        private readonly AppDataContext appDataContext;

        public Uploader()
        {
            appDataContext = App.Current?.DataContext as AppDataContext ?? throw new NullReferenceException();
        }

        /// <summary>
        /// 生成请求表单
        /// </summary>
        /// <param name="fileInfo">文件信息</param>
        /// <returns>FormContent</returns>
        private MultipartFormDataContent GenFormContent(FileInfo fileInfo)
        {
            MultipartFormDataContent httpContent = new()
            {
                // 传输代码
                { new StringContent(appDataContext.TransactionCode), "transactionCode"},
                // 文件格式
                {
                    new StringContent(fileInfo.Extension switch
                    {
                        ".png" => "1",
                        ".mp4" => "4",
                        _ => "0"
                    }),
                    "proofType"
                },
                // 文件名
                { new StringContent(fileInfo.Name), "proofName" },
                // 网页地址
                { new StringContent(string.Empty), "proofAdress" },
                // 取证时间
                { new StringContent("2023-2-8 13:05:00"), "obtainTime" },
                // 开始采集时间
                { new StringContent("2023-2-8 13:05:00"), "obtainEvidenecStart" },
                // 结束采集时间
                { new StringContent("2023-2-8 13:05:00"), "obtainEvidenecEnd" },
                // 文件
                { new ByteArrayContent(File.ReadAllBytes(fileInfo.FullName)), "file", fileInfo.Name }
            };

            return httpContent;
        }

        /// <summary>
        /// 单个文件上传
        /// </summary>
        /// <param name="file">文件路径</param>
        /// <returns>是否上传成功</returns>
        public async Task<bool> Upload(string file)
        {
            FileInfo fileInfo = new(file);

            // 发送请求
            using HttpRequestMessage requestMessage = new()
            {
                RequestUri = new Uri(appDataContext.UploadUrl),
                Method = HttpMethod.Post,
                Content = GenFormContent(fileInfo)
            };

            using HttpClient httpClient = new();
            httpClient.DefaultRequestHeaders.Add("Authorization", appDataContext.ApiToken);

            using HttpResponseMessage responseMessage = await httpClient.SendAsync(requestMessage);
            string resultStr = await responseMessage.Content.ReadAsStringAsync();

            // 解析上传结果
            UploadResult? result = JsonSerializer.Deserialize<UploadResult>(resultStr);

            if (result?.Code == 200)
                File.Delete(fileInfo.FullName);

            return result?.Code == 200;
        }

        /// <summary>
        /// 报告上传成功文件数量
        /// </summary>
        /// <returns>是否报告成功</returns>
        public async Task<bool> Report(int uploaded)
        {
            HttpContent httpContent = new StringContent(JsonSerializer.Serialize(new
            {
                proofCount = uploaded,
                transactionCode = appDataContext.TransactionCode
            }));

            httpContent.Headers.ContentType = new("application/json");

            HttpClient httpClient = new();
            httpClient.DefaultRequestHeaders.Add("Authorization", appDataContext.ApiToken);
            HttpResponseMessage responseMessage = await httpClient.PostAsync(appDataContext.ReportUrl, httpContent);

            string resultStr = await responseMessage.Content.ReadAsStringAsync();
            UploadResult? result = JsonSerializer.Deserialize<UploadResult>(resultStr);

            return result?.Code == 200;
        }
    }
}
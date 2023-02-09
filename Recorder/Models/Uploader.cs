using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Recorder.Models
{
    public class Uploader
    {
        private readonly AppDataContext appDataContext;

        private readonly string savePath = string.Empty;

        public Uploader(string savePath)
        {
            appDataContext = App.Current?.DataContext as AppDataContext ?? throw new NullReferenceException();
            this.savePath = savePath;
        }

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

        public List<Task> Upload()
        {
            List<Task> tasks = new();

            // Upload files
            Directory.GetFiles(savePath).ToList().ForEach(file =>
            {
                tasks.Add(Task.Run(async () =>
                {
                    FileInfo fileInfo = new(file);

                    // Send the request
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

                    // Parse response result
                    UploadResult result = JsonSerializer.Deserialize<UploadResult>(resultStr)
                        ?? new UploadResult() { Msg = string.Empty, Code = 404 };

                    if (result.Code != 200)
                        return;

                    File.Delete(fileInfo.FullName);
                }));
            });

            return tasks;
        }
    }
}
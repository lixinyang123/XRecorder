using Avalonia.Controls.ApplicationLifetimes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Handlers;
using System.Text.Json;
using System.Threading.Tasks;

namespace Recorder.Models
{
    public class Uploader
    {
        private readonly IClassicDesktopStyleApplicationLifetime applicationLifetime;

        private readonly string savePath = string.Empty;

        private readonly Dictionary<string, HttpProgressEventArgs> fileUploadProgress;

        public event Action<long, long>? UploadProgressChanged;

        public Uploader(string savePath)
        {
            this.savePath = savePath;

            applicationLifetime = App.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime
                ?? throw new NullReferenceException();

            fileUploadProgress = new();
        }

        private MultipartFormDataContent GenFormContent(FileInfo fileInfo)
        {
            MultipartFormDataContent httpContent = new()
            {
                // 传输代码
                { new StringContent("1"), "transactionCode" },
                // 文件格式
                {
                    new StringContent(fileInfo.Extension switch
                    {
                        ".jpg" => "1",
                        ".mp4" => "4",
                        _ => "0"
                    }),
                    "proofType"
                },
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

        private void NotifyProgressChanged()
        {
            long totalBytes = 0;
            long bytesTransferred = 0;

            fileUploadProgress.Values.ToList().ForEach(progress =>
            {
                bytesTransferred += progress.BytesTransferred;
                totalBytes += progress.TotalBytes ?? 0;
            });

            UploadProgressChanged?.Invoke(totalBytes, bytesTransferred);
        }

        public void Upload()
        {
            // Check the upload url is not null or empty.
            string uploadUrl = applicationLifetime.Args?.FirstOrDefault() ?? "http://shinetechzz.tpddns.cn:31714/proof/webSaveProof";
            if (string.IsNullOrEmpty(uploadUrl))
                return;

            // Upload files
            Directory.GetFiles(savePath).ToList().ForEach(file =>
            {
                _ = Task.Run(async () =>
                {
                    FileInfo fileInfo = new(file);
                    MultipartFormDataContent content = GenFormContent(fileInfo);

                    // init the upload progress
                    HttpMessageHandler httpMessageHandler = new SocketsHttpHandler();
                    ProgressMessageHandler progressMessageHandler = new(httpMessageHandler);
                    progressMessageHandler.HttpSendProgress += (object? sender, HttpProgressEventArgs args) =>
                    {
                        fileUploadProgress.Remove(fileInfo.Name);
                        fileUploadProgress.Add(fileInfo.Name, args);

                        NotifyProgressChanged();
                    };

                    // Send the request
                    HttpRequestMessage requestMessage = new()
                    {
                        RequestUri = new Uri(uploadUrl),
                        Method = HttpMethod.Post,
                        Content = content
                    };

                    HttpClient httpClient = new(progressMessageHandler);
                    httpClient.DefaultRequestHeaders.Add("Authorization", "eyJhbGciOiJIUzUxMiJ9.eyJsb2dpbl91c2VyX2tleSI6IjUxZGI1YjliLTY5ZjYtNDM1MC05NjMyLTQ3YTlhYjZlNjgwNyJ9.unH89V_qtGv48YggoY4lhIa7UDs2b5h6C5HUbiZGiGk3VQnoQA_i7Jb-Wy91pKuXgGJtxIbycjfuUYxl9cXjug");

                    HttpResponseMessage responseMessage = await httpClient.SendAsync(requestMessage);
                    string resultStr = await responseMessage.Content.ReadAsStringAsync();

                    // Parse response result
                    UploadResult result = JsonSerializer.Deserialize<UploadResult>(resultStr)
                        ?? new UploadResult() { Msg = string.Empty, Code = 404 };

                    if (result.Code == 200)
                    {
                        File.Delete(fileInfo.FullName);
                        Console.WriteLine(resultStr);
                        Console.WriteLine($"{fileInfo.Name} 上传成功");
                    }
                });
            });
        }
    }
}
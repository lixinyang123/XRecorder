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
        private readonly AppDataContext appDataContext;

        private readonly string savePath = string.Empty;

        private readonly Dictionary<string, HttpProgressEventArgs> fileUploadProgress;

        private static readonly object locker = new();

        public event Action<long, long>? UploadProgressChanged;

        public Uploader(string savePath)
        {
            appDataContext = App.Current?.DataContext as AppDataContext ?? throw new NullReferenceException();
            this.savePath = savePath;
            fileUploadProgress = new();
        }

        private static MultipartFormDataContent GenFormContent(FileInfo fileInfo)
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
            // Upload files
            Directory.GetFiles(savePath).ToList().ForEach(file =>
            {
                _ = Task.Run(async () =>
                {
                    FileInfo fileInfo = new(file);

                    // init the upload progress
                    using ProgressMessageHandler progressMessageHandler = new(new SocketsHttpHandler());
                    progressMessageHandler.HttpSendProgress += (object? sender, HttpProgressEventArgs args) =>
                    {
                        lock (locker)
                        {
                            fileUploadProgress.Remove(fileInfo.Name);
                            fileUploadProgress.Add(fileInfo.Name, args);

                            NotifyProgressChanged();
                        }
                    };

                    // Send the request
                    using HttpRequestMessage requestMessage = new()
                    {
                        RequestUri = new Uri(appDataContext.UploadUrl),
                        Method = HttpMethod.Post,
                        Content = GenFormContent(fileInfo)
                    };

                    using HttpClient httpClient = new(progressMessageHandler);
                    httpClient.DefaultRequestHeaders.Add("Authorization", appDataContext.ApiToken);

                    using HttpResponseMessage responseMessage = await httpClient.SendAsync(requestMessage);
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
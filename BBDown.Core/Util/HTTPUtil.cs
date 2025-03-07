﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Cache;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using static BBDown.Core.Logger;

namespace BBDown.Core.Util
{
    public class HTTPUtil
    {
        public static readonly HttpClient AppHttpClient = new(new HttpClientHandler
        {
            AllowAutoRedirect = true,
            AutomaticDecompression = DecompressionMethods.All,
            ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
        })
        {
            Timeout = TimeSpan.FromMinutes(5)
        };

        public static async Task<string> GetWebSourceAsync(string url)
        {
            string htmlCode = string.Empty;
            using var webRequest = new HttpRequestMessage(HttpMethod.Get, url);
            webRequest.Headers.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/13.0 Safari/605.1.15");
            webRequest.Headers.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate");
            webRequest.Headers.TryAddWithoutValidation("Cookie", (url.Contains("/ep") || url.Contains("/ss")) ? Config.COOKIE + ";CURRENT_FNVAL=4048;" : Config.COOKIE);
            if (url.Contains("1735831202235667.cn-hongkong.fc.aliyuncs.com/2016-08-15/proxy/bililimit.LATEST/removelimit/pgc/player/web/playurl") || url.Contains("https://1735831202235667.cn-hongkong.fc.aliyuncs.com/2016-08-15/proxy/bililimit.LATEST/removelimit/pugv/player/web/playurl"))
                webRequest.Headers.TryAddWithoutValidation("Referer", "https://www.bilibili.com");
            webRequest.Headers.CacheControl = CacheControlHeaderValue.Parse("no-cache");
            webRequest.Headers.Connection.Clear();

            LogDebug("获取网页内容：Url: {0}, Headers: {1}", url, webRequest.Headers);
            var webResponse = (await AppHttpClient.SendAsync(webRequest, HttpCompletionOption.ResponseHeadersRead)).EnsureSuccessStatusCode();

            htmlCode = await webResponse.Content.ReadAsStringAsync();
            LogDebug("Response: {0}", htmlCode);
            return htmlCode;
        }

        public static async Task<string> GetPostResponseAsync(string Url, byte[] postData)
        {
            LogDebug("Post to: {0}, data: {1}", Url, Convert.ToBase64String(postData));
            string htmlCode = string.Empty;
            using HttpRequestMessage request = new(HttpMethod.Post, Url);
            request.Headers.TryAddWithoutValidation("Content-Type", "application/grpc");
            request.Headers.TryAddWithoutValidation("Content-Length", postData.Length.ToString());
            request.Headers.TryAddWithoutValidation("User-Agent", "Dalvik/2.1.0 (Linux; U; Android 6.0.1; oneplus a5010 Build/V417IR) 6.10.0 os/android model/oneplus a5010 mobi_app/android build/6100500 channel/bili innerVer/6100500 osVer/6.0.1 network/2");
            request.Headers.TryAddWithoutValidation("Cookie", Config.COOKIE);
            request.Content = new ByteArrayContent(postData);
            var webResponse = await AppHttpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            Stream myRequestStream = await webResponse.Content.ReadAsStreamAsync();
            htmlCode = await webResponse.Content.ReadAsStringAsync();
            return htmlCode;
        }
    }
}

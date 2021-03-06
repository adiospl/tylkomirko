﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Mirko.Gfycat
{
    public class ReplyJSON
    {
        public string gfyId { get; set; }
        public string gfyName { get; set; }
        public string gfyNumber { get; set; }
        public string userName { get; set; }
        public int width { get; set; }
        public int height { get; set; }
        public int frameRate { get; set; }
        public int numFrames { get; set; }
        public string mp4Url { get; set; }
        public string webmUrl { get; set; }
        public string gifUrl { get; set; }
        public int gifSize { get; set; }
        public int mp4Size { get; set; }
        public int webmSize { get; set; }
        public string createDate { get; set; }
        public string views { get; set; }
        public object title { get; set; }
        public List<string> extraLemmas { get; set; }
        public string md5 { get; set; }
        public object tags { get; set; }
        public string nsfw { get; set; }
        public object sar { get; set; }
        public string url { get; set; }
        public string source { get; set; }
        public object dynamo { get; set; }
        public string subreddit { get; set; }
        public string redditId { get; set; }
        public string redditIdText { get; set; }
        public object uploadGifName { get; set; }
        public string task { get; set; }
        public string gfyname { get; set; }
        public int gfysize { get; set; }
    }

    public class GfyItem
    {
        public string gfyId { get; set; }
        public string gfyName { get; set; }
        public string gfyNumber { get; set; }
        public string userName { get; set; }
        public int width { get; set; }
        public int height { get; set; }
        public int frameRate { get; set; }
        public int numFrames { get; set; }
        public string mp4Url { get; set; }
        public string webmUrl { get; set; }
        public string gifUrl { get; set; }
        public int gifSize { get; set; }
        public int mp4Size { get; set; }
        public int webmSize { get; set; }
        public string createDate { get; set; }
        public string views { get; set; }
        public object title { get; set; }
        public object extraLemmas { get; set; }
        public string md5 { get; set; }
        public object tags { get; set; }
        public string nsfw { get; set; }
        public object sar { get; set; }
        public string url { get; set; }
        public string source { get; set; }
        public object dynamo { get; set; }
        public string subreddit { get; set; }
        public string redditId { get; set; }
        public string redditIdText { get; set; }
        public object uploadGifName { get; set; }
    }

    public class GFYReply
    {
        public GfyItem gfyItem { get; set; }
    }

    public static class Gfycat
    {
        public static async Task<string> GIFtoMP4(string gifURL)
        {
            string URL = "http://upload.gfycat.com/transcode?fetchUrl=" + gifURL;

            try
            {
                using (var result = await App.ApiService.HttpClient.PostAsync(URL, null))
                using (var stream = await result.Content.ReadAsStreamAsync())
                using (StreamReader sr = new StreamReader(stream))
                using (JsonReader reader = new JsonTextReader(sr))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    var reply = serializer.Deserialize<ReplyJSON>(reader);
                    return reply.mp4Url;
                }
            }
            catch (Exception e)
            {
                App.TelemetryClient.TrackException(e);
                return null;
            }
        }

        public static async Task<string> GFYgetURL(string url)
        {
            var name = url.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries)[2];
            string URL = "http://gfycat.com/cajax/get/" + name;

            try
            {
                using (var result = await App.ApiService.HttpClient.PostAsync(URL, null))
                using (var stream = await result.Content.ReadAsStreamAsync())
                using (StreamReader sr = new StreamReader(stream))
                using (JsonReader reader = new JsonTextReader(sr))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    var reply = serializer.Deserialize<GFYReply>(reader);
                    return reply.gfyItem.mp4Url;
                }
            }
            catch (Exception e)
            {
                App.TelemetryClient.TrackException(e);
                return null;
            }
        }
    }
}

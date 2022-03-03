using System;
using System.Text.Json;
using System.Net;
using System.IO;
using System.Threading.Tasks;
using System.Text;
using System.Data;
using System.Collections.Generic;

namespace Scraper
{
    static class Scraper
    {
        static void Main()
        {
            APIHandler();
            Thread.Sleep(Timeout.Infinite);
        }
        static async void APIHandler()
        {
            Console.WriteLine("Please enter the user_id from which you would like to get the favorites");
            string WebsiteAdress = "https://e621.net/favorites.json?user_id=" + Console.ReadLine();
            HttpClient APIHandler = new HttpClient();
            HttpResponseMessage message = new HttpResponseMessage();
            APIHandler.DefaultRequestHeaders.Add("User-Agent", "favorite scraper by actinoide");
            try
            {
                message = await APIHandler.GetAsync(WebsiteAdress);
            }
            catch
            {
                Console.WriteLine("could not reach server");
                return;
            }
            if (!message.IsSuccessStatusCode)
            {
                Console.WriteLine("server reported error Error Code:" + message.StatusCode);
                return;
            }
            String content = await message.Content.ReadAsStringAsync();
            Data Data = JsonSerializer.Deserialize<Data>(content);
            if (Data == null) return;
            Console.WriteLine("please enter the folder path youd like the files to be saved to");
            string path = Console.ReadLine();
            if (!System.IO.Directory.Exists(path))
            {
                Console.WriteLine("invalid folder path");
                return;
            }
            foreach (Post thing in Data.posts)
            {
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage message1 = await client.GetAsync(thing.file.url);
                    HttpContent content1 = message1.Content;
                    Stream response = content1.ReadAsStream();
                    byte[] imagebytes;
                    using (BinaryReader br = new BinaryReader(response))
                    {
                        imagebytes = br.ReadBytes(((int)response.Length));
                    }
                    if (content1.Headers.ContentType.MediaType == "image/jpeg")
                    {
                        FileStream fs = new FileStream(path + thing.id + ".jpeg", FileMode.Create);
                        BinaryWriter bw = new BinaryWriter(fs);
                        bw.Write(imagebytes);
                    }
                    else if (content1.Headers.ContentType.MediaType == "image/png")
                    {
                        FileStream fs = new FileStream(path + thing.id + ".png", FileMode.Create);
                        BinaryWriter bw = new BinaryWriter(fs);
                        bw.Write(imagebytes);
                    }
                    else if (content1.Headers.ContentType.MediaType == "video/webm")
                    {
                        FileStream fs = new FileStream(path + thing.id + ".webm", FileMode.Create);
                        fs.Write(imagebytes);
                    }
                }
            }
        }
    }
    class File
    {
        public string url { get; set; } = "";
    }
    class Post
    {
        public File file { get; set; } = new File();
        public int id { get; set; } = 0;
    }
    class Data
    {
        public Post[] posts { get; set; } = new Post[1];
    }
}
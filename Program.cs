using System;
using System.Configuration;
using System.IO;
using System.Threading.Tasks;

namespace BlobUploader
{
    class Program
    {
        static async Task Main()
        {
            const string localPathPrefix = @"I:\Music\";
            //const string collection = @"My Downloaded Music - FLAC\";
            const string collection = @"My CDs - FLAC\";
            const string shareName = @"music";

            string connectionString = ConfigurationManager.AppSettings.Get("connectionString");

            using (StreamWriter log = new StreamWriter("./log.txt", append: true))
            {
                var blobService = new BlobService(connectionString, log);
                var artists = Directory.GetDirectories(localPathPrefix + collection);

                var startWithArtist = "Jethro Tull";
                var started = false;

                try
                {
                    foreach (var artistPath in artists)
                    {
                        var artist = Path.GetFileName(artistPath);
                        if (!string.IsNullOrEmpty(startWithArtist) && 
                            !started && artist != startWithArtist)
                        {
                            continue;
                        }
                        started = true;
                        artist += @"\";

                        var albums = Directory.GetDirectories(artistPath);
                        foreach (var albumPath in albums)
                        {
                            var album = Path.GetFileName(albumPath) + @"\";

                            var songs = Directory.GetFiles(albumPath);
                            foreach (var songPath in songs)
                            {
                                var song = Path.GetFileName(songPath);
                                await blobService.WriteFile(songPath, shareName,
                                    collection + artist + album + song);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    log.WriteLine("Error: " + e.Message);
                    log.Flush();
                }

                log.Close();
            }
        }
    }
}

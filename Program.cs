using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Threading.Tasks;

namespace BlobUploader
{
    class Program
    {
        public static BlobService BlobService;

        static async Task Main()
        {
            const string localPathPrefix = @"I:\Music\";
            const string databasePath = @"Database\Music Inventory.accdb";
            const string shareName = @"music";

            var collections = new List<string>()
            {
                @"My CDs - FLAC\",
                @"My Downloaded Music - FLAC\",
            };
            var connectionString = ConfigurationManager.AppSettings.Get("ConnectionString");

            using (var log = new BlobLog())
            {
                BlobService = new BlobService(connectionString, log);

                try
                {
                    var lastRunTime = log.GetLastRunTime();

                    // Update playlist files at the top level that have changed since the 
                    // last time we ran this program.
                    var playlists = Directory.GetFiles(localPathPrefix);
                    foreach (var playlistPath in playlists)
                    {
                        var playlist = Path.GetFileName(playlistPath);
                        await WriteFileIfNeeded(localPathPrefix + playlist, playlist, 
                            true, lastRunTime);
                    }

                    await WriteFileIfNeeded(localPathPrefix + databasePath, databasePath, 
                        true, lastRunTime);

                    // TODO: update database file

                    // Drill down the path of collection/artist/album to find songs that
                    // have been updated since the last time we ran this program.
                    foreach (var collection in collections)
                    {
                        var artists = Directory.GetDirectories(localPathPrefix + collection);

                        foreach (var artistPath in artists)
                        {
                            var artist = Path.GetFileName(artistPath) + @"\";

                            var albums = Directory.GetDirectories(artistPath);
                            foreach (var albumPath in albums)
                            {
                                var album = Path.GetFileName(albumPath) + @"\";

                                var songs = Directory.GetFiles(albumPath);
                                foreach (var songPath in songs)
                                {
                                    var song = Path.GetFileName(songPath);
                                    await WriteFileIfNeeded(songPath, 
                                        collection + artist + album + song, true, lastRunTime);
                                }
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
                log.SetLastRunTime(DateTime.Now);
            }

            async Task WriteFileIfNeeded(string localPath, string blobPath,
                bool overwrite, DateTime lastRunTime)
            {
                var lastWriteTime = File.GetLastWriteTime(localPath);
                if (lastWriteTime > lastRunTime)
                {
                    await BlobService.WriteFile(localPath, shareName, blobPath, overwrite);
                }
            }
        }
    }
}

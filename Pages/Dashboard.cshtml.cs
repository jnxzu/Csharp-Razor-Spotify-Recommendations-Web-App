using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;

namespace SpotifyR
{

    public class DashboardModel : PageModel
    {
        private IMemoryCache _cache { get; set; }

        [BindProperty]
        public List<Track> NEW_RELEASES { get; set; }
        [BindProperty]
        public List<Track> RECOMM { get; set; }

        [BindProperty]
        public String ArtistsNames { get; set; }

        [BindProperty]
        public String AlbumsNames { get; set; }

        private SpotifyAuth sAuth = new SpotifyAuth();

        private String _accessToken { get; set; }

        private PolecankoDBContext _polecankoDbContext;

        JsonSerializerSettings settings = new JsonSerializerSettings()
        {
            MissingMemberHandling = MissingMemberHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore
        };

        public DashboardModel(IMemoryCache cache, PolecankoDBContext polecankoDbContext)
        {
            _polecankoDbContext = polecankoDbContext;
            _cache = cache;
        }

        public IActionResult OnGet()
        {
            string code = (string)@TempData["code"];
            _accessToken = _cache.GetOrCreate("token", entry =>
            {
                return GetTokens(code).access_token;
            });
            var followedArtists = _cache.GetOrCreate("artists", entry =>
            {
                return GetFollowedArtists(_accessToken, null);
            });
            RECOMM = _cache.GetOrCreate("recomm", entry =>
            {
                return GetSimilar(_accessToken, followedArtists);
            });
            NEW_RELEASES = _cache.GetOrCreate("new", entry =>
            {
                return NewReleases(_accessToken, followedArtists);
            });
            if (AlbumsNames == null)
            {
                AlbumsNames = _cache.Get<string>("albumtext");
                ArtistsNames = _cache.Get<string>("artiststext");
            }
            return Page();
        }

        public void OnPost(string ArtistId, bool value)
        {
            _accessToken = _cache.Get<string>("token");
            Rate(ArtistId, value);
        }


        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //                                                                                                           //
        //                                                                                                           //
        //                                                                                                           //
        //                                                                                                           //
        //                                              REQUESTY ITD                                                 //
        //                                                                                                           //
        //                                                                                                           //
        //                                                                                                           //
        //                                                                                                           //
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////


        public TokensResponse GetTokens(string code)
        {
            string responseString;
            using (HttpClient client = new HttpClient())
            {
                var authorization = Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(sAuth.clientID + ":" + sAuth.clientSecret));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authorization);
                var parameters = new FormUrlEncodedContent(new Dictionary<string, string>{
                        {"code", code},
                        {"redirect_uri", sAuth.redirectURL},
                        {"grant_type", "authorization_code"},
                    });
                var response = client.PostAsync("https://accounts.spotify.com/api/token", parameters);
                var responseContent = response.Result.Content;
                responseString = responseContent.ReadAsStringAsync().Result;
            }
            return JsonConvert.DeserializeObject<TokensResponse>(responseString, settings);
        }

        public PagingAlbum GetArtistsAlbums(string _accessToken, string artistID)
        {
            string responseString;
            using (HttpClient client = new HttpClient())
            {
                var authorization = _accessToken;
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
                String adres = "https://api.spotify.com/v1/artists/" + artistID + "/albums?include_groups=album&limit=1";
                var response = client.GetAsync(adres);
                while (response.Result.StatusCode.ToString().Equals("TooManyRequests"))
                {
                    var timeToWait = response.Result.Headers.RetryAfter.Delta?.Seconds;
                    Console.WriteLine("too many requests, waiting to retry...");
                    System.Threading.Thread.Sleep((int)timeToWait * 1000);
                    response = client.GetAsync(adres);
                }
                var responseContent = response.Result.Content;
                responseString = responseContent.ReadAsStringAsync().Result;
            }
            return JsonConvert.DeserializeObject<PagingAlbum>(responseString, settings);
        }

        public PagingAlbum GetArtistsSingles(string _accessToken, string artistID)
        {
            string responseString;
            using (HttpClient client = new HttpClient())
            {
                var authorization = _accessToken;
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
                String adres = "https://api.spotify.com/v1/artists/" + artistID + "/albums?include_groups=single&limit=2";
                var response = client.GetAsync(adres);
                while (response.Result.StatusCode.ToString().Equals("TooManyRequests"))
                {
                    var timeToWait = response.Result.Headers.RetryAfter.Delta?.Seconds;
                    Console.WriteLine("too many requests, waiting to retry...");
                    System.Threading.Thread.Sleep((int)timeToWait * 1000);
                    response = client.GetAsync(adres);
                }
                var responseContent = response.Result.Content;
                responseString = responseContent.ReadAsStringAsync().Result;
            }
            return JsonConvert.DeserializeObject<PagingAlbum>(responseString, settings);
        }

        public List<Artist> GetFollowedArtists(String _accessToken, String next)
        {
            string responseString;
            List<Artist> resultList = new List<Artist>();
            using (HttpClient client = new HttpClient())
            {
                var authorization = _accessToken;
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
                String adres = next == null ? "https://api.spotify.com/v1/me/following?type=artist&limit=50" : next + "&limit=50";
                var response = client.GetAsync(adres);
                while (response.Result.StatusCode.ToString().Equals("TooManyRequests"))
                {
                    var timeToWait = response.Result.Headers.RetryAfter.Delta?.Seconds;
                    Console.WriteLine("too many requests, waiting to retry...");
                    System.Threading.Thread.Sleep((int)timeToWait * 1000);
                    response = client.GetAsync(adres);
                }
                var responseContent = response.Result.Content;
                responseString = responseContent.ReadAsStringAsync().Result;
            }
            var responseContainer = JsonConvert.DeserializeObject<ArtistsContainer>(responseString, settings).artists;
            resultList.AddRange(responseContainer.items);
            if (responseContainer.next != null)
            {
                resultList.AddRange(GetFollowedArtists(_accessToken, responseContainer.next));
            }
            return resultList;
        }

        public List<Track> NewReleases(String _accessToken, List<Artist> followedArtists)
        {
            var newestAlbums = GetNewReleases(_accessToken, followedArtists);
            var newSongs = GetPopularSongs(_accessToken, newestAlbums);
            newSongs = newSongs.GroupBy(x => x.name).Select(x => x.First()).OrderBy(x => new Random().Next()).ToList();
            return newSongs;
        }

        public List<Album> GetNewReleases(String _accessToken, List<Artist> artists)
        {
            var resultList = new List<Album>();
            var results = new ConcurrentBag<Album>();
            Parallel.ForEach(artists, (artist) =>
           {
               var artistsAlbums = GetArtistsAlbums(_accessToken, artist.id).items;
               if (artistsAlbums != null)
               {
                   foreach (Album album in artistsAlbums)
                   {
                       if (album.release_date_precision == "day")
                       {
                           DateTime albumDate = DateTime.Parse(album.release_date);
                           TimeSpan ts = DateTime.Now.Subtract(albumDate);
                           if (ts.TotalDays < 30)
                               results.Add(album);
                       }
                   }
                   var artistsSingles = GetArtistsSingles(_accessToken, artist.id).items;
                   if (artistsSingles != null)
                   {
                       foreach (Album single in artistsSingles)
                       {
                           if (single.release_date_precision == "day")
                           {
                               DateTime singleDate = DateTime.Parse(single.release_date);
                               TimeSpan ts = DateTime.Now.Subtract(singleDate);
                               if (ts.TotalDays < 30)
                                   results.Add(single);
                           }
                       }
                   }
               }
           });
            resultList = results.ToList();
            _cache.GetOrCreate("albumtext", entry =>
            {
                Random rand = new Random();
                var chosenAlbums = resultList.OrderBy(x => rand.Next()).Where(x => x.album_type != "single").Distinct().Take(resultList.Count < 3 ? resultList.Count : 3).ToList();
                for (var i = 0; i < chosenAlbums.Count; i++)
                {
                    if (i == chosenAlbums.Count - 1)
                    {
                        AlbumsNames = AlbumsNames.Remove(AlbumsNames.Length - 2);
                        AlbumsNames += " & " + chosenAlbums[i].name;
                    }
                    else
                    {
                        AlbumsNames += chosenAlbums[i].name + ", ";
                    }
                }
                return AlbumsNames;
            });
            return resultList;
        }

        public AlbumsContainer GetManyAlbums(String _accessToken, String url)
        {
            string responseString;
            using (HttpClient client = new HttpClient())
            {
                var authorization = _accessToken;
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
                String adres = "https://api.spotify.com/v1/albums/?ids=" + url;
                var response = client.GetAsync(adres);
                while (response.Result.StatusCode.ToString().Equals("TooManyRequests"))
                {
                    var timeToWait = response.Result.Headers.RetryAfter.Delta?.Seconds;
                    Console.WriteLine("too many requests, waiting to retry...");
                    System.Threading.Thread.Sleep((int)timeToWait * 1000);
                    response = client.GetAsync(adres);
                }
                var responseContent = response.Result.Content;
                responseString = responseContent.ReadAsStringAsync().Result;
            }
            return JsonConvert.DeserializeObject<AlbumsContainer>(responseString, settings);
        }

        public List<Track> GetPopularSongs(String _accessToken, List<Album> albums)
        {
            var resultList = new List<Track>();
            var urls = new String[(int)Math.Ceiling((decimal)albums.Count / 20)];
            var i = 0;
            var j = 0;
            while (i < albums.Count)
            {
                for (j = i; j < (albums.Count < i + 20 ? albums.Count : i + 20); j++)
                {
                    urls[(int)Math.Floor((decimal)j / 20)] += albums[j].id + ",";
                }
                i += j;
            }
            foreach (var url in urls)
            {
                var fixUrl = url.Remove(url.Length - 1);
                var response = GetManyAlbums(_accessToken, fixUrl);
                foreach (var album in response.albums)
                {
                    var albumTracks = album.tracks.items.ToList();
                    albumTracks.Sort((p, q) => p.popularity.CompareTo(q.popularity));
                    var returnSize = albumTracks.Count * 0.15 <= 1 ? 1 : 2;
                    for (var k = 0; k < returnSize; k++)
                    {
                        albumTracks[k].album = album;
                        resultList.Add(albumTracks[k]);
                    }
                }
            }
            return resultList;
        }

        public List<Track> GetSimilar(String _accessToken, List<Artist> artists)
        {
            var results = new List<Track>();
            Random rand = new Random();
            var chosenArtists = artists.OrderBy(x => rand.Next()).Distinct().Take(artists.Count < 5 ? artists.Count : 5).ToList();
            var seedArtists = "";
            string responseString;
            foreach (var artist in chosenArtists)
            {
                seedArtists += artist.id + ",";
            }
            seedArtists = seedArtists.Remove(seedArtists.Length - 1);
            using (HttpClient client = new HttpClient())
            {
                var authorization = _accessToken;
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
                var url = "https://api.spotify.com/v1/recommendations?limit=" + rand.Next(20, 45) + "&seed_artists=" + seedArtists;
                var response = client.GetAsync(url);
                while (response.Result.StatusCode.ToString().Equals("TooManyRequests"))
                {
                    var timeToWait = response.Result.Headers.RetryAfter.Delta?.Seconds;
                    Console.WriteLine("too many requests, waiting to retry...");
                    System.Threading.Thread.Sleep((int)timeToWait * 1000);
                    response = client.GetAsync(url);
                }
                var responseContent = response.Result.Content;
                responseString = responseContent.ReadAsStringAsync().Result;
            }
            var resultTracks = JsonConvert.DeserializeObject<Recommendations>(responseString, settings).tracks;
            results.AddRange(resultTracks.ToList());
            _cache.GetOrCreate("artiststext", entry =>
            {
                var chosenResults = results.OrderBy(x => rand.Next()).Select(x => x.artists[0].name).Distinct().Take(3).ToList();
                ArtistsNames = chosenResults[0] + ", " + chosenResults[1] + " & " + chosenResults[2];
                return ArtistsNames;
            });
            return results;
        }

        private User GetUser(string _accessToken)
        {
            var result = new User();

            using (HttpClient client = new HttpClient())
            {
                var authorization = _accessToken;
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
                var url = "https://api.spotify.com/v1/me";
                var response = client.GetAsync(url);
                var responseContent = response.Result.Content;
                result = JsonConvert.DeserializeObject<User>(responseContent.ReadAsStringAsync().Result, settings);
            }
            return result;
        }

        public void Rate(string ArtistId, bool value)
        {
            var userId = GetUser(_accessToken).id;
            if (_polecankoDbContext.users.Find(userId) == null)
            {
                UserDB userDB = new UserDB();
                userDB.id = userId;
                _polecankoDbContext.users.Add(userDB);
            }
            if (_polecankoDbContext.artists.Find(ArtistId) == null)
            {
                ArtistDB artistDB = new ArtistDB();
                artistDB.id = ArtistId;
            }
            Rating rating = new Rating();
            rating.user = _polecankoDbContext.users.Find(userId);
            rating.artist = _polecankoDbContext.artists.Find(userId);
            rating.value = value;
            _polecankoDbContext.Add(rating);
        }
    }
}
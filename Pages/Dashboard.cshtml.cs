using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;

namespace SpotifyR
{
    public class DashboardModel : PageModel
    {
        [BindProperty]
        public List<Track> NEW_RELEASES { get; set; }

        [BindProperty]
        public List<Track> DISCOVER { get; set; }
        private SpotifyAuth sAuth = new SpotifyAuth();

        JsonSerializerSettings settings = new JsonSerializerSettings()
        {
            MissingMemberHandling = MissingMemberHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore
        };

        public IActionResult OnGet(String code)
        {
            var access_token = GetTokens(code).access_token;
            NEW_RELEASES = NewReleases(access_token);
            // RECOMM = RecomReleases(access_token);
            return Page();
        }

        public List<Track> NewReleases(String access_token)
        {
            var followedArtists = GetFollowedArtists(access_token, null);
            var newestAlbums = GetNewReleases(access_token, followedArtists);
            var newSongs = GetPopularSongs(access_token, newestAlbums);
            // remove duplicates
            // shuffle
            return newSongs;
        }

        public List<Track> Discover(String access_token)
        {
            //TODO
            return null;
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

        public Track GetTrackById(string access_token, string tID)
        {
            string responseString = "";
            using (HttpClient client = new HttpClient())
            {
                var authorization = access_token;
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", access_token);
                var response = client.GetAsync("https://api.spotify.com/v1/tracks/" + tID);
                var responseContent = response.Result.Content;
                responseString += responseContent.ReadAsStringAsync().Result;
            }
            return JsonConvert.DeserializeObject<Track>(responseString, settings);
        }

        public PagingAlbum GetArtistsAlbums(string access_token, string artistID)
        {
            string responseString;
            using (HttpClient client = new HttpClient())
            {
                var authorization = access_token;
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", access_token);
                String adres = "https://api.spotify.com/v1/artists/" + artistID + "/albums?include_groups=album&limit=1";
                var response = client.GetAsync(adres);
                var responseContent = response.Result.Content;
                responseString = responseContent.ReadAsStringAsync().Result;
            }
            return JsonConvert.DeserializeObject<PagingAlbum>(responseString, settings);
        }

        public PagingAlbum GetArtistsSingles(string access_token, string artistID)
        {
            string responseString;
            using (HttpClient client = new HttpClient())
            {
                var authorization = access_token;
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", access_token);
                String adres = "https://api.spotify.com/v1/artists/" + artistID + "/albums?include_groups=single&limit=2";
                var response = client.GetAsync(adres);
                var responseContent = response.Result.Content;
                responseString = responseContent.ReadAsStringAsync().Result;
            }
            return JsonConvert.DeserializeObject<PagingAlbum>(responseString, settings);
        }

        public Album GetAlbumById(string access_token, string albumID)
        {
            string responseString;
            using (HttpClient client = new HttpClient())
            {
                var authorization = access_token;
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", access_token);
                String adres = "https://api.spotify.com/v1/albums/" + albumID;
                var response = client.GetAsync(adres);
                var responseContent = response.Result.Content;
                responseString = responseContent.ReadAsStringAsync().Result;
            }
            return JsonConvert.DeserializeObject<Album>(responseString, settings);
        }

        public List<Artist> GetFollowedArtists(String access_token, String next)
        {
            string responseString;
            List<Artist> resultList = new List<Artist>();
            using (HttpClient client = new HttpClient())
            {
                var authorization = access_token;
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", access_token);
                String adres = next == null ? "https://api.spotify.com/v1/me/following?type=artist&limit=50" : next + "&limit=50";
                var response = client.GetAsync(adres);
                var responseContent = response.Result.Content;
                responseString = responseContent.ReadAsStringAsync().Result;
            }
            var responseContainer = JsonConvert.DeserializeObject<ArtistsContainer>(responseString, settings).artists;
            resultList.AddRange(responseContainer.items);
            if (responseContainer.next != null)
            {
                resultList.AddRange(GetFollowedArtists(access_token, responseContainer.next));
            }
            return resultList;
        }

        public List<Album> GetNewReleases(String access_token, List<Artist> artists)
        {
            var resultList = new List<Album>();

            var results = new ConcurrentBag<Album>();

            Parallel.ForEach(artists, (artist) =>
           {
               var artistsAlbums = GetArtistsAlbums(access_token, artist.id).items;
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

                   var artistsSingles = GetArtistsSingles(access_token, artist.id).items;

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
            return resultList;
        }

        public List<Track> GetPopularSongs(String access_token, List<Album> albums)
        {
            var resultList = new List<Track>();
            foreach (var album in albums)
            {
                var albumSpecific = GetAlbumById(access_token, album.id);
                if (albumSpecific.id != null)
                {
                    var albumTracks = albumSpecific.tracks.items.ToList();
                    albumTracks.Sort((p, q) => p.popularity.CompareTo(q.popularity));
                    var returnSize = albumTracks.Count * 0.15;
                    returnSize = returnSize <= 1 ? 1 : 2;
                    for (var i = 0; i < returnSize; i++)
                    {
                        albumTracks[i].album = album;
                        resultList.Add(albumTracks[i]);
                    }
                }
            }
            return resultList;
        }

        public List<Track> RecomReleases(String access_token)
        {
            var followedArtists = GetFollowedArtists(access_token, null);
            var recom = GetSimmilar(access_token, followedArtists);
            return recom;
        }

        public List<Track> GetSimmilar(String access_token, List<Artist> artists)
        {
            var resultList = new List<Track>();
            string responseString = "";
            foreach (var artist in artists)
            {
                using (HttpClient client = new HttpClient())
                {
                    var authorization = access_token;
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", access_token);
                    var response = client.GetAsync("https://api.spotify.com/v1/recommendations?seed_artists=" + artist.id);
                    var responseContent = response.Result.Content;
                    responseString += responseContent.ReadAsStringAsync().Result;
                }
            }
            resultList = JsonConvert.DeserializeObject<Recommendations>(responseString, settings).tracks.ToList();
            return resultList;
        }

    }
}
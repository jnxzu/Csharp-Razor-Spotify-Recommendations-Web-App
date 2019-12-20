using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
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
                var responseContent = client.PostAsync("https://accounts.spotify.com/api/token", parameters).Result.Content;
                responseString = responseContent.ReadAsStringAsync().Result;
            }
            return JsonConvert.DeserializeObject<TokensResponse>(responseString, settings);
        }
        public Track GetTrack(string access_token, string tID)
        {
            string responseString = "";
            using (HttpClient client = new HttpClient())
            {
                var authorization = access_token;
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", access_token);
                var responseContent = client.GetAsync("https://api.spotify.com/v1/tracks/" + tID).Result.Content;
                responseString += responseContent.ReadAsStringAsync().Result;
            }
            return JsonConvert.DeserializeObject<Track>(responseString, settings);
        }
        public PagingTrack GetTracks(string access_token, int i)
        {
            string responseString = "";
            using (HttpClient client = new HttpClient())
            {
                var authorization = access_token;
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", access_token);
                var responseContent = client.GetAsync("https://api.spotify.com/v1/me/tracks?offset=" + i).Result.Content;
                responseString += responseContent.ReadAsStringAsync().Result;
            }
            return JsonConvert.DeserializeObject<PagingTrack>(responseString, settings);
        }

        public PagingAlbum GetAlbums(string access_token, string artistID)
        {
            string responseString;
            using (HttpClient client = new HttpClient())
            {
                var authorization = access_token;
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", access_token);
                String adres = "https://api.spotify.com/v1/artists/" + artistID + "/albums";
                var responseContent = client.GetAsync(adres).Result.Content;
                responseString = responseContent.ReadAsStringAsync().Result;
            }
            return JsonConvert.DeserializeObject<PagingAlbum>(responseString, settings);
        }

        public PagingAlbum GetAlbum(string access_token, string albumID)
        {
            string responseString;
            using (HttpClient client = new HttpClient())
            {
                var authorization = access_token;
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", access_token);
                String adres = "https://api.spotify.com/v1/albums/" + albumID + "/tracks";
                var responseContent = client.GetAsync(adres).Result.Content;
                responseString = responseContent.ReadAsStringAsync().Result;
            }
            return JsonConvert.DeserializeObject<PagingAlbum>(responseString, settings);
        }


        public Boolean Datownik(String data, int ileMaxDni) {
            if (data.Length!=10) return false;
            var numbers = data.Split('-').Select(Int32.Parse).ToList();
            DateTime date = new DateTime(numbers[0], numbers[1], numbers[2]);
            var now = DateTime.Now;
            TimeSpan diff = now.Subtract(date);
            TimeSpan diff0 = new TimeSpan(ileMaxDni, 0, 0, 0);
            return diff<diff0;
        }
        public List<Track> ZrobJebanyAlgorytmRafałKurwa(TokensResponse tokens){
            var artists = new HashSet<String>();
            var newalbums = new HashSet<String>();
            var newtracks = new HashSet<String>();
            var poptracks = new HashSet<String>();
            var albums = new HashSet<Album>();
            var re = new List<Track>();

            var albumPaging = new PagingAlbum();
            for (int i = 0; i<10; i++) {
                var tracksPaging = GetTracks(tokens.access_token, 20*i);
                try{
                    foreach (var k in tracksPaging.items) foreach (var j in k.track.artists) artists.Add(j.id);
                foreach (var artist in artists) {
                    var albumsPaging = GetAlbums(tokens.access_token, artist);
                    foreach (var a in albumsPaging.items) if (Datownik(a.release_date, 300)) newalbums.Add(a.id);
                }
                foreach (var a in newalbums) {
                    var aPaging = GetAlbum(tokens.access_token, a);
                    foreach (var g in aPaging.items) newtracks.Add(g.id);
                }
                foreach (var s in newtracks) {
                    var track = GetTrack(tokens.access_token, s);
                    if (track.popularity>60) poptracks.Add(track.id);
                }
                foreach (var p in poptracks) {
                    re.Add(GetTrack(tokens.access_token, p));
                }
                }catch (System.NullReferenceException) {}
                //dla kazdego artysty w "artists" wszystkie albumny nowsze niz X dni wstecz od dzisiaj
                //dla kazdego z tych albumow top X najpopularniejszych utworow do "tracks"
            }
            return re;
        }

        public IActionResult OnGet(String code)
        {
            var tokens = GetTokens(code);
           var NEW_RELEASES = new List<Track>();
            NEW_RELEASES = ZrobJebanyAlgorytmRafałKurwa(tokens);
            return Page();
        }
    }
}
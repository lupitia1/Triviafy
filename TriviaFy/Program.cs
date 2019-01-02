using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using TriviaFy;
using TriviaFy.JsonModels;

namespace HttpClientTriviafy
{
    /*public class Product
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string Category { get; set; }
    }*/
    public class ClientCredentials
    {
        public string ClientID { get; set; }
        public string ClientSecret { get; set; }
    }
    class Program
    {
        static HttpClient client = new HttpClient();

        static async Task<AccessToken> GenerateToken(ClientCredentials spotifyClient)
        {
            string credentials = String.Format("{0}:{1}", spotifyClient.ClientID, spotifyClient.ClientSecret);

            //Define Headers
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes(credentials)));

            //Prepare Request Body
            List<KeyValuePair<string, string>> requestData = new List<KeyValuePair<string, string>>();
            requestData.Add(new KeyValuePair<string, string>("grant_type", "client_credentials"));
            FormUrlEncodedContent requestBody = new FormUrlEncodedContent(requestData);
            var request = await client.PostAsync("https://accounts.spotify.com/api/token", requestBody);
            var response = await request.Content.ReadAsStringAsync();
            Console.WriteLine(response);

            // return the Access Token
            return JsonConvert.DeserializeObject<AccessToken>(response);

        }

        static async Task<PlaylistsReceived> GetPlaylist(String tokenAuth)
        {
            //Define Headers
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenAuth);

            //Prepare Request Body
            List<KeyValuePair<string, string>> requestData = new List<KeyValuePair<string, string>>();
            //requestData.Add(new KeyValuePair<string, string>("grant_type", "client_credentials"));
            //FormUrlEncodedContent requestBody = new FormUrlEncodedContent(requestData);
            //var url = "https://api.spotify.com/v1/browse/featured-playlists";

            var builder = new UriBuilder("https://api.spotify.com/v1/browse/featured-playlists");
            builder.Port = -1;
            var query = HttpUtility.ParseQueryString(builder.Query);
            query["limit"] = "1";
            query["timestamp"] = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
            query["country"] = "ES";

            builder.Query = query.ToString();
            string url = builder.ToString();

            var request = await client.GetAsync(url);
            var response = await request.Content.ReadAsStringAsync();
          
            // return the received playlist
            return JsonConvert.DeserializeObject<PlaylistsReceived>(response);
        }

        static async Task<TracksInfo> GetPlaylistTracks(String tokenAuth, String playlistID)
        {
            //Define Headers
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenAuth);

            //Prepare Request Body
            List<KeyValuePair<string, string>> requestData = new List<KeyValuePair<string, string>>();
            String urlWithtId = "https://api.spotify.com/v1/playlists/" + playlistID + "/tracks";
            var builder = new UriBuilder(urlWithtId);
            builder.Port = -1;
            var query = HttpUtility.ParseQueryString(builder.Query);
            //query["limit"] = "1";
            query["market"] = "ES";
            query["fields"] = "items(track(name,href, id, uri, album(name,href,images)))";

            builder.Query = query.ToString();
            string url = builder.ToString();

            var request = await client.GetAsync(url);
            var response = await request.Content.ReadAsStringAsync();
            
            // return the received tracks info
            return JsonConvert.DeserializeObject<TracksInfo>(response);
        }

        static async Task<AccessToken> GetSeveralTracks(ClientCredentials spotifyClient)
        {
            string credentials = String.Format("{0}:{1}", spotifyClient.ClientID, spotifyClient.ClientSecret);

            //Define Headers
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes(credentials)));

            //Prepare Request Body
            List<KeyValuePair<string, string>> requestData = new List<KeyValuePair<string, string>>();
            requestData.Add(new KeyValuePair<string, string>("grant_type", "client_credentials"));
            FormUrlEncodedContent requestBody = new FormUrlEncodedContent(requestData);
            var request = await client.PostAsync("https://accounts.spotify.com/api/token", requestBody);
            var response = await request.Content.ReadAsStringAsync();
            Console.WriteLine(response);

            // return the Access Token
            return JsonConvert.DeserializeObject<AccessToken>(response);
        }

        static List<Track> SelectTracks(TracksInfo o)
        {
            var numberOfTracks = o.items.Count;

            // Crear objeto. Utiliza el reloj del sistema para crear una semilla.
            Random rnd = new Random();
            int randomNumber;
            List<int> tracksRandomNumbers = new List<int>();
            List<Track> listOfSelectedTracks = new List<Track>();

            for (int i = 0; i < 4; i++) {

                randomNumber = rnd.Next(numberOfTracks - 1);
                tracksRandomNumbers.Add(randomNumber);
            }
            foreach (int number in tracksRandomNumbers)
            {
                listOfSelectedTracks.Add(o.items[number].track);
            }
            return listOfSelectedTracks;
        }


       
         static void Main()
        {
            RunAsync().GetAwaiter().GetResult();
        }

        static async Task RunAsync()
        {
            client.BaseAddress = new Uri("https://accounts.spotify.com");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            try
            {
                // Get a Token  (OAuth)
                ClientCredentials spotifyClient = new ClientCredentials
                {
                    ClientID = "8a5cb9ed6fa44409a8379c7dc186c1e7",
                    ClientSecret = "6afa8191a22a4e55ac55c60c3db3711b"
                };            

                //Get the OAuth Token
                var tokenReceived = await GenerateToken(spotifyClient);
                var tokenReceivedString = tokenReceived.Access_token;
        
                //Get a Playlist
                var playlistReceived = await GetPlaylist(tokenReceivedString);
                PlaylistSelected playlistSelected = new PlaylistSelected(playlistReceived);

                //Get a Playlist Tracks (And extra info about them)
                var playlistTracksReceived = await GetPlaylistTracks(tokenReceivedString, playlistSelected.id);

                //Choose the four tracks to send
                var selectedTracks = SelectTracks(playlistTracksReceived);
                Console.WriteLine("");
                foreach (Track track in selectedTracks)
                {
                  Console.WriteLine("Name: " + track.name);
                  Console.WriteLine("Id: " + track.id);
                  Console.WriteLine("Album Name: " + track.album.name);

                    Console.WriteLine("");


                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            Console.ReadLine();
        }

    }

    public class AccessToken
    {
        public string Access_token { get; set; }
        public string Token_type { get; set; }
        public long Expires_in { get; set; }
    }

    public class PlaylistSelected
    {
        public PlaylistSelected(PlaylistsReceived o){
            this.id = o.playlists.items[0].id;
            this.name = o.playlists.items[0].name;
            this.message = o.message;
            this.tracksNumber = o.playlists.items[0].tracks.total;

    }
        public string id { get; set; }
        public string name { get; set; }
        public string message { get; set; }
        public int tracksNumber { get; set; }


    }
   

}

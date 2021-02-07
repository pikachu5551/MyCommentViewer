using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MyCommentViewer
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ConnectOpenrec_Click(object sender, RoutedEventArgs e)
        {
            ConnectOpenrecMethod(OpenrecUrl.Text);
        }

        private void ConnectOpenrecMethod(string url)
        {
            string html = GetHtml(url);
            string userId = OpenrecUserIdPurser(html);
            string movieId = OpenrecMovieIdPurser(userId);
            string websocketUrl = $"wss://chat.openrec.tv/socket.io/?movieId={movieId}&EIO=3&transport=websocket";
            
            //Console.WriteLine(movieId);
        }

        /// <summary>
        /// 配信者のユーザーIDからapiのURLを作成し、配信ページのmovie_idをパースする事で取得
        /// </summary>
        /// <param name="userId">配信者のユーザーID</param>
        /// <returns>配信ページのmovie_id</returns>
        private string OpenrecMovieIdPurser(string userId)
        {
            string apiUrl = $"https://public.openrec.tv/external/api/v5/movies?channel_ids={userId}&sort=onair_status&is_upload=false";
            string movieId = GetHtml(apiUrl);
            movieId = movieId.Substring(movieId.IndexOf("movie_id"));
            movieId = movieId.Replace("movie_id\":", "");
            movieId = movieId.Substring(0, movieId.IndexOf(","));
            return movieId;
        }

        /// <summary>
        /// 配信ページのHTMLをパースし、配信者のユーザーIDを取得
        /// </summary>
        /// <param name="html">配信ページのHTML</param>
        /// <returns>配信者のユーザーID</returns>
        private string OpenrecUserIdPurser(string html)
        {
            string userId = html.Substring(html.IndexOf("],\"channel\":{\"id\":\""));
            userId = userId.Replace("],\"channel\":{\"id\":\"", "");
            userId = userId.Substring(0, userId.IndexOf("\""));
            return userId;
        }

        /// <summary>
        /// 引数にとったURLのHTMLを文字列で取得
        /// </summary>
        /// <param name="url">配信ページのURL</param>
        /// <returns>配信ページのHTML</returns>
        private string GetHtml(string url)
        {
            WebClient wc = new WebClient();
            var html = wc.DownloadString(url);
            return html;
        }
    }
}

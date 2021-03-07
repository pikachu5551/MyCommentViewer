using System;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace MyCommentViewer
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        string _url;

        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Viewの接続ボタンを押した時の動作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ConnectOpenrec_Click(object sender, RoutedEventArgs e)
        {
            await Task.Run(() => ConnectOpenrecMethod());
        }

        /// <summary>
        /// Openrecのコメントサーバーへ接続し、表示する。
        /// </summary>
        private async void ConnectOpenrecMethod()
        {
            // 非同期処理中はメインスレッドであるUIのOpenrecUrl.Textを取得出来ないのでこの関数が必要になる
            this.Dispatcher.Invoke((Action)(() =>
            {
                this._url = this.OpenrecUrl.Text;
            }));

            string html = GetHtml(this._url);
            string userId = OpenrecUserIdPurser(html);
            string movieId = OpenrecMovieIdPurser(userId);
            string webSocketUrl = $"wss://chat.openrec.tv/socket.io/?movieId={movieId}&EIO=3&transport=websocket";
            // wssのアドレスがString型なのでUriにするのに必要？
            var uri = new Uri(webSocketUrl);



            var ws = new ClientWebSocket();
            // ConnectAsync()用のキャンセルトークンのインスタンスを生成(第二引数をCancellationToken.Noneとすると不要だったのでコメント化している)
            // var cts = new CancellationTokenSource();

            // WebSocketでコメントサーバーと接続
            await ws.ConnectAsync(uri, CancellationToken.None);

            var buffer = new byte[1024];

            while (ws.State.ToString() == "Open")
            {
                var segment = new ArraySegment<byte>(buffer);
                var result = await ws.ReceiveAsync(segment, CancellationToken.None);
                int count = result.Count;

                // 受信した文字列データが、用意しているバッファよりも少ない場合、バッファの残りの部分が0になるらしいのでTrimEnd('\0')している
                var message = Encoding.UTF8.GetString(buffer, 0, count).TrimEnd('\0');
                Console.WriteLine("> " + message);

                this.Dispatcher.Invoke((Action)(() =>
                {
                    CommentView.Items.Add(message);
                }));

            }


/*
            while (true)
            {
                //所得情報確保用の配列を準備
                var segment = new ArraySegment<byte>(buffer);

                //サーバからのレスポンス情報を取得
                var result = await ws.ReceiveAsync(segment, CancellationToken.None);

                //エンドポイントCloseの場合、処理を中断
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "OK",
                      CancellationToken.None);
                    return;
                }

                //バイナリの場合は、当処理では扱えないため、処理を中断
                if (result.MessageType == WebSocketMessageType.Binary)
                {
                    await ws.CloseAsync(WebSocketCloseStatus.InvalidMessageType,
                      "I don't do binary", CancellationToken.None);
                    return;
                }

                *//*//メッセージの最後まで取得
                int count = result.Count;

                while (!result.EndOfMessage)
                {
                    if (count >= buffer.Length)
                    {
                        await ws.CloseOutputAsync(WebSocketCloseStatus.InvalidPayloadData,
                          "That's too long", CancellationToken.None);
                        return;
                    }
                    segment = new ArraySegment<byte>(buffer, count, buffer.Length - count);
                    result = await ws.ReceiveAsync(segment, CancellationToken.None);

                    count += result.Count;
                }*//*

                //メッセージを取得
                var message = Encoding.UTF8.GetString(buffer, 0, count);
                Console.WriteLine("> " + message);
            }
*/

            Console.WriteLine("配信ページのmovieId：" + movieId);
            Console.WriteLine("配信のWebSocketアドレス：" + webSocketUrl);
            // WebSocketの状態の確認
            Console.WriteLine("WebSocket接続状態：" + ws.State);
        }

        /// <summary>
        /// 配信者のユーザーIDからapiのURLを作成し、配信ページのmovie_idをパースする事で取得する
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
        /// 配信ページのHTMLをパースし、配信者のユーザーIDを取得する
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
        /// 引数にとったURLのHTMLを文字列で取得する
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

Public Class Form1
    Private url As String = "http://192.168.121.87"
    Private error_uri As Uri = New Uri("about:blank")
    Private statuscode As String
    Private mailserver As String = "doMail.showa-aircraft.co.jp"
    Private debugIP As String = "192.168.121.87"
    Private brows_flg As Boolean = True
    Private sendmail_flg As Boolean = True
    Private mail_address As String = "t.uehara@showa-aircraft.co.jp"
    Private start_h As Int32 = 7
    Private stop_h As Int32 = 19

    Private Sub Form1_Load(ByVal sender As Object, ByVal e As EventArgs) Handles MyBase.Load

        Me.KeyPreview = True
        WebBrowser1.ScriptErrorsSuppressed = True
        WebBrowser1.ScrollBarsEnabled = False
        WebBrowser1.Width = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width
        WebBrowser1.Height = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height
        WebBrowser1.Navigate("about:blank")
        WebBrowser1.Document.BackColor = System.Drawing.Color.Black

        importSetting()

        '「最大化表示」→「フルスクリーン表示」では
        ' タスク・バーが消えないので、いったん「通常表示」を行う
        If Me.WindowState = FormWindowState.Maximized Then
            Me.WindowState = FormWindowState.Normal
        End If
        Me.Width = 800
        Me.Height = 600
        'フォームの境界線スタイルを「None」にする
        Me.FormBorderStyle = FormBorderStyle.None
        'フォームのウィンドウ状態を「最大化」する
        Me.WindowState = FormWindowState.Maximized
        '最前面にする
        Me.TopMost = True
        Me.Focus()

        'カーソルを非表示にする 
        System.Windows.Forms.Cursor.Hide()

    End Sub

    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        importSetting()
        ' 必要な変数を宣言する
        Dim dtNow As DateTime = DateTime.Now
        ' 時 (Hour) を取得する
        Dim iHour As Int32 = dtNow.Hour
        Console.WriteLine(dtNow)
        If start_h <= iHour AndAlso iHour < stop_h Then
            browsPage(True)
        Else
            browsPage(False)
        End If
    End Sub

    'PictureBox1のMouseEnterイベントハンドラ
    Private Sub Form1_MouseEnter(ByVal sender As Object, ByVal e As System.EventArgs) Handles MyBase.MouseEnter

        'PictureBox1内でカーソルを非表示にする 
        System.Windows.Forms.Cursor.Hide()
    End Sub

    'PictureBox1のMouseLeaveイベントハンドラ
    Private Sub Form1_MouseLeave(ByVal sender As Object, ByVal e As System.EventArgs) Handles MyBase.MouseLeave

        'PictureBox1から出たらカーソルを表示にする 
        System.Windows.Forms.Cursor.Show()
    End Sub


    Private Function checkLAN()
        'ネットワークに接続されているか調べる
        If My.Computer.Network.IsAvailable Then
            Console.WriteLine("ネットワークに接続されています")
            'If My.Computer.Network.Ping(debugIP, 1000) Then
            '    Console.WriteLine("Pingに成功。")
            '    checkLAN = getHTTPStatusCode(Me.url)
            'Else
            '    Console.WriteLine("Pingに失敗。")
            '    checkLAN = 10000
            'End If

            'Pingオブジェクトの作成
            Dim p As New System.Net.NetworkInformation.Ping()
            '"www.yahoo.com"にPingを送信する
            Dim reply As System.Net.NetworkInformation.PingReply = p.Send(debugIP, 1000)
            '結果を取得
            If reply.Status = System.Net.NetworkInformation.IPStatus.Success Then
                Console.WriteLine("Reply from {0}:bytes={1} time={2}ms TTL={3}", reply.Address, reply.Buffer.Length, reply.RoundtripTime, reply.Options.Ttl)
                checkLAN = getHTTPStatusCode(Me.url)
            Else
                Console.WriteLine("Ping送信に失敗。({0})", reply.Status)
                checkLAN = 10000
            End If
            p.Dispose()
        Else
                Console.WriteLine("ネットワークに接続されていません")
                checkLAN = -1
        End If
    End Function

    Private Function getHTTPStatusCode(ByVal url As String)
        Dim statuscode As String = Nothing

        'WebRequestの作成
        Dim webreq As System.Net.HttpWebRequest = CType(System.Net.WebRequest.Create(url), System.Net.HttpWebRequest)
        Dim webres As System.Net.HttpWebResponse = Nothing

        webreq.Timeout = 10000

        Try
            'サーバーからの応答を受信するためのWebResponseを取得
            webres = CType(webreq.GetResponse(), System.Net.HttpWebResponse)
            'webres = webreq.GetResponse()

            '応答したURIを表示する
            Console.WriteLine(webres.ResponseUri)
            '応答ステータスコードを表示する
            statuscode = webres.StatusCode.GetHashCode
            'Console.WriteLine("{0}:{1}", webres.StatusCode.GetHashCode, webres.StatusDescription)
        Catch ex As System.Net.WebException
            'HTTPプロトコルエラーかどうか調べる
            If ex.Status = System.Net.WebExceptionStatus.ProtocolError Then
                'HttpWebResponseを取得
                Dim errres As System.Net.HttpWebResponse = CType(ex.Response, System.Net.HttpWebResponse)
                '応答したURIを表示する
                Console.WriteLine(errres.ResponseUri)
                '応答ステータスコードを表示する
                statuscode = errres.StatusCode.GetHashCode
                'Console.WriteLine("errer:{0}:{1}", errres.StatusCode.GetHashCode, errres.StatusDescription)
            Else
                Console.WriteLine(ex.Message)
            End If
        Finally
            '閉じる
            If Not (webres Is Nothing) Then
                webres.Close()
            End If
            getHTTPStatusCode = statuscode
        End Try
    End Function


    Private Sub WebBrowser1_PreviewKeyDown(sender As Object, e As PreviewKeyDownEventArgs) Handles WebBrowser1.PreviewKeyDown
        'おまじない
        e.IsInputKey = True
        Debug.WriteLine(e.KeyCode)
        If e.KeyCode = Keys.Escape Then
            If Me.WindowState = FormWindowState.Maximized Then
                'カーソルを非表示にする 
                System.Windows.Forms.Cursor.Show()
                Me.WindowState = FormWindowState.Normal
                Me.FormBorderStyle = Windows.Forms.FormBorderStyle.Sizable
                Me.Width = 800
                Me.Height = 640
                Me.TopMost = False
            Else
                'カーソルを非表示にする 
                System.Windows.Forms.Cursor.Hide()
                Me.FormBorderStyle = FormBorderStyle.None
                Me.WindowState = FormWindowState.Maximized
                Me.TopMost = True
                Me.Focus()
            End If
        End If
    End Sub

    Private Sub browsPage(ByVal flag As Boolean)
        statuscode = checkLAN()
        Console.WriteLine("ステータスコード:" & statuscode)

        If flag AndAlso statuscode <> -1 AndAlso statuscode < 400 Then
            If brows_flg Then
                'webページを表示
                WebBrowser1.Url = New Uri(url)
                Console.WriteLine("send mail to administrator:brows page")
                sendMail("chanege online screen." + vbCrLf + "statuscode is " + statuscode + vbCrLf + "clock flag is " + flag.ToString)
                brows_flg = False
            End If
        Else
            If WebBrowser1.Url <> error_uri Then
                If statuscode <> -1 AndAlso My.Computer.Network.Ping(mailserver, 500) AndAlso sendmail_flg Then
                    'メール飛ばす処理
                    Console.WriteLine("send mail to administrator:error message")
                    sendMail("chanege offline screen." + vbCrLf + "statuscode is " + statuscode + vbCrLf + "clock flag is " + flag.ToString)
                End If
                '表示を変える処理
                Console.WriteLine("chanege screen in timer.")
                'WebBrowser1.Url = error_uri
                WebBrowser1.Navigate(error_uri)
                brows_flg = True
            End If
        End If
    End Sub

    Private Sub sendMail(mymessage As String)
        '送信者
        Dim senderMail As String = "solar-power-display@showa-aircraft.co.jp"
        '宛先
        Dim recipientMail As String = mail_address
        '件名
        Dim subject As String = "solar power display admin mail"
        '本文
        Dim body As String = mymessage

        'SmtpClientオブジェクトを作成する
        Dim sc As New System.Net.Mail.SmtpClient()
        'SMTPサーバーを指定する
        sc.Host = mailserver
        'ポート番号を指定する（既定値は25）
        sc.Port = 25 'メールを送信する

        Try
            sc.Send(senderMail, recipientMail, subject, body)
        Catch ex As Exception
            Console.WriteLine(ex)
        Finally
            '後始末（.NET Framework 4.0以降）
            sc.Dispose()
        End Try
    End Sub

    Private Sub importSetting()
        Dim settingFile As IO.StreamReader
        Dim sa As New ArrayList
        Dim stBuffer As String

        Debug.WriteLine("import Setting start.")
        Try
            settingFile = New IO.StreamReader(".\config.txt", System.Text.Encoding.GetEncoding("Shift-JIS"), False)
            While (settingFile.Peek() >= 0)
                stBuffer = settingFile.ReadLine()
                Debug.WriteLine(stBuffer)
                sa.Add(stBuffer)
            End While
            settingFile.Close()
        Catch ex As Exception
            Debug.WriteLine("file open error:")
            Debug.WriteLine(ex)
        End Try

        If sa.Count > 0 Then
            Dim setting_item() As String
            For Each setting_line As String In sa
                Debug.WriteLine(setting_line)
                ' 空行、行頭2文字が//の場合は次の行へ
                If setting_line.Length = 0 OrElse setting_line.Substring(0, 2) = "//" Then
                    Continue For
                End If
                setting_item = Split(setting_line, "::", 2, CompareMethod.Text)
                Select Case setting_item(0)
                    Case "url"         ' URL
                        url = setting_item(1)
                    Case "error_screen"         ' エラースクリーン
                        error_uri = New Uri(setting_item(1))
                    Case "debugIP"              ' デバッグ用IP
                        debugIP = setting_item(1)
                    Case "timerInterval"        ' タイマーインターバル
                        Timer1.Interval = setting_item(1)
                    Case "mailFlag"             ' send mail flag
                        sendmail_flg = setting_item(1)
                    Case "mailAddress"
                        mail_address = setting_item(1)
                    Case "startH"
                        start_h = setting_item(1)
                    Case "stopH"
                        stop_h = setting_item(1)
                    Case Else
                        ' それ以外
                        Console.WriteLine("不明な設定:" & setting_item(1))
                End Select
            Next
        End If
    End Sub

End Class

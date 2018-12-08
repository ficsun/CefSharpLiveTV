using CefSharp;
using CefSharp.WinForms;
using Open.WinKeyboardHook;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CefSharpLiveTV
{
    public partial class Form1 : Form
    {
        private const int SW_HIDE = 0;  //隐藏任务栏
        private const int SW_RESTORE = 9;//显示任务栏
        [DllImport("user32.dll")]
        public static extern int ShowWindow(int hwnd, int nCmdShow);
        [DllImport("user32.dll")]
        public static extern int FindWindow(string lpClassName, string lpWindowName);

        ChromiumWebBrowser chromeBrowser;
        MyRequestHandler myRequestHandler;
        LiveTVChannel liveTVChannel;
        private readonly IKeyboardInterceptor _interceptor;
        public Form1()
        {
            var settings = new CefSettings
            {
                LogSeverity = LogSeverity.Verbose,
                Locale = "zh-CN",
                AcceptLanguageList = "zh-CN",
                MultiThreadedMessageLoop = true,
                CachePath = System.AppDomain.CurrentDomain.BaseDirectory + @"\cache",
                PersistSessionCookies = true
            };
            settings.CefCommandLineArgs.Add("ppapi-flash-path", System.AppDomain.CurrentDomain.BaseDirectory + "plugins\\pepflashplayer64_32_0_0_101.dll"); //指定flash的版本，不使用系统安装的flash版本
            settings.CefCommandLineArgs.Add("ppapi-flash-version", "32_0_0_101");
            Cef.Initialize(settings);
            chromeBrowser = new ChromiumWebBrowser("about:blank");
            chromeBrowser.FrameLoadEnd += ChromeBrowser_FrameLoadEnd;
            myRequestHandler = new MyRequestHandler();
            chromeBrowser.RequestHandler = myRequestHandler;
            chromeBrowser.Dock = DockStyle.Fill;
            chromeBrowser.Visible = true;
            this.Controls.Add(chromeBrowser);
            InitializeComponent();
            label1.BringToFront();
            _interceptor = new KeyboardInterceptor();
            _interceptor.KeyDown += (sender, args) => Hook_KeyDown(sender, args);
        }
        private void Hook_KeyDown(object sender, KeyEventArgs e)
        {
            if (this.ContainsFocus)
            {
                switch (e.KeyCode)
                {
                    case Keys.Left://chs--
                        if (liveTVChannel.now > 0)
                        {
                            liveTVChannel.now--;
                        }
                        else
                        {
                            liveTVChannel.now = (byte)(liveTVChannel.size - 1);
                        }
                        this.Text = (liveTVChannel.now + 1).ToString() + liveTVChannel.name[liveTVChannel.now] + liveTVChannel.url[liveTVChannel.now];
                        label1.Text = this.Text;
                        label1.Visible = true;
                        timer1.Enabled = true;
                        chromeBrowser.Stop();
                        myRequestHandler.filter = false;
                        chromeBrowser.Load("about:blank");
                        chromeBrowser.Load(liveTVChannel.url[liveTVChannel.now]);
                        break;
                    case Keys.Right://chs++
                        if (liveTVChannel.now < (byte)(liveTVChannel.size - 1))
                        {
                            liveTVChannel.now++;
                        }
                        else
                        {
                            liveTVChannel.now = 0;
                        }
                        this.Text = (liveTVChannel.now + 1).ToString() + liveTVChannel.name[liveTVChannel.now] + liveTVChannel.url[liveTVChannel.now];
                        label1.Text = this.Text;
                        label1.Visible = true;
                        timer1.Enabled = true;
                        chromeBrowser.Stop();
                        myRequestHandler.filter = false;
                        chromeBrowser.Load("about:blank");
                        chromeBrowser.Load(liveTVChannel.url[liveTVChannel.now]);
                        break;
                    case Keys.Enter://全屏切换
                        if (this.FormBorderStyle == FormBorderStyle.None)
                        {
                            ShowWindow(FindWindow("Shell_TrayWnd", null), SW_RESTORE);
                            ShowWindow(FindWindow("Button", null), SW_RESTORE);
                            this.WindowState = FormWindowState.Normal;
                            this.FormBorderStyle = FormBorderStyle.Sizable;
                            this.WindowState = FormWindowState.Maximized;
                        }
                        else
                        {
                            ShowWindow(FindWindow("Shell_TrayWnd", null), SW_HIDE);
                            ShowWindow(FindWindow("Button", null), SW_HIDE);
                            this.WindowState = FormWindowState.Normal;
                            this.FormBorderStyle = FormBorderStyle.None;
                            this.WindowState = FormWindowState.Maximized;
                        }
                        break;
                    case Keys.F4://关闭
                        this.Close();
                        break;
                    default:
                        break;
                }
            }
        }
        private void ChromeBrowser_FrameLoadEnd(object sender, FrameLoadEndEventArgs e)
        {
            string script = @"
            (function() {
                document.body.style.backgroundColor='#000000';
                if (typeof(wsplayer)=='undefined')
                {
                    return;
                }
                for (j = 0; j < 8; j++)
                {
                    var div = document.getElementsByTagName('div');
                    for(i = 0; i < div.length; i++)
                    {
                        if(div[i].innerHTML.indexOf('WsPlayer.swf') == -1)
                        {
                            div[i].remove();
                        }
                    }
                }
                for (j = 0; j < 8; j++)
                {
                    var link = document.getElementsByTagName('link');
                    for(i = 0; i < link.length; i++)
                    {
                        link[i].remove();
                    }
                }
                wsplayer.style.marginTop = '0px';
                wsplayer.style.marginLeft = '0px';
                wsplayer.style.width = '" + (this.ClientSize.Width - 20).ToString() + @"px';
                wsplayer.style.height = '" + (this.ClientSize.Height - 20).ToString() + @"px';
            })();";
            chromeBrowser.ExecuteScriptAsync(script);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            label1.Text = "频道信息加载中……";
            liveTVChannel = new LiveTVChannel();
            if (!liveTVChannel.GetChannel())
            {
                this.Close();
                return;
            }
            else if(liveTVChannel.size == 0)
            {
                this.Close();
                return;
            }
            label1.Text = "频道信息加载完成";
            chromeBrowser.Load(liveTVChannel.url[liveTVChannel.now]);
            this.Text = (liveTVChannel.now + 1).ToString() + liveTVChannel.name[liveTVChannel.now] + liveTVChannel.url[liveTVChannel.now];
            label1.Text = this.Text;
            ShowWindow(FindWindow("Shell_TrayWnd", null), SW_HIDE);
            ShowWindow(FindWindow("Button", null), SW_HIDE);
            this.WindowState = FormWindowState.Normal;
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            _interceptor.StartCapturing();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Cef.Shutdown();
            _interceptor.StopCapturing();
            ShowWindow(FindWindow("Shell_TrayWnd", null), SW_RESTORE);
            ShowWindow(FindWindow("Button", null), SW_RESTORE);
            this.WindowState = FormWindowState.Normal;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.WindowState = FormWindowState.Maximized;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            string script = @"
            (function() {
                if (typeof(wsplayer)=='undefined')
                {
                    return 0;
                }
                var result = 1;
                var div = document.getElementsByTagName('div');
                for(i = 0; i < div.length; i++)
                {
                    if(div[i].innerHTML.indexOf('WsPlayer.swf') == -1)
                    {
                        div[i].remove();
                        result = 0;
                    }
                }
                return result;
            })();";
            Task<CefSharp.JavascriptResponse> task = chromeBrowser.EvaluateScriptAsync(script);
            task.ContinueWith(t =>
            {
                if (!t.IsFaulted)
                {
                    var response = t.Result;
                    if (response.Success == true)
                    {
                        int result = 0;
                        int.TryParse(response.Result.ToString(), out result);
                        if (result == 1)
                        {
                            label1.Visible = false;
                            timer1.Enabled = false;
                        }
                    }
                }
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            if (chromeBrowser.Created)
            { 
            string script = @"
            (function() {
                document.body.style.backgroundColor='#000000';
                if (typeof(wsplayer)=='undefined')
                {
                    return;
                }
                for (j = 0; j < 8; j++)
                {
                    var div = document.getElementsByTagName('div');
                    for(i = 0; i < div.length; i++)
                    {
                        if(div[i].innerHTML.indexOf('WsPlayer.swf') == -1)
                        {
                            div[i].remove();
                        }
                    }
                }
                for (j = 0; j < 8; j++)
                {
                    var link = document.getElementsByTagName('link');
                    for(i = 0; i < link.length; i++)
                    {
                        link[i].remove();
                    }
                }
                wsplayer.style.marginTop = '0px';
                wsplayer.style.marginLeft = '0px';
                wsplayer.style.width = '" + (this.ClientSize.Width - 20).ToString() + @"px';
                wsplayer.style.height = '" + (this.ClientSize.Height - 20).ToString() + @"px';
            })();";
            chromeBrowser.ExecuteScriptAsync(script);
            }
        }
    }
}

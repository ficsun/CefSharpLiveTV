using CefSharp;
using CefSharp.Handler;

namespace CefSharpLiveTV
{
    public class MyRequestHandler : DefaultRequestHandler
    {
        public bool filter = false;
        public override CefReturnValue OnBeforeResourceLoad(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, IRequestCallback callback)
        {
            if (request.ResourceType == ResourceType.PluginResource)
            {
                filter = true;
                return CefReturnValue.Continue;
            }
            else if ((!filter) && (request.ResourceType != ResourceType.Image) && (request.ResourceType != ResourceType.Stylesheet))
            {
                return CefReturnValue.Continue;
            }
            return CefReturnValue.Cancel;
        }
    }
}

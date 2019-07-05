using System.Web;
using System.Web.Optimization;

namespace MemoriesCollection
{
    public class BundleConfig
    {
        // 如需「搭配」的詳細資訊，請瀏覽 http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                         "~/Scripts/jquery-{version}.js",
                          "~/Scripts/jquery-ui-1.12.1.min.js",
                         "~/Scripts/jquery.lazy.min.js",
                         "~/Scripts/jquery.number.min.js",
                         "~/Scripts/jquery.batch.min.js",
                         "~/Scripts/jquery.scrollstop.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            bundles.Add(new ScriptBundle("~/bundles/WjJs").Include(
                         "~/Scripts/wj-tools.js"));

            bundles.Add(new ScriptBundle("~/bundles/ThirdPartyJs").Include(
                        "~/Scripts/select2.min.js",
                        "~/Scripts/lightgallery-all.min.js",// 相片輪播
                        "~/Scripts/lg-deletebutton.js",// 擴充刪除按鈕
                        "~/Scripts/lg-editbutton.js",// 擴充編輯按鈕
                        "~/Scripts/lg-setbgbutton.js",// 擴充設定背景按鈕
                        "~/Scripts/exif.js",// 相片Exif
                        "~/Scripts/ua-parser.js",// User-Agent 
                        "~/Scripts/input-upload.js",// 上傳按鈕
                        "~/Scripts/layer/layer.js",
                        "~/Scripts/menu.js"
                       ));

            // 使用開發版本的 Modernizr 進行開發並學習。然後，當您
            // 準備好實際執行時，請使用 http://modernizr.com 上的建置工具，只選擇您需要的測試。
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            //bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
            //          "~/Scripts/bootstrap.js",
            //          "~/Scripts/respond.js"));

            bundles.Add(new ScriptBundle("~/Content/WjCss").Include(
                       "~/Content/reset.css",
                       "~/Content/site.css",
                       "~/Content/button.css",
                       "~/Content/modal.css",
                       "~/Content/wj-gallery.css",
                       "~/Content/wj-video.css",
                       "~/Content/table.css",
                       "~/Content/block.css",
                       "~/Content/responsive.css"));

            bundles.Add(new ScriptBundle("~/Content/ThirdPartyCss").Include(
                        "~/Content/font-awesome.min.css",
                        "~/Content/css/select2.min.css",
                        "~/Content/lightgallery.min.css",
                        "~/Content/menu.css"));
        }
    }
}

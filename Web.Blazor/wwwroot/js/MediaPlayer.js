<script type="text/javascript">
    jQuery(function ($) {
            if(!(ej.browserInfo().name === "msie" && Number(ej.browserInfo().version) < 9)) {
                var isMobile = screen.width < 640 ? true : false;
                var width = "700px";
                var height = "400px";
                if (isMobile) {
        width = "100%";
                    height = "250px";
                }
                $("#basicPlayer").ejMediaPlayer({
        width: width,
                    height: height,
                    source: [
                        {
        "url": "http://files2.syncfusion.com/Videos/samples/Data+Driven+%231+Internet+of+Things.mp4",
                            "title": "Internet of things",
                            "author": "Syncfusion",
                            "posterUrl": "images/mediaplayer/internetthings.png"
                        },
                        {
        "url": "http://files2.syncfusion.com/Videos/samples/Data+Driven+%233+Measure+Everything.mp4",
                            "title": "Measure Everything",
                            "author": "Syncfusion",
                            "posterUrl": "images/mediaplayer/measure.png"
                        },
                        {
        "url": "http://files2.syncfusion.com/Videos/samples/Data+Driven+%234+Big+Data%2C+Big+Risks.mp4",
                            "title": "BigData Big Risks",
                            "author": "syncfusion",
                            "posterUrl": "images/mediaplayer/bigdata.png"
                        },
                         {
        "url": "http://files2.syncfusion.com/Videos/samples/Syncfusion+Dashboard+Designer.mp4",
                             "title": "Syncfusion Dashboard Designer",
                             "posterUrl": "images/mediaplayer/dbdesigner.png",
                             "author": "syncfusion"
                         },
                        {
        "url": "http://files2.syncfusion.com/Videos/samples/Syncfusion+Dashboard+SDK+and+Mobile+Application.mp4",
                            "title": "Syncfusion Dashboard SDK and Mobile Apllication",
                            "posterUrl": "images/mediaplayer/dashboard.png",
                            "author": "syncfusion"
                        },
                        {
        "url": "http://files2.syncfusion.com/Videos/samples/Syncfusion+Dashboard+Server.mp4",
                            "title": "Dashboard and Report Server",
                            "posterUrl": "images/mediaplayer/reportserver.png",
                            "author": "syncfusion"
                        },
                         {
        "url": "http://files2.syncfusion.com/Videos/samples/Fall+Madly+in+Love+with+10+Xamarin+Charts!+(February+2017+we.mp4",
                             "title": "Xamarin Charts",
                             "posterUrl": "images/mediaplayer/xamarinChart.png",
                             "author": "syncfusion"
                         },
                        {
        "url": " http://files2.syncfusion.com/Videos/samples/File-Format+manipulation+in+Xamarin.Forms.mp4",
                            "title": "Xamarin Forms",
                            "posterUrl": "images/mediaplayer/xamarinForms.png",
                            "author": "syncfusion"
                        },
                    ]
                });
            }
            else {
        alert("Media Player (HTML5) will not be supported in IE Version < 9");
            }
        });
    </script>
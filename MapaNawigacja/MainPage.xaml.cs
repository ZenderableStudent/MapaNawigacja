using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Services.Maps;
using Windows.UI.Xaml.Controls.Maps;
using Windows.Media.Devices;
using Windows.Devices.Geolocation;


// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace MapaNawigacja
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    /// 

   

    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            if (DaneGeograficzne.opisCelu == null)
                return;
            var pkt = new Geopoint(DaneGeograficzne.pktStartowy);
            MapIcon start = new MapIcon()
            {
                Location = pkt,
                Title = "Tu jesteś"
            };
            MapaNawigacja.MapElements.Add(start);
            var pktCelu = new Geopoint(DaneGeograficzne.pktDocelowy);
            MapaNawigacja.MapElements.Add(
                new MapIcon {
                    Title = DaneGeograficzne.opisCelu,
                    Location = pktCelu
                });
            Trasa(pkt, pktCelu);
            MapPolyline trasaLotem = new MapPolyline
            {
                StrokeColor = Windows.UI.Colors.Black,
                StrokeThickness = 3,
                StrokeDashed = true,
                Path = new Geopath(new List<BasicGeoposition> {DaneGeograficzne.pktStartowy,DaneGeograficzne.pktDocelowy}),
            };
            MapaNawigacja.MapElements.Add(trasaLotem);
            await MapaNawigacja.TrySetViewAsync(new Geopoint(DaneGeograficzne.pktStartowy), 8);

            var komunikat = new Windows.UI.Popups.MessageDialog(DaneGeograficzne.distance + " km", $"Odległość od Twojej lokalizacji do {DaneGeograficzne.opisCelu} to:");
            await komunikat.ShowAsync();
        }

        async void Trasa(Geopoint start, Geopoint stop)
        {
            var wynik = await MapRouteFinder.GetDrivingRouteAsync(start, stop);
            if (wynik.Status == MapRouteFinderStatus.Success)
            {
                var trasa = wynik.Route;
                var trasaNaMape = new MapRouteView(wynik.Route)
                {
                    RouteColor = Windows.UI.Colors.Blue
                };
                MapaNawigacja.Routes.Add(trasaNaMape);
                await MapaNawigacja.TrySetViewBoundsAsync(trasa.BoundingBox, new Thickness(25), MapAnimationKind.Bow);
            }
            else
            {
                var dlg = new Windows.UI.Popups.MessageDialog(wynik.Status.ToString(), DaneGeograficzne.opisCelu);
                await dlg.ShowAsync();
            }
        }

        private void powMapa(object sender, RoutedEventArgs e)
        {
            MapaNawigacja.ZoomLevel++;
        }

        private void pomMapa(object sender, RoutedEventArgs e)
        {
            MapaNawigacja.ZoomLevel--;
        }


        private void trybMapy(object sender, RoutedEventArgs e)
        {
            var ab = sender as AppBarButton;
            FontIcon ficon = new FontIcon
            {
                FontFamily = FontFamily.XamlAutoFontFamily
            };
            FontFamily = FontFamily.XamlAutoFontFamily;
            if (MapaNawigacja.Style == MapStyle.Road)
            {
                MapaNawigacja.Style = MapStyle.AerialWithRoads;
                ficon.Glyph = "M";
                ab.Label = "mapa";
                ab.Icon = ficon;
            }
            else
            {
                MapaNawigacja.Style = MapStyle.Road;
                ficon.Glyph = "S";
                ab.Label = "satelita";
                ab.Icon = ficon;
            }
        }

        private void koordynaty(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(Koordynaty));
        }
    }
}

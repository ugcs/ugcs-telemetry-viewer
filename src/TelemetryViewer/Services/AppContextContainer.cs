using Avalonia.Collections;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using UGCS.TelemetryViewer.ViewModels.Auxiliary;
using AppContext = UGCS.TelemetryViewer.Helpers.AppContext;

namespace UGCS.TelemetryViewer.Services
{
    public class AppContextContainer : IAppContextContainer
    {
        public class Options
        {
            public double DefaultWidth { get; set; }
            public double DefaultHeight { get; set; }
        }

        private readonly IStorageService _storageService;

        private readonly double _defaultWidth;
        private readonly double _defaultHeight;

        public Helpers.AppContext Context { get; private set; }

        public AppContextContainer(IOptions<Options> options, IStorageService storageService)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));
            if (options.Value == null)
                throw new ArgumentException("Options can not be null");

            _defaultHeight = options.Value.DefaultHeight;
            _defaultWidth = options.Value.DefaultWidth;
            _storageService = storageService ?? throw new ArgumentNullException(nameof(storageService));

            if (_storageService.TryLoadAppContext(out AppContext context))
            {
                Context = context;
            }
            else
            {
                Context = initDefaultContext();
            }

            TelemetryViewerApp.Current.Desktop.Exit += (s, e) => storeAppContext();
        }

        private void storeAppContext()
        {
            _storageService.StoreAppContext(Context);
        }

        private AppContext initDefaultContext()
        {
            AppContext context = new AppContext()
            {
                mainWindowWidth = _defaultWidth,
                mainWindowHeight = _defaultHeight,
                mainWindowPosition = new Avalonia.PixelPoint(0, 0),
                vehiclePlatesMap = new Dictionary<int, AvaloniaList<ITelemetryPlate>>()
            };
            return context;
        }

    }
}

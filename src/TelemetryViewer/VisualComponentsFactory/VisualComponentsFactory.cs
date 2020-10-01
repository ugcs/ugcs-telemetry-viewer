using System;
using System.Collections.Generic;
using UGCS.TelemetryViewer.ViewModels;
using UGCS.TelemetryViewer.ViewModels.Intrerfaces;
using UGCS.TelemetryViewer.Views;

namespace UGCS.TelemetryViewer
{
    public class VisualComponentsFactory : VisualComponentsFactoryBase
    {
        protected override Dictionary<Type, Type> ViewDataContextMap { get; } = new Dictionary<Type, Type>() {
            { typeof(InitializingSplashWindow),  typeof(IInitializingSplashWindowViewModel) },
            { typeof(HostWindow),  typeof(IHostWindowViewModel) },
            { typeof(MainWindow),  typeof(IMainWindowViewModel) },
            { typeof(LoginWindow),  typeof(ILoginWindowViewModel) },
            { typeof(CreatePlateWindow),  typeof(ICreatePlateWindowViewModel) },
        };
    }
}

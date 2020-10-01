using Avalonia.Controls;
using System;
using System.Collections.Generic;

namespace UGCS.TelemetryViewer
{
    public abstract class VisualComponentsFactoryBase : IVisualComponentsFactory
    {
        protected abstract Dictionary<Type, Type> ViewDataContextMap { get; }

        public T CreateUserControl<T>(bool withoutDataContext = false) where T : UserControl, new()
        {
            if (!withoutDataContext)
            {
                if (!ViewDataContextMap.TryGetValue(typeof(T), out Type viewModelType))
                    throw new NoViewModelBoundException(typeof(T));

                object viewModel;
                try
                {
                    viewModel = TelemetryViewerApp.ServiceProvider.GetService(viewModelType);
                }
                catch (Exception e)
                {
                    throw new ViewModelCreationException(viewModelType, e);
                }

                if (viewModel == null)
                    throw new ViewModelCreationException($"There is no injected {viewModelType} into DI container");

                return new T()
                {
                    DataContext = viewModel
                };
            }
            else
                return new T();
        }

        public T CreateWindow<T>(bool withoutDataContext = false) where T : Window, new()
        {
            if (!withoutDataContext)
            {
                if (!ViewDataContextMap.TryGetValue(typeof(T), out Type viewModelType))
                    throw new NoViewModelBoundException(typeof(T));

                var viewModel = TelemetryViewerApp.ServiceProvider.GetService(viewModelType);

                if (viewModel == null)
                    throw new ViewModelCreationException($"There is no injected {viewModelType} into DI container");

                return new T()
                {
                    DataContext = viewModel
                };
            }
            else
                return new T();
        }
    }
}

using Avalonia.Controls;

namespace UGCS.TelemetryViewer
{
    public interface IVisualComponentsFactory
    {
        /// <summary>
        /// Creates a window and set data context to it.
        /// </summary>
        /// <typeparam name="T">A type of the window you want to create</typeparam>
        /// <param name="withoutDataContext">Indicate that a window should be created without a view model</param>
        /// <returns>Returns created window.</returns>
        /// <exception cref="NoViewModelBoundException">There is no a data context bounded for this type into factory. Use <paramref name="withoutDataContext"/> to set data context manualy.</exception>
        /// <exception cref="ViewModelCreationException">An error is raised on aS data context creation.</exception>
        T CreateWindow<T>(bool withoutDataContext = false) where T : Window, new();

        /// <summary>
        /// Creates a user control and set data context to it.
        /// </summary>
        /// <typeparam name="T">A type of the user control you want to create</typeparam>
        /// <param name="withoutDataContext">Indicate that a control should be created without a view model</param>
        /// <returns>Rerurns created user control</returns>
        /// <exception cref="NoViewModelBoundException">There is no a data context bounded for this type into factory. Use <paramref name="withoutDataContext"/> to set data context manualy.</exception>
        /// <exception cref="ViewModelCreationException">An error is raised on a data context creation.</exception>
        T CreateUserControl<T>(bool withoutDataContext = false) where T : UserControl, new();

    }
}

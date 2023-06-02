using AnySoftMobile.Utils;
using AnySoftMobile.Utils.Dialogs;
using AnySoftMobile.ViewModels;
using AnySoftMobile.Views;
using Autofac;
using Autofac.Extras.CommonServiceLocator;
using CommonServiceLocator;
using Xamarin.Forms;

namespace AnySoftMobile.Core
{
    public abstract class AppContainer
    {
        public void Setup()
        {
            var containerBuilder = new ContainerBuilder();

            RegisterServices(containerBuilder);

            var container = containerBuilder.Build();

            ServiceLocator.SetLocatorProvider(() => new AutofacServiceLocator(container));

            container.BeginLifetimeScope();
        }

        protected virtual void RegisterServices(ContainerBuilder containerBuilder)
        {
            containerBuilder.RegisterType<App>().SingleInstance();

            containerBuilder.RegisterType<NavigationService>().As<INavigationService>().SingleInstance();
            containerBuilder.RegisterType<JobDialogService>().As<IJobDialogService>().InstancePerDependency();
            
            containerBuilder.RegisterType<SingleProductView>().Named<Page>(ViewNames.SingleProductView).As<SingleProductView>().InstancePerDependency();
            containerBuilder.RegisterType<DashboardView>().Named<Page>(ViewNames.DashboardView).As<DashboardView>().InstancePerDependency();
            containerBuilder.RegisterType<MainView>().Named<Page>(ViewNames.MainView).As<MainView>().InstancePerDependency();
            containerBuilder.RegisterType<MultipleProductsView>().Named<Page>(ViewNames.MultipleProductsView).As<MultipleProductsView>().InstancePerDependency();
            containerBuilder.RegisterType<CartView>().Named<Page>(ViewNames.CartView).As<CartView>().InstancePerDependency();
            containerBuilder.RegisterType<LibraryView>().Named<Page>(ViewNames.LibraryView).As<LibraryView>().InstancePerDependency();
            containerBuilder.RegisterType<LoginView>().Named<Page>(ViewNames.LoginView).As<LoginView>().InstancePerDependency();
            containerBuilder.RegisterType<ProfileView>().Named<Page>(ViewNames.ProfileView).As<ProfileView>().InstancePerDependency();
            
            containerBuilder.RegisterType<MultipleProductsViewModel>().InstancePerDependency();
            containerBuilder.RegisterType<CartViewModel>().InstancePerDependency();
            containerBuilder.RegisterType<LibraryViewModel>().InstancePerDependency();
            containerBuilder.RegisterType<SingleProductViewModel>().InstancePerDependency();
            containerBuilder.RegisterType<DashboardViewModel>().InstancePerDependency();
            containerBuilder.RegisterType<MainViewModel>().InstancePerDependency();
            containerBuilder.RegisterType<LoginViewModel>().InstancePerDependency();
            containerBuilder.RegisterType<ProfileViewModel>().InstancePerDependency();
        }
    }
}
﻿using System;
using AnySoftMobile.ViewModels;
using Xamarin.Forms;

namespace AnySoftMobile.Views
{
    public abstract class BaseView<TViewModel> : ContentPage where TViewModel : BaseViewModel
    {
        protected TViewModel ViewModel { get; }

        protected BaseView()
        {
            ViewModel = CommonServiceLocator.ServiceLocator.Current.GetInstance<TViewModel>();
            BindingContext = ViewModel;
        }
        
        protected override void OnAppearing()
        {
            base.OnAppearing();
            ViewModel.OnViewAppearing(this, EventArgs.Empty);
        }
    }
}
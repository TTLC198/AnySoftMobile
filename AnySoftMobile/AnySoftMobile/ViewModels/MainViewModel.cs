using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using AnySoftMobile.Utils.Dialogs;
using AnySoftMobile.Views;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using XF.Material.Forms.Models;

namespace AnySoftMobile.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly IJobDialogService _dialogService;

        private ObservableCollection<TestModel> _models;
        public ObservableCollection<TestModel> Models
        {
            get => _models;
            set => Set(ref _models, value);
        }

        private List<int> _selectedFilters;
        public List<int> SelectedFilters
        {
            get => _selectedFilters;
            set => Set(ref _selectedFilters, value);
        }
        
        public MainViewModel(IJobDialogService dialogService)
        {
            _dialogService = dialogService;

            Models = new ObservableCollection<TestModel>()
            {
                new TestModel
                {
                    Title = "Mobile Developer (Xamarin)",
                    Id = Guid.NewGuid().ToString("N"),
                },
                new TestModel
                {
                    Title = "Mobile Developer (Native Android)",
                    Id = Guid.NewGuid().ToString("N")
                },
                new TestModel
                {
                    Title = "Mobile Developer (Native iOS)",
                    Id = Guid.NewGuid().ToString("N")
                }
            };
            SelectedFilters = new List<int>();
        }

        private async Task ListMenuSelected(MaterialMenuResult s)
        {
            switch (s.Index)
            {
                case 0:
                    {
                        var result = await _dialogService.AddNewJob();

                        if (Models.Any(m => m.Title == result))
                        {
                            await _dialogService.AlertExistingJob(result);
                        }
                        else if (!string.IsNullOrEmpty(result))
                        {
                            Models.Where(m => m.IsNew).ForEach(m => m.IsNew = false);

                            var model = new TestModel
                            {
                                Title = result,
                                Id = Guid.NewGuid().ToString("N"),
                                IsNew = true
                            };

                            Models.Add(model);
                        }

                        break;
                    }
                case 1:
                    Models = new ObservableCollection<TestModel>(Models.OrderBy(m => m.Title));
                    break;
                case 2:
                    SelectedFilters = new List<int>();
                    break;
            }
        }

        private async Task ViewItemSelected(string id)
        {
            var selectedModel = Models.FirstOrDefault(m => m.Id == id);

            await Navigation.PushAsync(ViewNames.SingleProductView, selectedModel);
        }

        private async Task MenuSelected(MaterialMenuResult i)
        {
            var model = Models.FirstOrDefault(m => m.Title == (string)i.Parameter);

            switch (i.Index)
            {
                case 0:
                    {
                        if (model != null)
                        {
                            var result = await _dialogService.EditJob(model.Title);

                            if (!string.IsNullOrEmpty(result))
                            {
                                model.Title = result;
                            }
                        }

                        break;
                    }
                case 1:
                    {
                        if (model != null)
                        {
                            var confirmed = await _dialogService.DeleteJob(model.Title);

                            if (confirmed == true)
                            {
                                Models.Remove(model);

                                await _dialogService.JobDeleted();
                            }
                        }

                        break;
                    }
            }
        }
    }

    public class TestModel : PropertyChangeAware
    {
        private bool _isNew;
        private string _id;
        private string _title;

        public string Id
        {
            get => _id;
            set => Set(ref _id, value);
        }

        public string Title
        {
            get => _title;
            set => Set(ref _title, value);
        }

        public bool IsNew
        {
            get => _isNew;
            set => Set(ref _isNew, value);
        }
    }
}
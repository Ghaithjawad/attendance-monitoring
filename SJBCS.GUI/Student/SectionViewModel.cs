﻿using AMS.Utilities;
using MaterialDesignThemes.Wpf;
using SJBCS.Data;
using SJBCS.GUI.Dialogs;
using SJBCS.Services.Repository;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Unity;

namespace SJBCS.GUI.Student
{
    public class SectionViewModel : BindableBase
    {
        private ILevelsRepository _levelsRepository;
        private ISectionsRepository _sectionsRepository;
        private IStudentsRepository _studentsRepository;

        private bool _addEditMode;

        public bool AddEditMode
        {
            get { return _addEditMode; }
            set { SetProperty(ref _addEditMode, value); }
        }

        private AddEditSectionViewModel addEditSectionViewModel;
        public AddEditSectionViewModel AddEditSectionViewModel
        {
            get { return addEditSectionViewModel; }
            set { SetProperty(ref addEditSectionViewModel, value); }
        }

        private ObservableCollection<Level> _levels;
        public ObservableCollection<Level> Levels
        {
            get { return _levels; }
            set { SetProperty(ref _levels, value); }
        }

        private ObservableCollection<Section> _sections;
        public ObservableCollection<Section> Sections
        {
            get { return _sections; }
            set { SetProperty(ref _sections, value); }
        }

        private Guid _selectedLevelId;
        public Guid SelectedLevelId
        {
            get { return _selectedLevelId; }
            set
            {
                if (value != null)
                {
                    Sections = new ObservableCollection<Section>(_sectionsRepository.GetSections(value));
                    if (Sections.Any())
                        SelectedSectionId = Sections[0].SectionID;
                    else
                        SelectedSection = new Section();
                }
                SetProperty(ref _selectedLevelId, value);
                EditCommand.RaiseCanExecuteChanged();
                DeleteCommand.RaiseCanExecuteChanged();
            }
        }

        private Guid _selectedSectionId;
        public Guid SelectedSectionId
        {
            get { return _selectedSectionId; }
            set
            {
                if (value != null)
                    SelectedSection = _sectionsRepository.GetSection(value);
                SetProperty(ref _selectedSectionId, value);
            }
        }

        private Section _selectedSection;
        public Section SelectedSection
        {
            get
            {
                return _selectedSection;
            }

            set
            {
                SetProperty(ref _selectedSection, value);
            }
        }

        public RelayCommand AddCommand { get; private set; }
        public RelayCommand<Section> DeleteCommand { get; private set; }
        public RelayCommand<Section> EditCommand { get; private set; }

        public event Action<Section> AddRequested = delegate { };
        public event Action<Section> EditRequested = delegate { };

        public SectionViewModel(ILevelsRepository levelsRepository, ISectionsRepository sectionsRepository, IStudentsRepository studentsRepository)
        {
            _levelsRepository = levelsRepository;
            _sectionsRepository = sectionsRepository;
            _studentsRepository = studentsRepository;

            addEditSectionViewModel = ContainerHelper.Container.Resolve<AddEditSectionViewModel>();

            EditCommand = new RelayCommand<Section>(OnEdit, CanEdit);
            AddCommand = new RelayCommand(OnAdd);
            DeleteCommand = new RelayCommand<Section>(OnDelete, CanDelete);

            AddRequested += NavToAddSection;
            EditRequested += NavToEditSection;
            addEditSectionViewModel.Done += NavToSection;
            Initialize();
        }

        private bool CanDelete(Section section)
        {
            if (!Sections.Any())
                return false;
            return true;
        }

        private bool CanEdit(Section section)
        {
            if (!Sections.Any())
                return false;
            return true;
        }

        private void NavToSection()
        {

            SelectedSection = null;
            AddEditMode = false;
            PopulateLevelComboBox();
            AddEditSectionViewModel.Initialize();
            AddEditSectionViewModel = addEditSectionViewModel;
        }

        private void NavToEditSection(Section section)
        {
            addEditSectionViewModel.EditMode = true;
            addEditSectionViewModel.SetStudent(section);
            addEditSectionViewModel.Initialize();
            AddEditMode = true;
            AddEditSectionViewModel = addEditSectionViewModel;
        }

        private void NavToAddSection(Section section)
        {
            addEditSectionViewModel.EditMode = false;
            addEditSectionViewModel.SetStudent(section);
            addEditSectionViewModel.Initialize();
            AddEditMode = true;
            AddEditSectionViewModel = addEditSectionViewModel;
        }

        private async void OnDelete(Section section)
        {
            try
            {
                _sectionsRepository.DeleteSection(section.SectionID);
                NavToSection();
                EditCommand.RaiseCanExecuteChanged();
                DeleteCommand.RaiseCanExecuteChanged();
            }
            catch
            {
                var view = new DialogBoxView
                {
                    DataContext = new DialogBoxViewModel(MessageType.Informational, "You cannot delete an active section.")
                };

                //show the dialog
                var result = await DialogHost.Show(view, "RootDialog", ClosingEventHandler);
            }
        }

        private void OnAdd()
        {
            AddRequested(new Section { LevelID = SelectedLevelId, SectionID = Guid.NewGuid() });
        }

        private void OnEdit(Section section)
        {
            EditRequested(section);
        }

        public void Initialize()
        {
            PopulateLevelComboBox();
        }

        private void PopulateLevelComboBox()
        {
            Levels = new ObservableCollection<Level>(_levelsRepository.GetLevels());
            if (Levels.Any())
            {
                if (SelectedLevelId == new Guid())
                    SelectedLevelId = Levels[0].LevelID;
                Sections = new ObservableCollection<Section>(_sectionsRepository.GetSections(SelectedLevelId));
                if (Sections.Any())
                    SelectedSectionId = Sections[0].SectionID;
            }
            OnPropertyChanged("SelectedLevelId");
            OnPropertyChanged("SelectedSectionId");
        }
    }
}

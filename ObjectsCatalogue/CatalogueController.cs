using System;
using System.Collections.Generic;
using System.Linq;
using Planner.CommonEditor.Objects;
using Planner.CommonEditor.ObjectsCategories;
using Planner.Utils;
using UniRx;
using UnityEngine;
using Zenject;

namespace Planner.MainEditor.ObjectsCatalogue
{
    /*! \defgroup mainEditorObjectCatalog Модуль каталога объектов 
       \brief Данный модуль описывает совокупность классов, отвечающих за инициализацию объектов для инжектирования
    
       Модуль инициализации объектов для инжектирования. Описывает совокупность классов, отвечающих за инициализацию объектов для инжектирования 
       
       \ingroup mainEditor
   */
    /*!
        \ingroup  mainEditorObjectCatalog
    */
    /// <summary>
    /// Класс реализующий работу каталога объектов
    /// </summary>
    public class CatalogueController : IInitializable, IDisposable
    {
        private const string BACK_BUTTON_NAME = "Назад";
        private const int UNPUBLISHED_CATEGORY_ID = 1;

        [Inject]
        private CategoriesView categoriesView;

        [Inject]
        private ObjectsView objectsView;

        [Inject]
        private IObjectsProvider objectsProvider;

        [Inject]
        private IObjectCategoriesProvider objectCategoriesProvider;

        [Inject]
        private ObjectDragProcessorFactory objectDragProcessorFactory;

        [Inject]
        private SearchCatalogModel searchModel;

        [Inject]
        private SearchCatalogView searchView;

        private readonly CompositeDisposable compositeDisposable = new CompositeDisposable();
        private readonly CompositeDisposable objectsViewDisposables = new CompositeDisposable ();

        // Словарь [id категории "Все", список id категорий группы]
        private readonly Dictionary<int, int[]> categoriesAll = new Dictionary<int, int[]>();

        /// <summary>
        /// При инициализации создаем представления для категорий объектов и самих объектов и подписываемся на необходимые события
        /// </summary>
        public void Initialize ()
        {
            var id = 0;

            objectCategoriesProvider
                .Do(group =>
                {
                    if (!group.Categories.Exists(x => x.Id == id))
                    {
                        group.Categories.Add(new ObjectCategory(id, "Все", false)); // Добавляем категорию "Все"
                    }
                    categoriesAll.Add(id--, group.Categories.Select(x => x.Id).ToArray());
                });

            var model = new CategoriesView.Model (
                objectCategoriesProvider
                    .Where (group => group.Id != UNPUBLISHED_CATEGORY_ID) // It was an order: hardcode 
                    .Select (group => 
                        new CategoryGroupView.Model (
                            group.Id,
                            group.Name,
                            group.Categories.Select (category => 
                                new CategoryView.Model (
                                    category.Id,
                                    category.Name,
                                    GetObjectsFromCategoryCount(category.Id),
                                    false)).ToArray (),
                            false))
                    .ToArray ());

            categoriesView.SetModel (model);

            categoriesView
                .CategoryClickedObservable
                .Subscribe(CategoryClicked)
                .AddTo(compositeDisposable);

            objectsView
                .BackClickAsObservable
                .Subscribe(_ => ObjectsBackClicked())
                .AddTo(compositeDisposable);

            searchModel
                .SearchStringAsObservable
                .Subscribe (SearchEdited)
                .AddTo(compositeDisposable);

            objectsView.Deactivate ();
        }

        private int GetObjectsFromCategoryCount(int categoryId)
        {
            if (categoriesAll.ContainsKey(categoryId))
            {
                return objectsProvider.GetObjectsFromCategoriesCount(categoriesAll[categoryId]);
            }
            else
            {
                return objectsProvider.GetObjectsFromCategoryCount(categoryId);
            }
        }

        public void Dispose ()
        {
            compositeDisposable.Dispose();
            categoriesAll.Clear();
        }

        /// <summary>
        /// Возвращаем представление объекта в каталоге по его модели данных
        /// </summary>
        /// <param name="objectModel">модель данных объекта</param>
        /// <returns></returns>
        private ObjectView.Model GetObjectViewModel (ObjectModel objectModel)
        {
            Sprite sprite = objectModel
                .CatalogueThumbnail
                .Value
                ?.ToSprite ();

            return new ObjectView.Model (
                objectModel.Id,
                objectsProvider.GetObjectNameById (objectModel.Id),
                sprite);
        }

        /// <summary>
        /// Возвращаем модель данных объекта по его идентификатору
        /// </summary>
        /// <param name="id">уникальный идентификатор объекта</param>
        /// <returns>модель данных объекта</returns>
        private ObjectModel GetObjectById (int id)
        {
            return objectsProvider.GetObjectById (id);
        }

        /// <summary>
        /// Обработчик события начала перетаскивания объекта.
        /// </summary>
        /// <param name="dragObservable">подписка на событие перетаскивания объекта</param>
        private void ObjectDragBegan (ObjectView.DragObservable dragObservable)
        {
            var objectModel = GetObjectById (dragObservable.Model.Id);

            objectDragProcessorFactory
                .Create (objectModel)
                ?.ObjectDragBegan (dragObservable, objectModel);
        }

        /// <summary>
        /// При клике на категорию отображаем содержащиеся в ней объекты
        /// </summary>
        /// <param name="category">модель данных категории объектов</param>
        private void CategoryClicked (CategoryView.Model category)
        {
            ObjectsView.Model objectsViewModel;

            if (categoriesAll.ContainsKey(category.Id))
            {
                objectsViewModel = new ObjectsView.Model(
                    category.Name,
                    objectsProvider
                        .GetObjectsFromCategories(categoriesAll[category.Id])
                        .Select(GetObjectViewModel)
                        .ToArray());
            }
            else
            {
                objectsViewModel = new ObjectsView.Model(
                    category.Name,
                    objectsProvider
                        .GetObjectsFromCategory(category.Id)
                        .Select(GetObjectViewModel)
                        .ToArray());
            }

            ShowObjectsView (objectsViewModel);
        }

        /// <summary>
        /// При клике на кнопку Назад в категории объектов возвращаемся к списку категорий
        /// </summary>
        private void ObjectsBackClicked ()
        {
            categoriesView.Activate ();
            objectsView.Deactivate ();
            objectsViewDisposables.Clear ();
            searchView.Clear ();
        }

        private void SearchEdited (string value)
        {
            if (searchModel.NeedSearch) {
                ShowObjectsView (
                    new ObjectsView.Model (
                        BACK_BUTTON_NAME,
                        objectsProvider
                            .GetObjectsByName (searchModel, UNPUBLISHED_CATEGORY_ID)
                            .Select (GetObjectViewModel)
                            .ToArray ()));
            } else if (!categoriesView.IsActiveSelf ()) {
                ObjectsBackClicked ();
            }
        }

        /// <summary>
        /// Отображаем содержимое категории объектов 
        /// </summary>
        /// <param name="model">модель данных списка объектов в категории </param>
        private void ShowObjectsView (ObjectsView.Model model)
        {
            categoriesView.Deactivate ();
            objectsView.Activate ();

            objectsView.SetModel (model);

            objectsView
                .OnDragObservable
                .Subscribe (ObjectDragBegan)
                .AddTo (objectsViewDisposables);
        }
    }
}
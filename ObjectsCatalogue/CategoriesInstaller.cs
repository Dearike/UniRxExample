using Planner.CommonEditor.UI;
using Planner.MainEditor.Installers;
using UnityEngine;
using Zenject;

namespace Planner.MainEditor.ObjectsCatalogue
{
    /*!
        \ingroup  mainEditorObjectCatalog
    */
    /// <summary>
    /// Класс отвечающий за инициализацию объектов инжектируемых в классы работающие с категориями объектов в каталоге
    /// </summary>
    public class CategoriesInstaller : MonoInstaller
    {
        [SerializeField]
        private CategoryGroupView groupPrefab;

        [SerializeField]
        private CategoryView categoryPrefab;

        [SerializeField]
        private CategoriesView view;

        [SerializeField]
        private SearchCatalogView searchView;

        [SerializeField]
        private ObjectsView objectsView;

        [SerializeField]
        private ObjectView objectPrefab;

        [SerializeField]
        private Palette palette;

        [SerializeField]
        private ObjectPreview objectPreview;

        public override void InstallBindings ()
        {
            ObjectsCatalogueDragProcessorsInstaller.Install (Container);

            Container
                .Bind<ObjectPreview> ()
                .FromInstance (objectPreview);

            Container
                .Bind<CategoriesView> ()
                .FromInstance (view);

            Container
                .Bind<SearchCatalogView> ()
                .FromInstance (searchView);

            Container
                .Bind<SearchCatalogModel> ()
                .AsSingle ();

            Container
                .Bind<ObjectsView> ()
                .FromInstance (objectsView);

            Container
                .BindMemoryPool<CategoryGroupView, CategoryGroupView.Pool> ()
                .FromComponentInNewPrefab (groupPrefab);

            Container
                .BindMemoryPool<CategoryView, CategoryView.Pool> ()
                .FromComponentInNewPrefab (categoryPrefab);

            Container
                .BindInterfacesAndSelfTo<CatalogueController> ()
                .AsSingle ();

            Container
                .Bind<CategoryGroupView.Settings> ()
                .FromInstance (new CategoryGroupView.Settings (palette.SelectedTextColor, palette.StandardTextColor));

            Container
                .Bind<CategoryView.Settings>()
                .FromInstance(new CategoryView.Settings(palette.SelectedTextColor, palette.StandardTextColor));

            Container
                .BindMemoryPool<ObjectView, ObjectView.Pool> ()
                .FromComponentInNewPrefab (objectPrefab);
        }
    }
}
using Planner.CommonEditor.Objects;

namespace Planner.MainEditor.ObjectsCatalogue
{
    /*!
        \ingroup  mainEditorObjectCatalog
    */
    /// <summary>
    /// Интерфейс для реализации классов-обработчиков событий перетаскивания объекта из каталога на сцену 
    /// </summary>
    public interface IObjectDragProcessor
    {
        void ObjectDragBegan (ObjectView.DragObservable dragObservable, ObjectModel objectModel);
    }
}
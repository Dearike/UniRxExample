using Planner.CommonEditor.Objects;
using Planner.CommonEditor.Tools;
using Planner.Geometry;
using Planner.MainEditor.Commands;
using Planner.MainEditor.Tools.Selection2D;
using UnityEngine.EventSystems;
using Zenject;

namespace Planner.MainEditor.ObjectsCatalogue
{
    /*!
        \ingroup  mainEditorObjectCatalog
    */
    /// <summary>
    /// Класс, отвечающий за обработку событий перетаскивания объектов из каталога на сцену в режиме 2D
    /// </summary>
    public class StandardObjectDragProcessor2D : AbstractStandardObjectDragProcessor
    {
        [Inject (Id = ID.DotsWallsPlane)]
        private Plane dotsWallsPlane;

        [Inject]
        private SelectionTool selectionTool;

        [Inject]
        private ToolsManager toolsManager;

        private AddObjectCommand addObjectCommand;

        /// <summary>
        /// Ничего не делаем в начале перетаскивания, все уже сделано в абстрактном классе
        /// </summary>
        /// <param name="objectModel">модель данных перетаскиваемого объекта</param>
        protected override void HandleObjectDragBegan (ObjectModel objectModel)
        {
        }

        /// <summary>
        /// В процессе перетаскивания создаем команду добавления объекта на сцену, а если она уже создана, то меняем позицию объекта
        /// </summary>
        /// <param name="pointerEventData">Данные события указателя</param>
        /// <param name="objectModel">модель данных перетаскиваемого объекта</param>
        protected override void HandleObjectDrag (PointerEventData pointerEventData, ObjectModel objectModel)
        {
            dotsWallsPlane.Raycast (pointerEventData.position, out var planePosition);
            planePosition = planePosition
                .FlipYZ ()
                .WithY (0f);

            if (addObjectCommand == null) {
                addObjectCommand = new AddObjectCommand (
                    objectModel,
                    planePosition,
                    objectInstanceModelPool,
                    apartmentModelProvider,
                    apartmentModelProvider.Apartment.FloorIndex.Value);
                undoRedoStack.Execute (addObjectCommand);
                objectPreview.Hide ();
            } else {
                addObjectCommand
                    .ObjectInstanceModel
                    .Position
                    .Value = planePosition;
            }
        }

        /// <summary>
        /// По завершении перетаскивания объекта выделяем объект на сцене и запоминаем финальное состояние в команде добавления объекта
        /// </summary>
        protected override void HandleObjectDragCompleted ()
        {
            toolsManager.Add (selectionTool);

            addObjectCommand?.RecordFinalState ();
            addObjectCommand = null;
        }
    }
}
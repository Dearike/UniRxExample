using Planner.CommonEditor.Objects;
using Planner.CommonEditor.Tools;
using Planner.MainEditor.Commands;
using Planner.MainEditor.Tools.Selection3D;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

namespace Planner.MainEditor.ObjectsCatalogue
{
    /*!
        \ingroup  mainEditorObjectCatalog
    */
    /// <summary>
    /// Класс, отвечающий за обработку событий перетаскивания объектов из каталога на сцену в режиме 3D
    /// </summary>
    public class StandardObjectDragProcessor3D : AbstractStandardObjectDragProcessor
    {
        [Inject (Id = ID.GroundPlane)]
        private Plane groundPlane;

        [Inject]
        private TranslateTool translateTool;

        private ObjectModel _objectModel;
        private AddObjectCommand addObjectCommand;

        /// <summary>
        /// Ничего не делаем в начале перетаскивания, все уже сделано в абстрактном классе
        /// </summary>
        /// <param name="objectModel">модель данных перетаскиваемого объекта</param>
        protected override void HandleObjectDragBegan (ObjectModel objectModel)
        {
            _objectModel = objectModel;
        }

        /// <summary>
        /// В процессе перетаскивания создаем команду добавления объекта на сцену, а если она уже создана, то меняем позицию объекта
        /// </summary>
        /// <param name="pointerEventData">Данные события указателя</param>
        /// <param name="objectModel">модель данных перетаскиваемого объекта</param>
        protected override void HandleObjectDrag (PointerEventData pointerEventData, ObjectModel objectModel)
        {
            if (!groundPlane.Raycast (pointerEventData.position, out var planePosition))
                return;

            if (addObjectCommand == null)
            {
                addObjectCommand = new AddObjectCommand (
                    objectModel,
                    planePosition,
                    objectInstanceModelPool,
                    apartmentModelProvider,
                    apartmentModelProvider.Apartment.FloorIndex.Value);
                undoRedoStack.Execute (addObjectCommand);
                objectPreview.Hide ();
            } else
            {
                addObjectCommand
                    .ObjectInstanceModel
                    .Position
                    .Value = planePosition;

                objectModel.Layer.Value = Layer.DraggedObject3D;
            }
        }

        /// <summary>
        /// По завершении перетаскивания объекта запоминаем финальное состояние в команде добавления объекта
        /// </summary>
        protected override void HandleObjectDragCompleted ()
        {
            _objectModel.Layer.Value = Layer.ObjectView;
            addObjectCommand?.RecordFinalState ();
            addObjectCommand = null;
        }
    }
}
using Planner.CommonEditor.Objects;
using Planner.MainEditor.View2D;
using UnityEngine;
using UnityEngine.Assertions;

namespace Planner.MainEditor.ObjectsCatalogue
{
    /*!
        \ingroup  mainEditorObjectCatalog
    */
    /// <summary>
    /// Класс, реализующий перетаскивание объекта свободной формы из каталога на сцену в режиме 2D
    /// </summary>
    public class FreeShapeObjectDragProcessor2D : StandardObjectDragProcessor2D
    {
/*        protected override ObjectInstanceModel CreateObjectInstance (ObjectModel objectModel, Vector3 position)
        {
            var freeShapeObject = objectModel as FreeShapeObjectModel;
            Assert.IsNotNull (freeShapeObject);

            var newFreeShapeObject = freeShapeObject.Clone ();
            var objectInstance = base.CreateObjectInstance (newFreeShapeObject, position);

            return objectInstance;
        }*/
    }
}
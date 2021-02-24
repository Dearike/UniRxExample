using Planner.CommonEditor.Objects;
using Planner.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace Planner.MainEditor.ObjectsCatalogue
{
    /*!
        \ingroup  mainEditorObjectCatalog
    */
    /// <summary>
    /// Класс реализующий миниатюру объекта, используемую при перетаскивании
    /// </summary>
    public class ObjectPreview : MonoBehaviour
    {
        [SerializeField]
        private Image image;

        public bool IsVisible => this.IsActiveSelf ();


        private void Start ()
        {
            Hide ();
        }

        /// <summary>
        /// Отображаем миниатюру объекта
        /// </summary>
        /// <param name="objectModel">модель данных перетаскиваемого объекта</param>
        /// <param name="screenPosition">Позиция появления миниатюры на сцене</param>
        public void Display (ObjectModel objectModel, Vector2 screenPosition)
        {
            this.Activate ();
            image.overrideSprite = objectModel
                .CatalogueThumbnail
                .Value
                ?.ToSprite ();

            Move (screenPosition);
        }

        /// <summary>
        /// Перемещаем миниатюру в заданную точку
        /// </summary>
        /// <param name="screenPosition">координаты тыочки, куда необходимо переместить миниатюру</param>
        public void Move (Vector2 screenPosition)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle (
                transform.parent.GetRectTransform (),
                screenPosition,
                null,
                out var local);

            this
                .GetRectTransform ()
                .anchoredPosition = local;
        }

        /// <summary>
        /// Скрываем миниатюру 
        /// </summary>
        public void Hide ()
        {
            this.Deactivate ();
        }
    }
}
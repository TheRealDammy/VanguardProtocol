using UnityEngine;
using UnityEngine.UIElements;

namespace VanguardProtocol.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class CrosshairUI : MonoBehaviour
    {
        private void Awake()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;

            var crosshair = new VisualElement();

            crosshair.style.position = Position.Absolute;
            crosshair.style.width = 12;
            crosshair.style.height = 12;
            crosshair.style.left = Length.Percent(50);
            crosshair.style.top = Length.Percent(50);
            crosshair.style.marginLeft = -6;
            crosshair.style.marginTop = -6;
            crosshair.style.borderTopWidth = 2;
            crosshair.style.borderBottomWidth = 2;
            crosshair.style.borderLeftWidth = 2;
            crosshair.style.borderRightWidth = 2;
            crosshair.style.borderTopColor = Color.white;
            crosshair.style.borderBottomColor = Color.white;
            crosshair.style.borderLeftColor = Color.white;
            crosshair.style.borderRightColor = Color.white;

            root.Add(crosshair);
        }
    }
}

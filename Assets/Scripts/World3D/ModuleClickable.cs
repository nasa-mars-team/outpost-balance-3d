using UnityEngine;
using UnityEngine.EventSystems;
using OutpostBalance.Core;
using OutpostBalance.Gameplay;

namespace OutpostBalance.World3D
{
    /// <summary>
    /// Allows clicking 3D outpost buildings in the scene to boost or cycle their power allocation.
    /// Requires PhysicsRaycaster on MainCamera.
    /// </summary>
    public class ModuleClickable : MonoBehaviour, IPointerClickHandler
    {
        public OutpostSubsystem subsystem;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) return;
            var gm = GameManager.Instance;
            if (gm != null)
            {
                gm.CycleSubsystemFrom3D(subsystem);
            }
        }
    }
}

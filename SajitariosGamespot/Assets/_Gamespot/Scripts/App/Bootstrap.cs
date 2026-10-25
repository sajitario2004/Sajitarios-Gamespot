using Gamespot.Core.Runtime.UI;
using UnityEngine;

namespace Gamespot.App
{
    /// <summary>
    /// Punto de entrada de la app (composition root): vive en el objeto <c>AppRoot</c> de la escena
    /// <c>App</c>, prepara los servicios y monta la UI.
    /// </summary>
    public sealed class Bootstrap : MonoBehaviour
    {
        private const int TargetFrameRate = 60;

        /// <summary>Raíz de la UI creada en el arranque.</summary>
        public UiRoot Ui { get; private set; }

        private void Awake()
        {
            Application.targetFrameRate = TargetFrameRate;
            Ui = UiRoot.Create(Camera.main);
        }

        private void OnDestroy()
        {
            if (Ui != null) Destroy(Ui.gameObject);
        }
    }
}

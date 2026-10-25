using UnityEngine;

namespace Gamespot.Core.Runtime.Navigation
{
    /// <summary>
    /// Una pantalla de la app (el equivalente a un widget de ruta de Flutter). Es un objeto C# normal:
    /// <see cref="Build"/> construye su UI bajo <see cref="Root"/> y el <see cref="Router"/> la muestra,
    /// la oculta y la destruye.
    /// </summary>
    public abstract class AppScreen
    {
        /// <summary>Raíz de la pantalla (estirada a la zona segura).</summary>
        public RectTransform Root { get; internal set; }

        /// <summary>Router que la muestra.</summary>
        public Router Router { get; internal set; }

        /// <summary>Ruta con la que se abrió (nula si se apiló con <see cref="Navigation.Router.Push"/>).</summary>
        public string Path { get; internal set; }

        /// <summary>Construye la UI. Se llama al mostrarla y al cambiar de idioma (tras vaciar <see cref="Root"/>).</summary>
        public abstract void Build(RectTransform root);

        /// <summary>Tras construirla y hacerla visible.</summary>
        public virtual void OnShow()
        {
        }

        /// <summary>Al quitarla (antes de destruirla): parar temporizadores y desuscribirse de eventos.</summary>
        public virtual void OnHide()
        {
        }

        /// <summary>
        /// Botón atrás del sistema. Devuelve verdadero si la pantalla lo ha gestionado (por ejemplo, abriendo
        /// el diálogo de abandonar la partida); si no, el router vuelve a la ruta padre.
        /// </summary>
        public virtual bool OnBack() => false;

        /// <summary>Se llama cada fotograma mientras es la pantalla visible (temporizadores de juego).</summary>
        public virtual void Tick(float deltaTime)
        {
        }
    }
}

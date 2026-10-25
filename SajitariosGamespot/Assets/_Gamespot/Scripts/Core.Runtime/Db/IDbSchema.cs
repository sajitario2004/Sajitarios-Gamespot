namespace Gamespot.Core.Runtime.Db
{
    /// <summary>
    /// Lo que cada juego aporta a la base de datos: sus tablas, sus migraciones y sus datos iniciales.
    /// El esquema vive en el juego, nunca en <c>Core</c> (regla de oro del registro de juegos).
    /// </summary>
    public interface IDbSchema
    {
        /// <summary>Crea las tablas del juego en una base de datos nueva.</summary>
        void OnCreate(IDb db);

        /// <summary>Aplica los pasos propios de <paramref name="oldVersion"/>+1 a <paramref name="newVersion"/>.</summary>
        void OnUpgrade(IDb db, int oldVersion, int newVersion);

        /// <summary>Carga los datos iniciales solo si sus tablas están vacías (idempotente).</summary>
        void SeedIfEmpty(IDb db, ISeedSource seeds);
    }
}

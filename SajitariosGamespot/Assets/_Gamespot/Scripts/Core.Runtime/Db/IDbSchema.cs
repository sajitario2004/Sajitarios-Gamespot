namespace Gamespot.Core.Runtime.Db
{
    /// <summary>
    /// Lo que cada juego aporta a la base de datos: sus tablas, sus migraciones y sus datos iniciales.
    /// El esquema vive en el juego, nunca en <c>Core</c> (regla de oro del registro de juegos).
    /// </summary>
    public interface IDbSchema
    {
        /// <summary>
        /// Asegura las tablas del juego. Se llama en CADA arranque, así que tiene que ser idempotente
        /// (<c>CREATE TABLE IF NOT EXISTS</c>, <c>CREATE INDEX IF NOT EXISTS</c>): así un juego añadido en una
        /// versión posterior de la app crea sus tablas en instalaciones existentes.
        /// </summary>
        void OnCreate(IDb db);

        /// <summary>
        /// Cambios en tablas que ya existían, de <paramref name="oldVersion"/>+1 a <paramref name="newVersion"/>
        /// (se llama después de <see cref="OnCreate"/>).
        /// </summary>
        void OnUpgrade(IDb db, int oldVersion, int newVersion);

        /// <summary>Carga los datos iniciales solo si sus tablas están vacías (idempotente).</summary>
        void SeedIfEmpty(IDb db, ISeedSource seeds);
    }
}

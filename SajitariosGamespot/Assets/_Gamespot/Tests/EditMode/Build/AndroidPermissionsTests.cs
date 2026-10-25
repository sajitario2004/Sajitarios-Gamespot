using System.Text.RegularExpressions;
using Gamespot.Editor.Setup;
using NUnit.Framework;

namespace Gamespot.Tests.EditMode.Build
{
    /// <summary>V059p: el manifiesto de los builds normales quita el permiso de internet (la app no lo usa).</summary>
    public sealed class AndroidPermissionsTests
    {
        private const string Manifest =
            "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" +
            "<manifest xmlns:android=\"http://schemas.android.com/apk/res/android\" package=\"com.unity3d.player\">\n" +
            "  <uses-permission android:name=\"android.permission.INTERNET\" />\n" +
            "  <uses-permission android:name=\"android.permission.VIBRATE\" />\n" +
            "  <application />\n" +
            "</manifest>\n";

        [Test]
        public void QuitaInternetYDejaLaMarcaParaLasLibrerias()
        {
            var result = AndroidPermissions.RemovePermission(Manifest, AndroidPermissions.Internet);
            StringAssert.Contains("xmlns:tools=\"http://schemas.android.com/tools\"", result);
            StringAssert.Contains("<uses-permission android:name=\"android.permission.INTERNET\" tools:node=\"remove\" />", result);
            StringAssert.Contains("android.permission.VIBRATE", result, "Los demás permisos se quedan");
            Assert.AreEqual(1, Regex.Matches(result, "android.permission.INTERNET").Count, "Solo queda la marca de quitar");
        }

        [Test]
        public void HacerloDosVecesDaLoMismo()
        {
            var once = AndroidPermissions.RemovePermission(Manifest, AndroidPermissions.Internet);
            Assert.AreEqual(once, AndroidPermissions.RemovePermission(once, AndroidPermissions.Internet));
        }
    }
}

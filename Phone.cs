using S1API.PhoneApp;
using S1API.UI;
using UnityEngine;

namespace ExampleMod.QuestTest
{

    /// <summary>
    /// Defines the MyAwesomeApp, a specialized application integrated into an in-game phone system.
    /// </summary>
    /// <remarks>
    /// This class leverages the PhoneApp framework to specify application-specific properties like name, title,
    /// icon label, and icon file name. It also overrides the method for defining the user interface layout upon creation.
    /// </remarks>
    public class MyAwesomeApp : PhoneApp
    {
        protected override string AppName => "test";
        protected override string AppTitle => "My test App";
        protected override string IconLabel => "test";
        protected override string IconFileName => "my_icon.png";

        protected override void OnCreatedUI(UnityEngine.GameObject container)
        {
            var panel = UIFactory.Panel("MainPanel", container.transform, Color.black);
        }
    }
}

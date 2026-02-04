using UnityEditor;
using UnityEngine;

namespace RuStore.Editor {

    public class PayClientSettings : RuStoreModuleSettings {

        [Header("Pay Client SDK")]
        public string consoleApplicationId;
        public readonly string internalConfigKey = "unity";
        public string deeplinkScheme;

        [MenuItem("Window/RuStoreSDK/Settings/PayClient")]
        public static void EditBillingClientSettings() {
            EditSettings<PayClientSettings>();
        }
    }
}

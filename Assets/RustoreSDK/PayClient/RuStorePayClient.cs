#nullable enable

using System;
using RuStore.CoreClient;
using RuStore.PayClient.Internal;
using System.Collections.Generic;
using UnityEngine;

namespace RuStore.PayClient {

    /// <summary>
    /// Класс реализует API для интегрирации платежей в мобильное приложение.
    /// </summary>
    public class RuStorePayClient {

        /// <summary>
        /// Версия плагина.
        /// </summary>
        public static string PluginVersion = "10.0.0";

        private static RuStorePayClient? instance;
        private AndroidJavaObject? clientWrapper { get; }

        /// <summary>
        /// Возвращает единственный экземпляр RuStorePayClient (реализация паттерна Singleton).
        /// Если экземпляр еще не создан, создает его.
        /// </summary>
        public static RuStorePayClient Instance {
            get {
                if (instance == null)
                    instance = new RuStorePayClient();

                return instance;
            }
        }

        private RuStorePayClient() {
            if (!IsPlatformSupported()) return;

            CallbackHandler.InitInstance();
            using (var clientJavaClass = new AndroidJavaClass("ru.rustore.unitysdk.payclient.RuStoreUnityPayClient")) {
                clientWrapper = clientJavaClass.GetStatic<AndroidJavaObject>("INSTANCE");
            }
        }

        /// <summary>
        /// Проверка статуса авторизации пользователя.
        /// </summary>
        /// <param name="onFailure">
        /// Действие, выполняемое в случае ошибки.
        /// Возвращает объект RuStore.RuStoreError с информацией об ошибке.
        /// </param>
        /// <param name="onSuccess">
        /// Действие, выполняемое при успешном завершении операции.
        /// Возвращает значение перечисленяи RuStore.PayClient.UserAuthorizationStatus с информцаией о статусе авторизации пользователя.
        /// </param>
        public void GetUserAuthorizationStatus(Action<RuStoreError> onFailure, Action<UserAuthorizationStatus> onSuccess) {
            if (!IsPlatformSupported(onFailure)) return;

            var listener = new UserAuthorizationStatusListener(onFailure, onSuccess);
            clientWrapper?.Call("getUserAuthorizationStatus", listener);
        }

        /// <summary>
        /// Проверка доступности платежей.
        /// Если все условия выполняются, возвращается RuStore.PayClient.PurchaseAvailabilityResult.isAvailable == true.
        /// В противном случае возвращается RuStore.PayClient.PurchaseAvailabilityResult.isAvailable == false.
        /// </summary>
        /// <param name="onFailure">
        /// Действие, выполняемое в случае ошибки.
        /// Возвращает объект RuStore.RuStoreError с информацией об ошибке.
        /// </param>
        /// <param name="onSuccess">
        /// Действие, выполняемое при успешном завершении операции.
        /// Возвращает объект RuStore.PayClient.PurchaseAvailabilityResult с информцаией о доступности оплаты.
        /// </param>
        public void GetPurchaseAvailability(Action<RuStoreError> onFailure, Action<PurchaseAvailabilityResult> onSuccess) {
            if (!IsPlatformSupported(onFailure)) return;

            var listener = new PurchaseAvailabilityListener(onFailure, onSuccess);
            clientWrapper?.Call("getPurchaseAvailability", listener);
        }

        /// <summary>
        /// Проверка установлен ли на устройстве пользователя RuStore.
        /// </summary>
        /// <returns>Возвращает true, если RuStore установлен, в противном случае — false.</returns>
        [Obsolete("Deprecated. Use RuStoreCore.Instance.IsRuStoreInstalled instead.")]
        public bool IsRuStoreInstalled() {
            if (!IsPlatformSupported()) return false;

            return RuStoreCoreClient.Instance.IsRuStoreInstalled();
        }

        /// <summary>
        /// Получение списка продуктов, добавленных в ваше приложение через RuStore консоль.
        /// </summary>
        /// <param name="productIds">Список идентификаторов продуктов (задаются при создании продукта в консоли разработчика).
        /// Список продуктов имеет ограничение в размере 1000 элементов.</param>
        /// <param name="onFailure">
        /// Действие, выполняемое в случае ошибки.
        /// Возвращает объект RuStore.RuStoreError с информацией об ошибке.
        /// </param>
        /// <param name="onSuccess">
        /// Действие, выполняемое при успешном завершении операции.
        /// Возвращает список объектов RuStore.PayClient.Product с информцаией о продуктах.
        /// </param>
        public void GetProducts(ProductId[] productsId, Action<RuStoreError> onFailure, Action<List<Product>> onSuccess) {
            if (!IsPlatformSupported(onFailure)) return;

            var ids = Array.ConvertAll(productsId, p => p.value);
            var listener = new ProductsResponseListener(onFailure, onSuccess);
            clientWrapper?.Call("getProducts", ids, listener);
        }

        /// <summary>
        /// Получение списка покупок пользователя.
        /// </summary>
        /// <param name="onFailure">
        /// Действие, выполняемое в случае ошибки.
        /// Возвращает объект RuStore.RuStoreError с информацией об ошибке.
        /// </param>
        /// <param name="onSuccess">
        /// Действие, выполняемое при успешном завершении операции.
        /// Возвращает список объектов RuStore.PayClient.Purchase с информцаией о покупках.
        /// </param>
        public void GetPurchases(Action<RuStoreError> onFailure, Action<List<IPurchase>> onSuccess) =>
            GetPurchases(null, null, onFailure, onSuccess);

        /// <summary>
        /// Получение списка покупок пользователя.
        /// </summary>
        /// <param name="productType">Тип продукта (необязательный параметр).</param>
        /// <param name="purchaseStatus">Статус покупки (необязательный параметр).</param>
        /// <param name="onFailure">
        /// Действие, выполняемое в случае ошибки.
        /// Возвращает объект RuStore.RuStoreError с информацией об ошибке.
        /// </param>
        /// <param name="onSuccess">
        /// Действие, выполняемое при успешном завершении операции.
        /// Возвращает список объектов RuStore.PayClient.Purchase с информцаией о покупках.
        /// </param>
        public void GetPurchases(ProductType? productType, Enum? purchaseStatus, Action<RuStoreError> onFailure, Action<List<IPurchase>> onSuccess) {
            if (!IsPlatformSupported(onFailure)) return;

            var _purchaseStatus = purchaseStatus != null
                ? $"{purchaseStatus.GetType().Name}.{purchaseStatus}"
                : null;

            var listener = new PurchasesResponseListener(onFailure, onSuccess);
            clientWrapper?.Call("getPurchases", productType?.ToString(), _purchaseStatus, listener);
        }

        /// <summary>
        /// Получение информации о покупке.
        /// </summary>
        /// <param name="purchaseId">
        /// Идентификатор продукта, который был присвоен продукту в RuStore Консоли.
        /// </param>
        /// <param name="onFailure">
        /// Действие, выполняемое в случае ошибки.
        /// Возвращает объект RuStore.RuStoreError с информацией об ошибке.
        /// </param>
        /// <param name="onSuccess">
        /// Действие, выполняемое при успешном завершении операции.
        /// Возвращает объект RuStore.PayClient.Purchase с информцаией о покупке.
        /// </param>
        public void GetPurchase(PurchaseId purchaseId, Action<RuStoreError> onFailure, Action<IPurchase> onSuccess) {
            if (!IsPlatformSupported(onFailure)) return;

            var listener = new PurchaseResponseListener(onFailure, onSuccess);
            clientWrapper?.Call("getPurchase", purchaseId.value, listener);
        }

        /// <summary>
        /// Покупка продукта.
        /// </summary>
        /// <param name="parameters">Параметры покупки продукта.</param>
        /// <param name="preferredPurchaseType">Предпочитаемый тип покупки – одностадийная (ONE_STEP) или двухстадийная (TWO_STEP).</param>
        /// <param name="onFailure">
        /// Действие, выполняемое в случае ошибки.
        /// Возвращает объект RuStoreError с информацией об ошибке.
        /// </param>
        /// <param name="onSuccess">
        /// Действие, выполняемое при успешном завершении операции.
        /// Возвращает объект ProductPurchaseResult с информцаией о результате покупки.
        /// </param>
        public void Purchase(ProductPurchaseParams parameters, PreferredPurchaseType preferredPurchaseType, Action<RuStoreError> onFailure, Action<ProductPurchaseResult> onSuccess) {
            if (!IsPlatformSupported(onFailure)) return;

            var listener = new ProductPurchaseResultListener(onFailure, onSuccess);
            clientWrapper?.Call(
                "purchase",
                parameters.productId.value,
                parameters.appUserEmail?.value,
                parameters.appUserId?.value,
                parameters.orderId?.value,
                parameters.quantity != null ? parameters.quantity.value : 1,
                parameters.developerPayload?.value,
                preferredPurchaseType.ToString(),
                listener);
        }

        /// <summary>
        /// Покупки продукта с двустадийной оплатой.
        /// </summary>
        /// <param name="parameters">Параметры покупки продукта.</param>
        /// <param name="onFailure">
        /// Действие, выполняемое в случае ошибки.
        /// Возвращает объект RuStoreError с информацией об ошибке.
        /// </param>
        /// <param name="onSuccess">
        /// Действие, выполняемое при успешном завершении операции.
        /// Возвращает объект ProductPurchaseResult с информацией о результате покупки.
        /// </param>
        public void PurchaseTwoStep(ProductPurchaseParams parameters, Action<RuStoreError> onFailure, Action<ProductPurchaseResult> onSuccess) {
            if (!IsPlatformSupported(onFailure)) return;

            var listener = new ProductPurchaseResultListener(onFailure, onSuccess);
            clientWrapper?.Call(
                "purchaseTwoStep",
                parameters.productId.value,
                parameters.appUserEmail?.value,
                parameters.appUserId?.value,
                parameters.orderId?.value,
                parameters.quantity != null ? parameters.quantity.value : 1,
                parameters.developerPayload?.value,
                listener);
        }

        /// <summary>
        /// Потребление (подтверждение) покупки. После вызова подтверждения покупка перейдёт в статус CONFIRMED.
        /// Запрос на потребление (подтверждение) покупки должен сопровождаться выдачей товара.
        /// </summary>
        /// <param name="purchaseId">Идентификатор покупки.</param>
        /// <param name="developerPayload">Строка, содержащая дополнительную информацию о заказе (необязательный параметр).</param>
        /// <param name="onFailure">
        /// Действие, выполняемое в случае ошибки.
        /// Возвращает объект RuStore.RuStoreError с информацией об ошибке.
        /// </param>
        /// <param name="onSuccess">
        /// Действие, выполняемое при успешном завершении операции.
        /// Возвращает объект наследник RuStore.PayClient.IProductPurchaseResult с информацией о результате покупки.
        /// </param>
        public void ConfirmTwoStepPurchase(PurchaseId purchaseId, DeveloperPayload? developerPayload, Action<RuStoreError> onFailure, Action onSuccess) {
            if (!IsPlatformSupported(onFailure)) return;

            var listener = new ConfirmTwoStepPurchaseResponseListener(onFailure, onSuccess);
            clientWrapper?.Call("confirmTwoStepPurchase", purchaseId.value, developerPayload?.value, listener);
        }

        /// <summary>
        /// Отмена покупки.
        /// Запрос на потребление (подтверждение) покупки должен сопровождаться выдачей товара.
        /// </summary>
        /// <param name="purchaseId">Идентификатор покупки.</param>
        /// <param name="onFailure">
        /// Действие, выполняемое в случае ошибки.
        /// Возвращает объект RuStore.RuStoreError с информацией об ошибке.
        /// </param>
        /// <param name="onSuccess">Действие, выполняемое при успешном завершении операции.</param>
        public void CancelTwoStepPurchase(PurchaseId purchaseId, Action<RuStoreError> onFailure, Action onSuccess) {
            if (!IsPlatformSupported(onFailure)) return;

            var listener = new CancelTwoStepPurchaseResponseListener(onFailure, onSuccess);
            clientWrapper?.Call("cancelTwoStepPurchase", purchaseId.value, listener);
        }

        private bool IsPlatformSupported(Action<RuStoreError>? onFailure = null) {
            if(Application.platform != RuntimePlatform.Android) {
                onFailure?.Invoke(new RuStoreError() {
                    name = "RuStorePayClientError",
                    description = "Unsupported platform"
                });
                return false;
            }

            return true;
        }
    }
}

using UnityEngine;
using System;

namespace RuStore.PayClient.Internal {

    public static partial class DataConverter {

        public static Product ConvertProduct(AndroidJavaObject obj) {
            if (obj == null) return null;

            var amountLabel = obj.Get<AndroidJavaObject>("amountLabel").Get<string>("value");
            var currency = obj.Get<AndroidJavaObject>("currency").Get<string>("value");
            var description = obj.Get<AndroidJavaObject>("description")?.Get<string>("value");
            var imageUrl = obj.Get<AndroidJavaObject>("imageUrl").Get<string>("value");
            var price = obj.Get<AndroidJavaObject>("price")?.Get<int>("value");
            var productId = obj.Get<AndroidJavaObject>("productId").Get<string>("value");
            var promoImageUrl = obj.Get<AndroidJavaObject>("promoImageUrl")?.Get<string>("value");
            var title = obj.Get<AndroidJavaObject>("title").Get<string>("value");
            var type = (ProductType)ConvertEnum<ProductType>(obj.Get<AndroidJavaObject>("type"));
            
            var product = new Product(
                new AmountLabel(amountLabel),
                new Currency(currency),
                description != null ? new Description(description) : null,
                new Url(imageUrl),
                price != null ? new Price((int)price) : null,
                new ProductId(productId),
                promoImageUrl != null ? new Url(imageUrl) : null,
                new Title(title),
                type
            );

            return product;
        }

        public static IPurchase ConvertPurchase(AndroidJavaObject obj) {
            if (obj == null) return null;

            using (var javaClass = obj.Call<AndroidJavaObject>("getClass")) {
                var className = javaClass.Call<string>("getName").Split('.');
                var type = className[className.Length - 1];

                switch (type) {
                    case "ProductPurchase":
                        return ConvertProductPurchase(obj);
                    case "SubscriptionPurchase":
                        return ConvertSubscriptionPurchase(obj);
                    default:
                        return null;
                }
            }
        }

        private static ProductPurchase ConvertProductPurchase(AndroidJavaObject obj) {
            var amountLabel = obj.Get<AndroidJavaObject>("amountLabel").Get<string>("value");
            var currency = obj.Get<AndroidJavaObject>("currency").Get<string>("value");
            var description = obj.Get<AndroidJavaObject>("description").Get<string>("value");
            var developerPayload = obj.Get<AndroidJavaObject>("developerPayload")?.Get<string>("value");
            var invoiceId = obj.Get<AndroidJavaObject>("invoiceId").Get<string>("value");
            var orderId = obj.Get<AndroidJavaObject>("orderId")?.Get<string>("value");
            var price = obj.Get<AndroidJavaObject>("price").Get<int>("value");
            var productId = obj.Get<AndroidJavaObject>("productId").Get<string>("value");
            var productType = (ProductType)ConvertEnum<ProductType>(obj.Get<AndroidJavaObject>("productType"));
            var purchaseId = obj.Get<AndroidJavaObject>("purchaseId").Get<string>("value");
            var purchaseTime = ConvertDateTime(obj.Get<AndroidJavaObject>("purchaseTime"));
            var purchaseType = (PurchaseType)ConvertEnum<PurchaseType>(obj.Get<AndroidJavaObject>("purchaseType"));
            var quantity = obj.Get<AndroidJavaObject>("quantity").Get<int>("value");
            var status = (ProductPurchaseStatus)ConvertEnum<ProductPurchaseStatus>(obj.Get<AndroidJavaObject>("status"));
            var sandbox = obj.Get<bool>("sandbox");

            var purchase = new ProductPurchase(
                new AmountLabel(amountLabel),
                new Currency(currency),
                new Description(description),
                developerPayload != null ? new DeveloperPayload(developerPayload) : null,
                new InvoiceId(invoiceId),
                orderId != null ? new OrderId(orderId) : null,
                new Price(price),
                new ProductId(productId),
                productType,
                new PurchaseId(purchaseId),
                purchaseTime,
                purchaseType,
                new Quantity(quantity),
                status,
                sandbox
            );

            return purchase;
        }

        private static SubscriptionPurchase ConvertSubscriptionPurchase(AndroidJavaObject obj) {
            var purchaseId = obj.Get<AndroidJavaObject>("purchaseId").Get<string>("value");
            var invoiceId = obj.Get<AndroidJavaObject>("invoiceId").Get<string>("value");
            var orderId = obj.Get<AndroidJavaObject>("orderId")?.Get<string>("value");
            var purchaseType = (PurchaseType)ConvertEnum<PurchaseType>(obj.Get<AndroidJavaObject>("purchaseType"));
            var status = (SubscriptionPurchaseStatus)ConvertEnum<SubscriptionPurchaseStatus>(obj.Get<AndroidJavaObject>("status"));
            var description = obj.Get<AndroidJavaObject>("description").Get<string>("value");
            var purchaseTime = ConvertDateTime(obj.Get<AndroidJavaObject>("purchaseTime"));
            var price = obj.Get<AndroidJavaObject>("price").Get<int>("value");
            var amountLabel = obj.Get<AndroidJavaObject>("amountLabel").Get<string>("value");
            var currency = obj.Get<AndroidJavaObject>("currency").Get<string>("value");
            var developerPayload = obj.Get<AndroidJavaObject>("developerPayload")?.Get<string>("value");
            var sandbox = obj.Get<bool>("sandbox");
            var productId = obj.Get<AndroidJavaObject>("productId").Get<string>("value");
            var expirationDate = ConvertDateTime(obj.Get<AndroidJavaObject>("expirationDate"));
            var gracePeriodEnabled = obj.Get<bool>("gracePeriodEnabled");

            var purchase = new SubscriptionPurchase(
                new PurchaseId(purchaseId),
                new InvoiceId(invoiceId),
                orderId != null ? new OrderId(orderId) : null,
                purchaseType,
                status,
                new Description(description),
                purchaseTime,
                new Price(price),
                new AmountLabel(amountLabel),
                new Currency(currency),
                developerPayload != null ? new DeveloperPayload(developerPayload) : null,
                sandbox,
                new ProductId(productId),
                expirationDate,
                gracePeriodEnabled
            );

            return purchase;
        }

        public static ProductPurchaseResult ConvertProductPurchaseResult(AndroidJavaObject obj) {
            var invoiceId = obj.Get<AndroidJavaObject>("invoiceId").Get<string>("value");
            var orderId = obj.Get<AndroidJavaObject>("orderId")?.Get<string>("value");
            var productId = obj.Get<AndroidJavaObject>("productId").Get<string>("value");
            var productType = (ProductType)ConvertEnum<ProductType>(obj.Get<AndroidJavaObject>("productType"));
            var purchaseId = obj.Get<AndroidJavaObject>("purchaseId").Get<string>("value");
            var purchaseType = (PurchaseType)ConvertEnum<PurchaseType>(obj.Get<AndroidJavaObject>("purchaseType"));
            var quantity = obj.Get<AndroidJavaObject>("quantity").Get<int>("value");
            var sandbox = obj.Get<bool>("sandbox");

            return new ProductPurchaseResult(
                new InvoiceId(invoiceId),
                orderId != null ? new OrderId(orderId) : null,
                new ProductId(productId),
                productType,
                new PurchaseId(purchaseId),
                purchaseType,
                new Quantity(quantity),
                sandbox
            );
        }

        public static T? ConvertEnum<T>(AndroidJavaObject obj) where T : struct {
            Type type = typeof(T);
            string strValue = obj?.Call<string>("toString");
            object enumValue;

            return Enum.TryParse(type, strValue, true, out enumValue) ? (T?)enumValue : null;
        }

        public static T ConvertEnumStrict<T>(AndroidJavaObject obj) where T : struct {
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            string strValue = obj.Call<string>("toString");

            return (T)Enum.Parse(typeof(T), strValue, true);
        }

        public static DateTime? ConvertDateTime(AndroidJavaObject obj) {
            DateTime? dateTime = null;
            if (obj != null) {
                long time = obj.Call<long>("getTime");
                dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(time);
            }

            return dateTime;
        }
    }
}

# Microsoft Dynamics 365 Fraud Protection - API examples
## Make a purchase – Existing user

Request that Dynamics 365 Fraud Protection make a risk recommendation when your existing customers try to make purchases. An existing user is typically a customer who logs into your business, whereas a guest user is a customer who typically has not registered with your business. The only difference in Dynamics 365 Fraud Protection between guest checkout and non-guest checkout is the user ID you send in the purchase event.

## Helpful links
- [Calling Dynamics 365 Fraud Protection](./Authenticate&#32;and&#32;call&#32;Fraud&#32;Protection.md)
- [Purchase - Data model and endpoint](https://apidocs.microsoft.com/services/dynamics365fraudprotection#/v1.0/V1.0MerchantservicesEventsPurchasePost)
- [Sample site - Make a purchase](../src/Web/Controllers/BasketController.cs) (see SetupPurchase and CheckoutDetails methods)
- [Sample site - Dynamics 365 Fraud Protection service](../src/Infrastructure/Services/FraudProtectionService.cs) (see PostPurchase method)

## Required data
- Purchase ID
- Merchant local date

## Optional data
- Basic user data (ID, name, email, phone, create/update date, etc.)
- Order details (tax, total, currency, etc.)
- Order line items
- Shipping address
- Payment instrument(s) details (credit card/PayPal/etc, billing addresses, etc.)
- Device info (IP, session ID, etc.)

**NOTES**
- Set the 'assessmentType' field based on if you plan to use the Dynamics 365 Fraud Protection risk recommendation:
  - Pass 'Evaluate' if you do not plan to use the Dynamics 365 Fraud Protection risk recommendation and are still evaluating Dynamics 365 Fraud Protection against your existing fraud solution.
  - Pass 'Protect' if you plan to use the Dynamics 365 Fraud Protection risk recommendation. If so, we must inform your bank about an incoming transaction via our Trusted MID program to potentially lift the bank acceptance rate. It also creates more accurate and detailed reports when we can distinguish between your 'Evaluate' and 'Protect' API calls.
- In the sample site, an existing user's ID is set to their email address which is also tied to how they log in to the sample site. Furthermore, the sample site ensures that no two customers have the same email address. This ensures user IDs will be unique for the sample site, as well as unique when sent to Dynamics 365 Fraud Protection. Decide what format to use for existing user IDs. It does not have to be an email address, but you should be careful not to duplicate user IDs.

## Example purchase request
```http
POST <Merchant API Endpoint>/v1.0/MerchantServices/events/Purchase HTTP/1.1
Host: <Merchant API Endpoint>
Authorization: bearer <token>
Content-Type: application/json; charset=utf-8
Content-Length: <content length>
x-ms-correlation-id: <correlation ID>

{
  "purchaseId": "<merchant mastered purchase ID>",
  "assessmentType": "<'Evaluate' or 'Protect' based on if you are using the Fraud Protection recommendation or not>",
  "customerLocalDate": "<customer local date in ISO 8601 format>",
  "merchantLocalDate": "<merchant local date in ISO 8601 format>",
  "totalAmount": 65.10,
  "salesTax": 5.10,
  "currency": "USD",
  "shippingMethod": "Standard",
  "user": {
    "userId": "<This demo application uses the existing user's email address, e.g. tami.shorts@microsoft.com>",
    "creationDate": "<user create date in ISO 8601 format>",
    "updateDate": "<user update date in ISO 8601 format>",
    "firstName": "Tami",
    "lastName": "Shorts",
    "country": "US",
    "zipCode": "98033",
    "timeZone": "Pacific Standard Time",
    "language": "EN-US",
    "phoneNumber": "+1-1234567890",
    "email": "tami.shorts@microsoft.com",
    "profileType": "Consumer"
  },
  "deviceContext": {
    "deviceContextId": "<Device Fingerprinting ID>",
    "ipAddress": "115.155.53.248",
    "provider": "DFPFingerPrinting"
  },
  "shippingAddress": {
    "firstName": "Tami",
    "lastName": "Shorts",
    "phoneNumber": "+1-1234567890",
    "street1": "123 State St",
    "city": "Bothell",
    "state": "WA",
    "zipCode": "98033",
    "country": "US"
  },
  "paymentInstrumentList": [
    {
      "purchaseAmount": 65.10,
      "merchantPaymentInstrumentId": "tami.shorts@microsoft.com-CreditCard",
      "type": "CreditCard",
      "creationDate": "<date credit card added to merchant's system in ISO 8601 format>",
      "state": "Active",
      "cardType": "Visa",
      "holderName": "Tami Shorts",
      "bin": "456978",
      "expirationDate": "03/21",
      "lastFourDigits": "6547",
      "billingAddress": {
        "firstName": "Tami",
        "lastName": "Shorts",
        "phoneNumber": "+1-1234567890",
        "street1": "123 State St",
        "city": "Bothell",
        "state": "WA",
        "zipCode": "98033",
        "country": "US"
      }
    }
  ],
  "productList": [
    {
      "productId": "1",
      "productName": ".NET Foundation Sweatshirt",
      "type": "Digital",
      "sku": "1",
      "category": "ClothingShoes",
      "market": "US",
      "salesPrice": 12.0,
      "currency": "USD",
      "cogs": 0.11,
      "isRecurring": false,
      "isFree": false,
      "language": "EN-US",
      "purchasePrice": 12.0,
      "margin": 2.1,
      "quantity": 2,
      "isPreorder": false,
      "shippingMethod": "Standard"
    },
    {
      "productId": "2",
      "productName": "Cup<T> White Mug",
      "type": "Digital",
      "sku": "2",
      "category": "HomeGarden",
      "market": "US",
      "salesPrice": 12.0,
      "currency": "USD",
      "cogs": 0.11,
      "isRecurring": false,
      "isFree": false,
      "language": "EN-US",
      "purchasePrice": 12.0,
      "margin": 2.1,
      "quantity": 3,
      "isPreorder": false,
      "shippingMethod": "Standard"
    }
  ],
  "_metadata": {
    "trackingId": "<tracking ID>",
    "merchantTimeStamp": "<event date in ISO 8601 format>"
  }
}
```
